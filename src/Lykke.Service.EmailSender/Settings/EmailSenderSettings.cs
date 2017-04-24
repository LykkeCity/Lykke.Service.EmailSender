using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.EmailSender.Settings
{
    public class EmailSenderSettings
    {
        public AzureTableSettings Log { get; set; }

        [Required(AllowEmptyStrings = false)]
        public AzureTableSettings Partners { get; set; }
    }

    public class AzureTableSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string ConnectionString { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TableName { get; set; }
    }
}