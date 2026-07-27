using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Commands;

public class VehicleMatchPersonCreatedEvent : BaseEvent
{
    public VehicleMatchPersonDto Match { get; }
    public VehicleMatchPersonCreatedEvent(VehicleMatchPersonDto match) => Match = match;
}

public sealed class CreateVehicleMatchPersonCommand : IRequest<VehicleMatchPersonDto>
{
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public int AssignmentKm { get; set; }
}

public class CreateVehicleMatchPersonCommandHandler : IRequestHandler<CreateVehicleMatchPersonCommand, VehicleMatchPersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public CreateVehicleMatchPersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleMatchPersonDto> Handle(CreateVehicleMatchPersonCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
        }

        var inService = await _unitOfWork.VehicleServices.IsVehicleInServiceAsync(request.VehicleId);
        if (inService)
        {
            throw new ValidationException("Araç şu anda serviste olduğu için yeni bir sürücü ataması yapılamaz.");
        }

        var person = await _unitOfWork.Person.GetByIdAsync(request.PersonId);
        if (person == null)
        {
            throw new NotFoundException($"ID'si {request.PersonId} olan kişi bulunamadı.");
        }

        // Update vehicle's current driver
        vehicle.PersonId = request.PersonId;
        _unitOfWork.Vehicles.Update(vehicle);

        var match = request.Adapt<Domain.Entities.VehicleMatchPerson>();
        
        await _unitOfWork.VehicleMatches.AddAsync(match);
        
        // Save database first to populate navigation properties or map them
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch back with details to map full DTO
        var savedMatch = await _unitOfWork.VehicleMatches.GetByIdAsync(match.Id);
        if (savedMatch == null)
        {
            throw new NotFoundException($"ID'si {match.Id} olan araç-kişi ataması oluşturulamadı.");
        }
        var dto = savedMatch.Adapt<VehicleMatchPersonDto>();

        match.AddDomainEvent(new VehicleMatchPersonCreatedEvent(dto));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate paged and item caches to ensure UI shows the new match immediately
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
        await _cacheService.RemoveAsync($"vehicle:{request.VehicleId}");
        await _cacheService.RemoveAsync($"person:{request.PersonId}");

        return dto;
    }
}
