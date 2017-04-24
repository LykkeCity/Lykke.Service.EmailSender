using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.EmailSender.Models
{
    public class EmailSendRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string FromPartnerId { get; set; }

        [Required]
        public EmailAddressee To { get; set; }

        [Required]
        public EmailMessage Message { get; set; }
    }

    public class EmailAddressee
    {
        [Required(AllowEmptyStrings = false)]
        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }
    }
}
