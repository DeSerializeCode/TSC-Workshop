using System;
using System.Threading;
using System.Threading.Tasks;

namespace WinFormsMain.VehicleLookup
{
    public class VehicleLookupController
    {
        private readonly VehicleLookupService _service;

        public VehicleLookupController(VehicleLookupService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public Task<VehicleInfo?> GetVehicleByRegoAsync(string rego, string state, CancellationToken cancellationToken = default)
        {
            return _service.GetVehicleByRegoAsync(rego, state, cancellationToken);
        }
    }
}
