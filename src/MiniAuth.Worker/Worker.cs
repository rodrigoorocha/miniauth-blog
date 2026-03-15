using System.Text;
using System.Text.Json;
using MiniAuth.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MiniAuth.Worker;

// BackgroundService = serviço que roda "pra sempre" em background
// Ele inicia quando a aplicação sobe e para quando a aplicação morre
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "comment-created";

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        EmailService emailService)
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    // Chamado quando o Worker inicia
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Conecta no RabbitMQ (igual ao Publisher, mas aqui é o Consumer)
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "miniauth",
            Password = _configuration["RabbitMQ:Password"] ?? "miniauth123",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // 2. Garante que a fila existe (mesmo pattern do Publisher)
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Processa uma mensagem por vez (não pega a próxima até terminar)
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        // 3. Cria o "listener" — fica escutando a fila
        var consumer = new EventingBasicConsumer(_channel);

        // 4. Quando chega uma mensagem, executa esse callback
        consumer.Received += async (sender, args) =>
        {
            try
            {
                // Converte os bytes de volta pra JSON string
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                // Deserializa pro nosso evento tipado
                var evt = JsonSerializer.Deserialize<CommentCreatedEvent>(json);

                if (evt is null)
                {
                    _logger.LogWarning("Evento recebido mas não conseguiu deserializar");
                    _channel.BasicAck(args.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "Evento recebido: comentário {CommentId} no post \"{PostTitle}\"",
                    evt.CommentId, evt.PostTitle);

                // Só envia email se o autor do post tem email
                if (!string.IsNullOrEmpty(evt.PostAuthorEmail))
                {
                    await _emailService.SendCommentNotificationAsync(
                        evt.PostAuthorEmail,
                        evt.PostTitle,
                        evt.CommenterName,
                        stoppingToken);
                }

                // ACK = "mensagem processada, pode remover da fila"
                _channel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila");

                // NACK = "deu erro, recoloca na fila pra tentar de novo"
                _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        // 5. Começa a consumir — a partir daqui, o callback acima é chamado
        //    autoAck: false = nós controlamos quando confirmar (manual ACK)
        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Worker escutando a fila \"{Queue}\"...", QueueName);

        // Mantém o worker vivo até o cancelamento
        return Task.CompletedTask;
    }

    // Cleanup quando o Worker para
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
