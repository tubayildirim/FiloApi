using Filo.Domain.Entities;
using System.Threading.Tasks;

namespace Filo.Domain.Interfaces;

public interface IVehicleFuelRepository : IGenericRepository<VehicleFuel>
{
    Task<VehicleFuel?> GetLastFuelEntryAsync(int vehicleId);
}
