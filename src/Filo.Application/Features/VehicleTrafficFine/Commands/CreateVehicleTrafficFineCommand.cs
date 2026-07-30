using Filo.Application.DTOs;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.VehicleTrafficFine.Commands;

public sealed class CreateVehicleTrafficFineCommand : VehicleTrafficFineDto.CreateRequest, IRequest<VehicleTrafficFineDto> { }

public class CreateVehicleTrafficFineCommandHandler : IRequestHandler<CreateVehicleTrafficFineCommand, VehicleTrafficFineDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateVehicleTrafficFineCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<VehicleTrafficFineDto> Handle(CreateVehicleTrafficFineCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<Filo.Domain.Entities.VehicleTrafficFine>();
        await _unitOfWork.VehicleTrafficFines.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Adapt<VehicleTrafficFineDto>();
    }
}
