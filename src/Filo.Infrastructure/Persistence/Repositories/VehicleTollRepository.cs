using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleTollRepository : GenericRepository<VehicleToll>, IVehicleTollRepository
{
    public VehicleTollRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleToll?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleToll> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleToll, bool>>? predicate = null, 
        Func<IQueryable<VehicleToll>, IOrderedQueryable<VehicleToll>>? orderBy = null)
    {
        IQueryable<VehicleToll> query = _dbSet.Include(x => x.Vehicle);

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
            query = query.OrderByDescending(x => x.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
