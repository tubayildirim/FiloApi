using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleInsurance.Commands;

public sealed class UpdateVehicleInsuranceCommand : VehicleInsuranceDto.UpdateRequest, IRequest
{
    public int Id { get; set; }
}

public class UpdateVehicleInsuranceCommandHandler : IRequestHandler<UpdateVehicleInsuranceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateVehicleInsuranceCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateVehicleInsuranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleInsurances.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        request.Adapt(entity);
        _unitOfWork.VehicleInsurances.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
