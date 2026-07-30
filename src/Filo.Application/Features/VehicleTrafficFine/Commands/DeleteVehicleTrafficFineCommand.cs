using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;

namespace Filo.Application.Features.VehicleTrafficFine.Commands;

public sealed record DeleteVehicleTrafficFineCommand(int Id) : IRequest;

public class DeleteVehicleTrafficFineCommandHandler : IRequestHandler<DeleteVehicleTrafficFineCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteVehicleTrafficFineCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteVehicleTrafficFineCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTrafficFines.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        _unitOfWork.VehicleTrafficFines.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
