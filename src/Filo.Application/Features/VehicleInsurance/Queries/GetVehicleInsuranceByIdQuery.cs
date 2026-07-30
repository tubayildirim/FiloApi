using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleInsurance.Queries;

public sealed record GetVehicleInsuranceByIdQuery(int Id) : IRequest<VehicleInsuranceDto>;

public class GetVehicleInsuranceByIdQueryHandler : IRequestHandler<GetVehicleInsuranceByIdQuery, VehicleInsuranceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetVehicleInsuranceByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleInsuranceDto> Handle(GetVehicleInsuranceByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleInsurances.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");
        return entity.Adapt<VehicleInsuranceDto>();
    }
}
