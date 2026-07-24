using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Vehicles.Commands;

public class UpdateVehicleCommand : IRequest
{
    public int Id { get; set; }
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

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateVehicleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç bulunamadı.");
        }

        if (vehicle.PlateNumber != request.PlateNumber)
        {
            var existing = await _unitOfWork.Vehicles.GetByPlateNumberAsync(request.PlateNumber);
            if (existing != null)
            {
                throw new ValidationException($"{request.PlateNumber} plakalı bir araç zaten mevcut.");
            }
        }

        request.Adapt(vehicle);

        _unitOfWork.Vehicles.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"vehicle:{request.Id}");
    }
}
