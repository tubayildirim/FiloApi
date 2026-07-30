using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleInsuranceRepository : GenericRepository<VehicleInsurance>, IVehicleInsuranceRepository
{
    public VehicleInsuranceRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleInsurance?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleInsurance> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleInsurance, bool>>? predicate = null, 
        Func<IQueryable<VehicleInsurance>, IOrderedQueryable<VehicleInsurance>>? orderBy = null)
    {
        IQueryable<VehicleInsurance> query = _dbSet.Include(x => x.Vehicle);

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
