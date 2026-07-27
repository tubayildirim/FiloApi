using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleService.Commands;

public sealed class DeleteVehicleServiceCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleServiceCommand(int id) => Id = id;
}

public class DeleteVehicleServiceCommandHandler : IRequestHandler<DeleteVehicleServiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleServiceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.VehicleServices.GetByIdAsync(request.Id);
        if (service == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan servis kaydı bulunamadı.");
        }

        _unitOfWork.VehicleServices.Delete(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehicleservice:{request.Id}");
    }
}
