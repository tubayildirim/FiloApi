using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleFuel.Commands;

public sealed class DeleteVehicleFuelCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleFuelCommand(int id) => Id = id;
}

public class DeleteVehicleFuelCommandHandler : IRequestHandler<DeleteVehicleFuelCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleFuelCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleFuelCommand request, CancellationToken cancellationToken)
    {
        var fuel = await _unitOfWork.VehicleFuels.GetByIdAsync(request.Id);
        if (fuel == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan yakıt kaydı bulunamadı.");
        }

        _unitOfWork.VehicleFuels.Delete(fuel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate single item and paged list caches
        await _cacheService.RemoveAsync($"vehiclefuel:{request.Id}");
        for (int page = 1; page <= 5; page++)
            await _cacheService.RemoveAsync($"vehiclefuels_paged_{page}_100");
    }
}
