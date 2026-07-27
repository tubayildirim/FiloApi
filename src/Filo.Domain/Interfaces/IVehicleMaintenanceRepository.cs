using Filo.Domain.Entities;
using System.Threading.Tasks;

namespace Filo.Domain.Interfaces;

public interface IVehicleMaintenanceRepository : IGenericRepository<VehicleMaintenance>
{
    Task<VehicleMaintenance?> GetLastMaintenanceEntryAsync(int vehicleId);
}
