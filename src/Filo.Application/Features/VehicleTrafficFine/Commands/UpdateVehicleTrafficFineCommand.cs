using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleTrafficFine.Commands;

public sealed class UpdateVehicleTrafficFineCommand : VehicleTrafficFineDto.UpdateRequest, IRequest
{
    public int Id { get; set; }
}

public class UpdateVehicleTrafficFineCommandHandler : IRequestHandler<UpdateVehicleTrafficFineCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateVehicleTrafficFineCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateVehicleTrafficFineCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.VehicleTrafficFines.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException($"ID: {request.Id} not found.");

        request.Adapt(entity);
        _unitOfWork.VehicleTrafficFines.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
