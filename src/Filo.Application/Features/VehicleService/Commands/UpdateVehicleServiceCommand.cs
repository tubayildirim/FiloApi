using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleService.Commands;

public sealed class UpdateVehicleServiceCommand : IRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public int Odometer { get; set; }
    public string ServiceCompany { get; set; } = string.Empty;
    public string FailureDescription { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public string Status { get; set; } = "Aktif";
    public string? InvoiceNumber { get; set; }
}

public class UpdateVehicleServiceCommandHandler : IRequestHandler<UpdateVehicleServiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateVehicleServiceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateVehicleServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.VehicleServices.GetByIdAsync(request.Id);
        if (service == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan servis kaydı bulunamadı.");
        }

        if (service.VehicleId != request.VehicleId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
            }
        }

        request.Adapt(service);

        _unitOfWork.VehicleServices.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehicleservice:{request.Id}");
    }
}
