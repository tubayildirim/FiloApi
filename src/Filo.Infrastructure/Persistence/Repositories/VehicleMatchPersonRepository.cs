using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Filo.Infrastructure.Persistence.Repositories;

public class VehicleMatchPersonRepository : GenericRepository<VehicleMatchPerson>, IVehicleMatchPersonRepository
{
    public VehicleMatchPersonRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<VehicleMatchPerson?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(x => x.Vehicle)
            .Include(x => x.Person)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<(IEnumerable<VehicleMatchPerson> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<VehicleMatchPerson, bool>>? predicate = null, 
        Func<IQueryable<VehicleMatchPerson>, IOrderedQueryable<VehicleMatchPerson>>? orderBy = null)
    {
        IQueryable<VehicleMatchPerson> query = _dbSet
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
            query = query.OrderBy(x => x.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
