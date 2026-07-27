using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMaintenance.Commands;

public sealed class DeleteVehicleMaintenanceCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleMaintenanceCommand(int id) => Id = id;
}

public class DeleteVehicleMaintenanceCommandHandler : IRequestHandler<DeleteVehicleMaintenanceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleMaintenanceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
        if (maintenance == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan bakım kaydı bulunamadı.");
        }

        _unitOfWork.VehicleMaintenances.Delete(maintenance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehiclemaintenance:{request.Id}");
    }
}
