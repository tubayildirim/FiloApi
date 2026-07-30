using Filo.Application.DTOs;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleInsurance.Commands;

public sealed class CreateVehicleInsuranceCommand : VehicleInsuranceDto.CreateRequest, IRequest<VehicleInsuranceDto> { }

public class CreateVehicleInsuranceCommandHandler : IRequestHandler<CreateVehicleInsuranceCommand, VehicleInsuranceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateVehicleInsuranceCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleInsuranceDto> Handle(CreateVehicleInsuranceCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<Filo.Domain.Entities.VehicleInsurance>();
        await _unitOfWork.VehicleInsurances.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Adapt<VehicleInsuranceDto>();
    }
}
