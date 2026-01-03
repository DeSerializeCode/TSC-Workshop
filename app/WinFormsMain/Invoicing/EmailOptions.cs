using System.ComponentModel.DataAnnotations;

namespace WinFormsMain.Invoicing
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        [Required]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 587;

        [Required]
        public string FromAddress { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;
    }
}
