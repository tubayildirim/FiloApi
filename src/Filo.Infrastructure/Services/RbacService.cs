using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Filo.Application.Common.Interfaces;
using Filo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Filo.Infrastructure.Services;

public class RbacService : IRbacService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceScopeFactory _scopeFactory;

    public RbacService(ICurrentUserService currentUserService, IServiceScopeFactory scopeFactory)
    {
        _currentUserService = currentUserService;
        _scopeFactory = scopeFactory;
    }

    public async Task<List<int>?> GetAllowedVehicleIdsAsync()
    {
        if (_currentUserService.IsAdmin || _currentUserService.UserId == null)
        {
            return null; // All allowed
        }

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (_currentUserService.IsStaff)
        {
            var matches = await unitOfWork.VehicleMatches.GetPagedAsync(1, 1000, m => m.PersonId == _currentUserService.UserId.Value, null);
            return matches.Items.Select(m => m.VehicleId).Distinct().ToList();
        }

        if (_currentUserService.IsManager)
        {
            var subordinates = await unitOfWork.Person.GetPagedAsync(1, 1000, p => p.ManagerId == _currentUserService.UserId.Value, null);
            var subIds = subordinates.Items.Select(p => p.Id).ToList();
            
            var matches = await unitOfWork.VehicleMatches.GetPagedAsync(1, 10000, m => subIds.Contains(m.PersonId), null);
            return matches.Items.Select(m => m.VehicleId).Distinct().ToList();
        }

        return new List<int>(); // No access
    }

    public async Task<List<int>?> GetAllowedPersonIdsAsync()
    {
        if (_currentUserService.IsAdmin || _currentUserService.UserId == null)
        {
            return null; // All allowed
        }

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (_currentUserService.IsStaff)
        {
            return new List<int> { _currentUserService.UserId.Value };
        }

        if (_currentUserService.IsManager)
        {
            var subordinates = await unitOfWork.Person.GetPagedAsync(1, 1000, p => p.ManagerId == _currentUserService.UserId.Value, null);
            var subIds = subordinates.Items.Select(p => p.Id).ToList();
            subIds.Add(_currentUserService.UserId.Value); // Manager can see themselves
            return subIds;
        }

        return new List<int>(); // No access
    }
}
