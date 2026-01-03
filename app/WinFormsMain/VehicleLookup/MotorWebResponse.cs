using System.Text.Json.Serialization;

namespace WinFormsMain.VehicleLookup
{
    public class MotorWebResponse
    {
        [JsonPropertyName("registrationNumber")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("vin")]
        public string Vin { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public string Year { get; set; } = string.Empty;

        [JsonPropertyName("make")]
        public string Make { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("badge")]
        public string? Badge { get; set; }
            = string.Empty;

        [JsonPropertyName("bodyType")]
        public string BodyType { get; set; } = string.Empty;

        [JsonPropertyName("fuelType")]
        public string FuelType { get; set; } = string.Empty;

        [JsonPropertyName("transmission")]
        public string Transmission { get; set; } = string.Empty;

        [JsonPropertyName("engineCapacity")]
        public string EngineCapacity { get; set; } = string.Empty;

        [JsonPropertyName("drivetrain")]
        public string Drivetrain { get; set; } = string.Empty;

        [JsonPropertyName("colour")]
        public string Colour { get; set; } = string.Empty;

        [JsonPropertyName("confidenceScore")]
        public decimal? ConfidenceScore { get; set; }
            = null;
    }
}
