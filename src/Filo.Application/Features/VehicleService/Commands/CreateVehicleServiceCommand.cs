using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleService.Commands;

public class VehicleServiceCreatedEvent : BaseEvent
{
    public VehicleServiceDto Service { get; }
    public VehicleServiceCreatedEvent(VehicleServiceDto service) => Service = service;
}

public sealed class CreateVehicleServiceCommand : IRequest<VehicleServiceDto>
{
    public int VehicleId { get; set; }
    public DateTime EntryDate { get; set; }
    public int Odometer { get; set; }
    public string ServiceCompany { get; set; } = string.Empty;
    public string FailureDescription { get; set; } = string.Empty;
}

public class CreateVehicleServiceCommandHandler : IRequestHandler<CreateVehicleServiceCommand, VehicleServiceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public CreateVehicleServiceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleServiceDto> Handle(CreateVehicleServiceCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
        }

        // Business Rule: Odometer validation against last fuel, maintenance, and previous service records
        var lastFuel = await _unitOfWork.VehicleFuels.GetLastFuelEntryAsync(request.VehicleId);
        if (lastFuel != null && request.Odometer < lastFuel.Odometer)
        {
            throw new ValidationException($"Girilen kilometre ({request.Odometer} KM), aracın son kaydedilen yakıt kilometresinden ({lastFuel.Odometer} KM) küçük olamaz.");
        }

        var lastMaintenance = await _unitOfWork.VehicleMaintenances.GetLastMaintenanceEntryAsync(request.VehicleId);
        if (lastMaintenance != null && request.Odometer < lastMaintenance.Odometer)
        {
            throw new ValidationException($"Girilen kilometre ({request.Odometer} KM), aracın son kaydedilen bakım kilometresinden ({lastMaintenance.Odometer} KM) küçük olamaz.");
        }

        var lastService = await _unitOfWork.VehicleServices.GetLastServiceEntryAsync(request.VehicleId);
        if (lastService != null && request.Odometer < lastService.Odometer)
        {
            throw new ValidationException($"Girilen kilometre ({request.Odometer} KM), aracın son kaydedilen servis kilometresinden ({lastService.Odometer} KM) küçük olamaz.");
        }

        var service = request.Adapt<Domain.Entities.VehicleService>();
        service.Status = "Aktif"; // Always default to Active upon entry

        await _unitOfWork.VehicleServices.AddAsync(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch back with details to get Vehicle properties populated
        var savedService = await _unitOfWork.VehicleServices.GetByIdAsync(service.Id);
        if (savedService == null)
        {
            throw new NotFoundException($"ID'si {service.Id} olan servis kaydı oluşturulamadı.");
        }
        var dto = savedService.Adapt<VehicleServiceDto>();

        service.AddDomainEvent(new VehicleServiceCreatedEvent(dto));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate paged list cache so new entry appears immediately
        for (int page = 1; page <= 5; page++)
            await _cacheService.RemoveAsync($"vehicleservices_paged_{page}_100");

        return dto;
    }
}
