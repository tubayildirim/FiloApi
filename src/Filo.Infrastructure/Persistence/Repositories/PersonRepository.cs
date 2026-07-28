using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Filo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Filo.Infrastructure.Persistence.Repositories;

public class PersonRepository : GenericRepository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Person?> GetByTcknAsync(string tckn)
    {
        return await _dbSet.Include(p => p.Vehicles).FirstOrDefaultAsync(p => p.Tckn == tckn);
    }

    public override async Task<Person?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(p => p.Vehicles).FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<(IEnumerable<Person> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Person, bool>>? predicate = null, Func<IQueryable<Person>, IOrderedQueryable<Person>>? orderBy = null)
    {
        IQueryable<Person> query = _dbSet.Include(p => p.Vehicles);
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
            query = query.OrderBy(p => p.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
