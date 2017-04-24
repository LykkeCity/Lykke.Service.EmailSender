using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.WebServices.EmailSender.Settings
{
    public class PartnerSmtpSettings : TableEntity
    {
        public string SmtpHost { get; set; }
        public int? Port { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string FromDisplayName { get; set; }
        public string FromEmailAddress { get; set; }
    }
}