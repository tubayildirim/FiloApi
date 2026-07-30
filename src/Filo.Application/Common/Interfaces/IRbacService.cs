using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filo.Application.Common.Interfaces;

public interface IRbacService
{
    Task<List<int>?> GetAllowedVehicleIdsAsync();
}
