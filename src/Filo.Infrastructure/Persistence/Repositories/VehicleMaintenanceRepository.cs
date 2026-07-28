using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleMaintenanceRepository : GenericRepository<VehicleMaintenance>, IVehicleMaintenanceRepository
{
    public VehicleMaintenanceRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleMaintenance?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleMaintenance> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleMaintenance, bool>>? predicate = null, 
        Func<IQueryable<VehicleMaintenance>, IOrderedQueryable<VehicleMaintenance>>? orderBy = null)
    {
        IQueryable<VehicleMaintenance> query = _dbSet
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
            query = query.OrderByDescending(x => x.MaintenanceDate).ThenByDescending(x => x.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<VehicleMaintenance?> GetLastMaintenanceEntryAsync(int vehicleId)
    {
        return await _dbSet
            .Where(x => x.VehicleId == vehicleId && !x.IsDeleted)
            .OrderByDescending(x => x.Odometer)
            .ThenByDescending(x => x.MaintenanceDate)
            .FirstOrDefaultAsync();
    }
}
