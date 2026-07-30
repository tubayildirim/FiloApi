namespace Filo.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVehicleRepository Vehicles { get; }
    IPersonRepository Person { get; }
    IVehicleMatchPersonRepository VehicleMatches { get; }
    IVehicleFuelRepository VehicleFuels { get; }
    IVehicleMaintenanceRepository VehicleMaintenances { get; }
    IVehicleServiceRepository VehicleServices { get; }
    IVehicleInsuranceRepository VehicleInsurances { get; }
    IVehicleTrafficFineRepository VehicleTrafficFines { get; }
    IVehicleTollRepository VehicleTolls { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
