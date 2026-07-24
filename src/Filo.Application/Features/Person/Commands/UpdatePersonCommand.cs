using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Commands;

public sealed class UpdatePersonCommand : IRequest
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Tckn { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
}

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdatePersonCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _unitOfWork.Person.GetByIdAsync(request.Id);
        if (person == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan kişi bulunamadı.");
        }

        if (person.Tckn != request.Tckn)
        {
            var existing = await _unitOfWork.Person.GetByTcknAsync(request.Tckn);
            if (existing != null)
            {
                throw new ValidationException($"{request.Tckn} TC kimlik numarasına sahip kişi zaten mevcut.");
            }
        }

        request.Adapt(person);

        _unitOfWork.Person.Update(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"person:{request.Id}");
    }
}
