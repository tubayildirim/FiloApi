using Filo.Domain.Interfaces;
using Filo.Infrastructure.Persistence.Repositories;

namespace Filo.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IVehicleRepository? _vehicles;
    private IPersonRepository? _person;
    private IVehicleMatchPersonRepository? _vehicleMatches;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);
    public IPersonRepository Person => _person ??= new PersonRepository(_context);
    public IVehicleMatchPersonRepository VehicleMatches => _vehicleMatches ??= new VehicleMatchPersonRepository(_context);

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
