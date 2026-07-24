using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Filo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetByPlateNumberAsync(string plateNumber)
    {
        return await _dbSet.Include(v => v.Person).FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
    }

    public override async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(v => v.Person).FirstOrDefaultAsync(v => v.Id == id);
    }

    public override async Task<(IEnumerable<Vehicle> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Vehicle, bool>>? predicate = null)
    {
        IQueryable<Vehicle> query = _dbSet.Include(v => v.Person);
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
