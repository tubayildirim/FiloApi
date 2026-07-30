using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;

namespace Filo.Application.Features.VehicleInsurance.Commands;

public sealed record DeleteVehicleInsuranceCommand(int Id) : IRequest;

public class DeleteVehicleInsuranceCommandHandler : IRequestHandler<DeleteVehicleInsuranceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteVehicleInsuranceCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteVehicleInsuranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleInsurances.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        _unitOfWork.VehicleInsurances.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
