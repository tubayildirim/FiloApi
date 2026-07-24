using Filo.Domain.Entities;

namespace Filo.Domain.Interfaces;

public interface IPersonRepository : IGenericRepository<Person>
{
    Task<Person?> GetByTcknAsync(string tckn);
}
