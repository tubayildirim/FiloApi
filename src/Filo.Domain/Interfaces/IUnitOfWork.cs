namespace Filo.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVehicleRepository Vehicles { get; }
    IPersonRepository Person { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
