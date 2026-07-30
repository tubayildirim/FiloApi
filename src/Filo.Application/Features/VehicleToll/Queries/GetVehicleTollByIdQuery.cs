using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleToll.Queries;

public sealed record GetVehicleTollByIdQuery(int Id) : IRequest<VehicleTollDto>;

public class GetVehicleTollByIdQueryHandler : IRequestHandler<GetVehicleTollByIdQuery, VehicleTollDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetVehicleTollByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleTollDto> Handle(GetVehicleTollByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTolls.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");
        return entity.Adapt<VehicleTollDto>();
    }
}
