using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Vehicles.Commands;

public class VehicleCreatedEvent : BaseEvent
{
    public VehicleDto Vehicle { get; }
    public VehicleCreatedEvent(VehicleDto vehicle) => Vehicle = vehicle;
}

public class CreateVehicleCommand : IRequest<VehicleDto>
{
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string PlateNumber { get; set; }
    public string? Color { get; set; }
    public string? FuelType { get; set; }
    public string? TransmissionType { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int? PersonId { get; set; }
}

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, VehicleDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Vehicles.GetByPlateNumberAsync(request.PlateNumber);
        if (existing != null)
        {
            throw new ValidationException($"{request.PlateNumber} plakalı bir araç zaten mevcut.");
        }

        var vehicle = request.Adapt<Vehicle>();
        
        await _unitOfWork.Vehicles.AddAsync(vehicle);
        
        var dto = vehicle.Adapt<VehicleDto>();
        vehicle.AddDomainEvent(new VehicleCreatedEvent(dto));
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vehicle.Adapt<VehicleDto>();
    }
}
