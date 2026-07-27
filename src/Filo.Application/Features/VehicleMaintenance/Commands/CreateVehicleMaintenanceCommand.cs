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

namespace Filo.Application.Features.VehicleMaintenance.Commands;

public class VehicleMaintenanceCreatedEvent : BaseEvent
{
    public VehicleMaintenanceDto Maintenance { get; }
    public VehicleMaintenanceCreatedEvent(VehicleMaintenanceDto maintenance) => Maintenance = maintenance;
}

public sealed class CreateVehicleMaintenanceCommand : IRequest<VehicleMaintenanceDto>
{
    public int VehicleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public int Odometer { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; set; }
    public int? NextMaintenanceKm { get; set; }
}

public class CreateVehicleMaintenanceCommandHandler : IRequestHandler<CreateVehicleMaintenanceCommand, VehicleMaintenanceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleMaintenanceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleMaintenanceDto> Handle(CreateVehicleMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
        }

        // Business Rule: Odometer validation against last fuel and last maintenance
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

        var maintenance = request.Adapt<Domain.Entities.VehicleMaintenance>();

        await _unitOfWork.VehicleMaintenances.AddAsync(maintenance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch back with details to get Vehicle properties populated
        var savedMaintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(maintenance.Id);
        if (savedMaintenance == null)
        {
            throw new NotFoundException($"ID'si {maintenance.Id} olan bakım kaydı oluşturulamadı.");
        }
        var dto = savedMaintenance.Adapt<VehicleMaintenanceDto>();

        maintenance.AddDomainEvent(new VehicleMaintenanceCreatedEvent(dto));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dto;
    }
}
