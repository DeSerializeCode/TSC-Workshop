using WinFormsMain;

namespace WinFormsMain.VehicleLookup
{
    public class VehicleInfo
    {
        public string RegistrationNumber { get; init; } = string.Empty;

        public string State { get; init; } = string.Empty;

        public string Vin { get; init; } = string.Empty;

        public string Year { get; init; } = string.Empty;

        public string Make { get; init; } = string.Empty;

        public string Model { get; init; } = string.Empty;

        public string? Badge { get; init; } = string.Empty;

        public string BodyType { get; init; } = string.Empty;

        public string FuelType { get; init; } = string.Empty;

        public string Transmission { get; init; } = string.Empty;

        public string EngineCapacity { get; init; } = string.Empty;

        public string Drivetrain { get; init; } = string.Empty;

        public string Colour { get; init; } = string.Empty;

        public decimal? ConfidenceScore { get; init; }
            = null;

        public string Source { get; init; } = "motorweb";

        public Vehicle ToVehicle()
        {
            return new Vehicle
            {
                Registration = RegistrationNumber,
                State = State,
                Vin = Vin,
                Make = Make,
                Model = Model,
                Engine = string.IsNullOrWhiteSpace(EngineCapacity) ? EngineCapacity : $"{EngineCapacity} ({Drivetrain})",
                Transmission = Transmission,
                OwnerName = string.Empty,
                OwnerPhone = string.Empty,
            };
        }
    }
}
