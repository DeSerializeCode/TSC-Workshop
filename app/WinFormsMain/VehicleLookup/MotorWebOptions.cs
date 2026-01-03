using System.ComponentModel.DataAnnotations;

namespace WinFormsMain.VehicleLookup
{
    public class MotorWebOptions
    {
        public const string SectionName = "MotorWeb";

        [Url]
        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }
}
