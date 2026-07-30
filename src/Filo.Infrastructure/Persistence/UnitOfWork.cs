using Filo.Domain.Interfaces;
using Filo.Infrastructure.Persistence.Repositories;

namespace Filo.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IVehicleRepository? _vehicles;
    private IPersonRepository? _person;
    private IVehicleMatchPersonRepository? _vehicleMatches;
    private IVehicleFuelRepository? _vehicleFuels;
    private IVehicleMaintenanceRepository? _vehicleMaintenances;
    private IVehicleServiceRepository? _vehicleServices;
    private IVehicleInsuranceRepository? _vehicleInsurances;
    private IVehicleTrafficFineRepository? _vehicleTrafficFines;
    private IVehicleTollRepository? _vehicleTolls;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);
    public IPersonRepository Person => _person ??= new PersonRepository(_context);
    public IVehicleMatchPersonRepository VehicleMatches => _vehicleMatches ??= new VehicleMatchPersonRepository(_context);
    public IVehicleFuelRepository VehicleFuels => _vehicleFuels ??= new VehicleFuelRepository(_context);
    public IVehicleMaintenanceRepository VehicleMaintenances => _vehicleMaintenances ??= new VehicleMaintenanceRepository(_context);
    public IVehicleServiceRepository VehicleServices => _vehicleServices ??= new VehicleServiceRepository(_context);
    public IVehicleInsuranceRepository VehicleInsurances => _vehicleInsurances ??= new VehicleInsuranceRepository(_context);
    public IVehicleTrafficFineRepository VehicleTrafficFines => _vehicleTrafficFines ??= new VehicleTrafficFineRepository(_context);
    public IVehicleTollRepository VehicleTolls => _vehicleTolls ??= new VehicleTollRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
