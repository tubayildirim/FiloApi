using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMaintenance.Commands;

public sealed class UpdateVehicleMaintenanceCommand : IRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public int Odometer { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; set; }
    public int? NextMaintenanceKm { get; set; }
}

public class UpdateVehicleMaintenanceCommandHandler : IRequestHandler<UpdateVehicleMaintenanceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateVehicleMaintenanceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateVehicleMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
        if (maintenance == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan bakım kaydı bulunamadı.");
        }

        if (maintenance.VehicleId != request.VehicleId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw new NotFoundException($"ID'si {request.VehicleId} olan araç bulunamadı.");
            }
        }

        request.Adapt(maintenance);

        _unitOfWork.VehicleMaintenances.Update(maintenance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate single item and paged list caches
        await _cacheService.RemoveAsync($"vehiclemaintenance:{request.Id}");
        for (int page = 1; page <= 5; page++)
            await _cacheService.RemoveAsync($"vehiclemaintenances_paged_{page}_100");
    }
}
