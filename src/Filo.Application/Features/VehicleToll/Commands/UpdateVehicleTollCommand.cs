using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleToll.Commands;

public sealed class UpdateVehicleTollCommand : VehicleTollDto.UpdateRequest, IRequest
{
    public int Id { get; set; }
}

public class UpdateVehicleTollCommandHandler : IRequestHandler<UpdateVehicleTollCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateVehicleTollCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateVehicleTollCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTolls.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        request.Adapt(entity);
        _unitOfWork.VehicleTolls.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
