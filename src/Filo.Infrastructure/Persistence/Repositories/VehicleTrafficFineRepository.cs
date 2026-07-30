using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleTrafficFineRepository : GenericRepository<VehicleTrafficFine>, IVehicleTrafficFineRepository
{
    public VehicleTrafficFineRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleTrafficFine?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .Include(x => x.Person)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleTrafficFine> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleTrafficFine, bool>>? predicate = null, 
        Func<IQueryable<VehicleTrafficFine>, IOrderedQueryable<VehicleTrafficFine>>? orderBy = null)
    {
        IQueryable<VehicleTrafficFine> query = _dbSet
            .Include(x => x.Vehicle)
            .Include(x => x.Person);

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
