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

namespace Filo.Application.Features.VehicleFuel.Commands;

public class VehicleFuelCreatedEvent : BaseEvent
{
    public VehicleFuelDto Fuel { get; }
    public VehicleFuelCreatedEvent(VehicleFuelDto fuel) => Fuel = fuel;
}

public sealed class CreateVehicleFuelCommand : IRequest<VehicleFuelDto>
{
    public int VehicleId { get; set; }
    public DateTime RefuelingDate { get; set; }
    public int Odometer { get; set; }
    public double Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public string? ReceiptNumber { get; set; }
}

public class CreateVehicleFuelCommandHandler : IRequestHandler<CreateVehicleFuelCommand, VehicleFuelDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleFuelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleFuelDto> Handle(CreateVehicleFuelCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
        }

        // Business Rule: Odometer validation
        var lastEntry = await _unitOfWork.VehicleFuels.GetLastFuelEntryAsync(request.VehicleId);
        if (lastEntry != null && request.Odometer < lastEntry.Odometer)
        {
            throw new ValidationException($"Girilen kilometre ({request.Odometer} KM), aracın son kaydedilen yakıt kilometresinden ({lastEntry.Odometer} KM) küçük olamaz.");
        }

        var fuel = request.Adapt<Domain.Entities.VehicleFuel>();
        fuel.TotalPrice = (decimal)request.Liters * request.PricePerLiter;

        await _unitOfWork.VehicleFuels.AddAsync(fuel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch back with details to get Vehicle properties populated
        var savedFuel = await _unitOfWork.VehicleFuels.GetByIdAsync(fuel.Id);
        if (savedFuel == null)
        {
            throw new NotFoundException($"ID'si {fuel.Id} olan yakıt kaydı oluşturulamadı.");
        }
        var dto = savedFuel.Adapt<VehicleFuelDto>();

        fuel.AddDomainEvent(new VehicleFuelCreatedEvent(dto));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dto;
    }
}
