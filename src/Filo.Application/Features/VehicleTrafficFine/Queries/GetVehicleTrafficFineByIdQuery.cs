using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleTrafficFine.Queries;

public sealed record GetVehicleTrafficFineByIdQuery(int Id) : IRequest<VehicleTrafficFineDto>;

public class GetVehicleTrafficFineByIdQueryHandler : IRequestHandler<GetVehicleTrafficFineByIdQuery, VehicleTrafficFineDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetVehicleTrafficFineByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleTrafficFineDto> Handle(GetVehicleTrafficFineByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTrafficFines.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");
        return entity.Adapt<VehicleTrafficFineDto>();
    }
}
