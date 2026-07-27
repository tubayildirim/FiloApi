using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Commands;

public sealed class DeleteVehicleMatchPersonCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleMatchPersonCommand(int id) => Id = id;
}

public class DeleteVehicleMatchPersonCommandHandler : IRequestHandler<DeleteVehicleMatchPersonCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleMatchPersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleMatchPersonCommand request, CancellationToken cancellationToken)
    {
        var match = await _unitOfWork.VehicleMatches.GetByIdAsync(request.Id);
        if (match == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç-kişi ataması bulunamadı.");
        }

        // Dissociate vehicle's current driver if it matches the assigned person
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(match.VehicleId);
        if (vehicle != null && vehicle.PersonId == match.PersonId)
        {
            vehicle.PersonId = null;
            _unitOfWork.Vehicles.Update(vehicle);
        }

        _unitOfWork.VehicleMatches.Delete(match);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehiclematch:{request.Id}");

        // Invalidate paged and item caches
        for (int p = 1; p <= 5; p++)
        {
            await _cacheService.RemoveAsync($"vehiclematches_paged_{p}_10");
            await _cacheService.RemoveAsync($"vehiclematches_paged_{p}_50");
            await _cacheService.RemoveAsync($"vehiclematches_paged_{p}_100");

            await _cacheService.RemoveAsync($"vehicles_paged_{p}_10");
            await _cacheService.RemoveAsync($"vehicles_paged_{p}_50");
            await _cacheService.RemoveAsync($"vehicles_paged_{p}_100");

            await _cacheService.RemoveAsync($"persons_paged_{p}_10");
            await _cacheService.RemoveAsync($"persons_paged_{p}_50");
            await _cacheService.RemoveAsync($"persons_paged_{p}_100");
        }
        await _cacheService.RemoveAsync($"vehicle:{match.VehicleId}");
        await _cacheService.RemoveAsync($"person:{match.PersonId}");
    }
}
