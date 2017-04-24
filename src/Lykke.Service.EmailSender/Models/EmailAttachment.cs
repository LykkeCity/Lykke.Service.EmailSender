namespace Lykke.WebServices.EmailSender.Models
{
    public class EmailAttachment
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
    }
}