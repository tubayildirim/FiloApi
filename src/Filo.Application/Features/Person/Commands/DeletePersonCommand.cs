using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using MediatR;

namespace Filo.Application.Features.Person.Commands;

public sealed class DeletePersonCommand : IRequest
{
    public int Id { get; set; }
    public DeletePersonCommand(int id) => Id = id;
}

public class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeletePersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _unitOfWork.Person.GetByIdAsync(request.Id);
        if (person == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan kişi bulunamadı.");
        }

        _unitOfWork.Person.Delete(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"person:{request.Id}");
    }
}
