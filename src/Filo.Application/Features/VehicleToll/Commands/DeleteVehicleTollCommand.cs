using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;

namespace Filo.Application.Features.VehicleToll.Commands;

public sealed record DeleteVehicleTollCommand(int Id) : IRequest;

public class DeleteVehicleTollCommandHandler : IRequestHandler<DeleteVehicleTollCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteVehicleTollCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteVehicleTollCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTolls.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        _unitOfWork.VehicleTolls.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
