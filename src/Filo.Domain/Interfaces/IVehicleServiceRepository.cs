using Filo.Domain.Entities;
using System.Threading.Tasks;

namespace Filo.Domain.Interfaces;

public interface IVehicleServiceRepository : IGenericRepository<VehicleService>
{
    Task<VehicleService?> GetLastServiceEntryAsync(int vehicleId);
    Task<bool> IsVehicleInServiceAsync(int vehicleId);
}
