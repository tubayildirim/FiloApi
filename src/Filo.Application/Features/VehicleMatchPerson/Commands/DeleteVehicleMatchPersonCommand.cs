using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Commands;

public sealed class DeleteVehicleMatchPersonCommand : IRequest
{
    public int Id { get; set; }
    public DeleteVehicleMatchPersonCommand(int id) => Id = id;
}

public class DeleteVehicleMatchPersonCommandHandler : IRequestHandler<DeleteVehicleMatchPersonCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteVehicleMatchPersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteVehicleMatchPersonCommand request, CancellationToken cancellationToken)
    {
        var match = await _unitOfWork.VehicleMatches.GetByIdAsync(request.Id);
        if (match == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç-kişi ataması bulunamadı.");
        }

        _unitOfWork.VehicleMatches.Delete(match);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"vehiclematch:{request.Id}");
    }
}
