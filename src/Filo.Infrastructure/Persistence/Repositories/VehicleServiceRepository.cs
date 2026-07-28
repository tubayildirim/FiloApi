using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleServiceRepository : GenericRepository<VehicleService>, IVehicleServiceRepository
{
    public VehicleServiceRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleService?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleService> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleService, bool>>? predicate = null, 
        Func<IQueryable<VehicleService>, IOrderedQueryable<VehicleService>>? orderBy = null)
    {
        IQueryable<VehicleService> query = _dbSet
            .Include(x => x.Vehicle);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync();
        
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderByDescending(x => x.EntryDate).ThenByDescending(x => x.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<VehicleService?> GetLastServiceEntryAsync(int vehicleId)
    {
        return await _dbSet
            .Where(x => x.VehicleId == vehicleId && !x.IsDeleted)
            .OrderByDescending(x => x.Odometer)
            .ThenByDescending(x => x.EntryDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsVehicleInServiceAsync(int vehicleId)
    {
        return await _dbSet
            .AnyAsync(x => x.VehicleId == vehicleId && x.Status == "Aktif" && !x.IsDeleted);
    }
}
