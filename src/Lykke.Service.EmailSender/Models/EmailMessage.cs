using System.ComponentModel.DataAnnotations;

namespace Lykke.WebServices.EmailSender.Models
{
    public class EmailMessage
    {
        [Required(AllowEmptyStrings = false)]
        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string TextBody { get; set; }

        public EmailAttachment[] Attachments { get; set; }
    }
}
