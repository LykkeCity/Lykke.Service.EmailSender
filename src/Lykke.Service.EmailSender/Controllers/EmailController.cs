﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.WebServices.EmailSender.Models;
using Lykke.WebServices.EmailSender.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Lykke.WebServices.EmailSender.Controllers
{
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        private readonly INoSQLTableStorage<PartnerSmtpSettings> _partnerSmtpSettings;

        public EmailController(INoSQLTableStorage<PartnerSmtpSettings> partnerSmtpSettings)
        {
            _partnerSmtpSettings = partnerSmtpSettings;
        }

        [HttpPost]
        [RequireHttps]
        public async Task Send(EmailSendRequest request)
        {
            if (!TryValidateModel(request))
                throw new ArgumentException(nameof(request));

            var partnerSmtpSettings = _partnerSmtpSettings[request.FromPartnerId, "smtp"];

            var message = new MimeMessage
            {
                Subject = request.Message.Subject
            };

            message.From.Add(new MailboxAddress(Encoding.UTF8, partnerSmtpSettings.FromDisplayName, partnerSmtpSettings.FromEmailAddress));
            message.To.Add(new MailboxAddress(Encoding.UTF8, request.To.DisplayName, request.To.EmailAddress));

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = request.Message.TextBody;
            bodyBuilder.HtmlBody = request.Message.HtmlBody;

            if (null != request.Message.Attachments)
                foreach (var attachment in request.Message.Attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(ParserOptions.Default, attachment.MimeType));
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
    }
}