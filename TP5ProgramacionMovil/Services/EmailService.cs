using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TP5ProgramacionMovil.Services
{
    public interface IEmailService
    {
        Task EnviarReporteAsync(
            string destinatario,
            string asunto,
            string mensaje,
            byte[] archivo,
            string nombreArchivo);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarReporteAsync(
            string destinatario,
            string asunto,
            string mensaje,
            byte[] archivo,
            string nombreArchivo)
        {
            var email = new MimeMessage();

            var remitente =
                _configuration["Email:SmtpUser"]
                ?? throw new InvalidOperationException(
                    "No se configuró Email:SmtpUser.");

            email.From.Add(
                new MailboxAddress(
                    "Sistema de Ventas",
                    remitente
                )
            );

            email.To.Add(
                MailboxAddress.Parse(destinatario)
            );

            email.Subject = asunto;

            var body = new BodyBuilder
            {
                TextBody = mensaje
            };

            body.Attachments.Add(
                nombreArchivo,
                archivo,
                ContentType.Parse("application/pdf")
            );

            email.Body = body.ToMessageBody();

            var host =
                _configuration["Email:SmtpHost"]
                ?? throw new InvalidOperationException(
                    "No se configuró Email:SmtpHost.");

            var password =
                _configuration["Email:SmtpPassword"]
                ?? throw new InvalidOperationException(
                    "No se configuró Email:SmtpPassword.");

            var puerto =
                int.Parse(
                    _configuration["Email:SmtpPort"]
                    ?? "587"
                );

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                host,
                puerto,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                remitente,
                password
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}