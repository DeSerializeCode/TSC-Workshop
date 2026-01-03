using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WinFormsMain.VehicleLookup
{
    public class VehicleLookupService
    {
        private readonly MotorWebClient _client;
        private readonly ILogger<VehicleLookupService> _logger;

        public VehicleLookupService(MotorWebClient client, ILogger<VehicleLookupService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<VehicleInfo?> GetVehicleByRegoAsync(string rego, string state, CancellationToken cancellationToken = default)
        {
            var payload = await _client.LookupAsync(rego, state, cancellationToken).ConfigureAwait(false);

            if (payload is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(payload.RegistrationNumber) || string.IsNullOrWhiteSpace(payload.State))
            {
                _logger.LogWarning("MotorWeb returned partial vehicle data for {Rego} ({State})", rego, state);
                return null;
            }

            return new VehicleInfo
            {
                RegistrationNumber = payload.RegistrationNumber,
                State = payload.State,
                Vin = payload.Vin,
                Year = payload.Year,
                Make = payload.Make,
                Model = payload.Model,
                Badge = payload.Badge,
                BodyType = payload.BodyType,
                FuelType = payload.FuelType,
                Transmission = payload.Transmission,
                EngineCapacity = payload.EngineCapacity,
                Drivetrain = payload.Drivetrain,
                Colour = payload.Colour,
                ConfidenceScore = payload.ConfidenceScore,
            };
        }
    }
}
