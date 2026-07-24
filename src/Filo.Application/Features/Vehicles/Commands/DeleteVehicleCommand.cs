using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;

namespace Filo.Application.Features.Vehicles.Commands;

public sealed class DeleteVehicleCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleCommand(int id) => Id = id;
}

public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç bulunamadı.");
        }

        _unitOfWork.Vehicles.Delete(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"vehicle:{request.Id}");
    }
}
