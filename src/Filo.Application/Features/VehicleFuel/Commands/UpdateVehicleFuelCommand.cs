using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleFuel.Commands;

public sealed class UpdateVehicleFuelCommand : IRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime RefuelingDate { get; set; }
    public int Odometer { get; set; }
    public double Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public string? ReceiptNumber { get; set; }
}

public class UpdateVehicleFuelCommandHandler : IRequestHandler<UpdateVehicleFuelCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateVehicleFuelCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateVehicleFuelCommand request, CancellationToken cancellationToken)
    {
        var fuel = await _unitOfWork.VehicleFuels.GetByIdAsync(request.Id);
        if (fuel == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan yakıt kaydı bulunamadı.");
        }

        if (fuel.VehicleId != request.VehicleId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
            }
        }

        request.Adapt(fuel);
        fuel.TotalPrice = (decimal)request.Liters * request.PricePerLiter;

        _unitOfWork.VehicleFuels.Update(fuel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehiclefuel:{request.Id}");
    }
}
