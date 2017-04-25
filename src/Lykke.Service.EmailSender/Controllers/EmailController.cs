using System;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.Service.EmailSender.Models;
using Lykke.Service.EmailSender.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Lykke.Service.EmailSender.Controllers
{
    [Route("api/[controller]/[action]")]
    public class EmailController : Controller
    {
        private readonly INoSQLTableStorage<PartnerSmtpSettings> _partnerSmtpSettings;
        private readonly ILog _log;

        public EmailController(INoSQLTableStorage<PartnerSmtpSettings> partnerSmtpSettings, ILog log)
        {
            _partnerSmtpSettings = partnerSmtpSettings;
            _log = log;
        }

        [HttpPost]
        public async Task Send(EmailSendRequest request)
        {
            if (!TryValidateModel(request))
                throw new ArgumentException(nameof(request));

            try
            {
                var partnerSmtpSettings = _partnerSmtpSettings[request.FromPartnerId, "smtp"];

                var message = new MimeMessage
                {
                    Subject = request.Message.Subject
                };

                message.From.Add(new MailboxAddress(Encoding.UTF8, partnerSmtpSettings.FromDisplayName,
                    partnerSmtpSettings.FromEmailAddress));
                message.To.Add(new MailboxAddress(Encoding.UTF8, request.To.DisplayName, request.To.EmailAddress));

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = request.Message.TextBody,
                    HtmlBody = request.Message.HtmlBody
                };

                if (null != request.Message.Attachments)
                    foreach (var attachment in request.Message.Attachments)
                    {
                        bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content,
                            ContentType.Parse(ParserOptions.Default, attachment.MimeType));
                    }

                message.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(partnerSmtpSettings.SmtpHost,
                        partnerSmtpSettings.Port ?? 0,
                        partnerSmtpSettings.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

                    await smtpClient.AuthenticateAsync(partnerSmtpSettings.UserName, partnerSmtpSettings.Password);

                    await smtpClient.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
                await _log.WriteFatalErrorAsync(nameof(EmailSender), nameof(Startup), nameof(Send), ex, DateTime.UtcNow);
                throw;
            }
        }
    }
}
