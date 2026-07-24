using Filo.Domain.Entities;

namespace Filo.Domain.Interfaces;

public interface IVehicleRepository : IGenericRepository<Vehicle>
{
    Task<Vehicle?> GetByPlateNumberAsync(string plateNumber);
}
