using Filo.Application.DTOs;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleToll.Commands;

public sealed class CreateVehicleTollCommand : VehicleTollDto.CreateRequest, IRequest<VehicleTollDto> { }

public class CreateVehicleTollCommandHandler : IRequestHandler<CreateVehicleTollCommand, VehicleTollDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateVehicleTollCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleTollDto> Handle(CreateVehicleTollCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<Filo.Domain.Entities.VehicleToll>();
        await _unitOfWork.VehicleTolls.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Adapt<VehicleTollDto>();
    }
}
