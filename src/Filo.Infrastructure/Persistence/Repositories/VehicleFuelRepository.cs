using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleFuelRepository : GenericRepository<VehicleFuel>, IVehicleFuelRepository
{
    public VehicleFuelRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleFuel?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleFuel> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleFuel, bool>>? predicate = null)
    {
        IQueryable<VehicleFuel> query = _dbSet
            .Include(x => x.Vehicle);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.RefuelingDate)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<VehicleFuel?> GetLastFuelEntryAsync(int vehicleId)
    {
        return await _dbSet
            .Where(x => x.VehicleId == vehicleId && !x.IsDeleted)
            .OrderByDescending(x => x.Odometer)
            .ThenByDescending(x => x.RefuelingDate)
            .FirstOrDefaultAsync();
    }
}
