using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniAuth.Domain.Events;
using MiniAuth.Domain.Interfaces;
using RabbitMQ.Client;

namespace MiniAuth.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = configuration["RabbitMQ:UserName"] ?? "miniauth",
            Password = configuration["RabbitMQ:Password"] ?? "miniauth123",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<T>(T domainEvent, CancellationToken ct = default) where T : DomainEvent
    {
        // Nome da fila baseado no tipo do evento:
        // CommentCreatedEvent → "comment-created"
        var queueName = GetQueueName(typeof(T).Name);

        // Declara a fila (se já existe, não faz nada)
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,       // sobrevive restart do RabbitMQ
            exclusive: false,    // outros consumers podem escutar
            autoDelete: false    // não deleta quando não tem consumer
        );

        // Serializa o evento pra JSON e converte pra bytes
        var json = JsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(json);

        // Marca a mensagem como persistente (sobrevive restart)
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        // Publica na fila
        _channel.BasicPublish(
            exchange: string.Empty,  // default exchange
            routingKey: queueName,   // nome da fila
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation(
            "Evento {EventType} publicado na fila {Queue}",
            typeof(T).Name, queueName
        );

        return Task.CompletedTask;
    }

    // CommentCreatedEvent → "comment-created"
    private static string GetQueueName(string eventTypeName)
    {
        // Remove "Event" do final e converte PascalCase pra kebab-case
        var name = eventTypeName.Replace("Event", "");
        var result = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                result.Append('-');
            result.Append(char.ToLower(name[i]));
        }

        return result.ToString();
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
