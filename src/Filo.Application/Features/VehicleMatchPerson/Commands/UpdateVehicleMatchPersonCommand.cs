using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Commands;

public sealed class UpdateVehicleMatchPersonCommand : IRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public int AssignmentKm { get; set; }
}

public class UpdateVehicleMatchPersonCommandHandler : IRequestHandler<UpdateVehicleMatchPersonCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateVehicleMatchPersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateVehicleMatchPersonCommand request, CancellationToken cancellationToken)
    {
        var match = await _unitOfWork.VehicleMatches.GetByIdAsync(request.Id);
        if (match == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç-kişi ataması bulunamadı.");
        }

        // Sync Vehicle.PersonId on change of vehicle or person in match
        if (match.VehicleId != request.VehicleId)
        {
            var oldVehicle = await _unitOfWork.Vehicles.GetByIdAsync(match.VehicleId);
            if (oldVehicle != null && oldVehicle.PersonId == match.PersonId)
            {
                oldVehicle.PersonId = null;
                _unitOfWork.Vehicles.Update(oldVehicle);
            }

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
            }
            vehicle.PersonId = request.PersonId;
            _unitOfWork.Vehicles.Update(vehicle);
        }
        else if (match.PersonId != request.PersonId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(match.VehicleId);
            if (vehicle != null)
            {
                vehicle.PersonId = request.PersonId;
                _unitOfWork.Vehicles.Update(vehicle);
            }
        }

        // Check if person exists
        if (match.PersonId != request.PersonId)
        {
            var person = await _unitOfWork.Person.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                throw new NotFoundException($"ID'si {request.PersonId} olan kişi bulunamadı.");
            }
        }

        request.Adapt(match);

        _unitOfWork.VehicleMatches.Update(match);
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
        await _cacheService.RemoveAsync($"vehicle:{request.VehicleId}");
        await _cacheService.RemoveAsync($"person:{match.PersonId}");
        await _cacheService.RemoveAsync($"person:{request.PersonId}");
    }
}
