using MailKit.Net.Smtp;
using MimeKit;

namespace MiniAuth.Worker;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendCommentNotificationAsync(
        string toEmail,
        string postTitle,
        string commenterName,
        CancellationToken ct = default)
    {
        // Pega configs do appsettings.json
        var smtpHost = _configuration["Smtp:Host"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "1025");
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@miniauth.com";
        var fromName = _configuration["Smtp:FromName"] ?? "MiniAuth Blog";

        // MimeMessage = a carta do email (remetente, destinatário, assunto, corpo)
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Novo comentário no post \"{postTitle}\"";

        // Corpo do email em HTML
        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Novo comentário no seu post!</h2>
                <p><strong>{commenterName}</strong> comentou no post <em>"{postTitle}"</em>.</p>
                <p>Acesse a plataforma para ver o comentário.</p>
                <hr />
                <small>Este é um email automático do MiniAuth Blog.</small>
                """
        };

        // SmtpClient do MailKit (não confundir com System.Net.Mail.SmtpClient)
        using var client = new SmtpClient();

        try
        {
            // MailHog não usa SSL/TLS, então conectamos sem segurança (só dev!)
            await client.ConnectAsync(smtpHost, smtpPort, false, ct);
            await client.SendAsync(message, ct);

            _logger.LogInformation(
                "Email enviado para {Email} sobre post \"{Post}\"",
                toEmail, postTitle);
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }
}
