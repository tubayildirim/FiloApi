using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Commands;

public class PersonCreatedEvent : BaseEvent
{
    public PersonDto Person { get; }
    public PersonCreatedEvent(PersonDto person) => Person = person;
}

public sealed class CreatePersonCommand : IRequest<PersonDto>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Tckn { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
}

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePersonCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PersonDto> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Person.GetByTcknAsync(request.Tckn);
        if (existing != null)
        {
            throw new ValidationException($"{request.Tckn} TC kimlik numarasına sahip kişi zaten mevcut.");
        }

        var person = request.Adapt<Filo.Domain.Entities.Person>();
        
        await _unitOfWork.Person.AddAsync(person);
        
        var dto = person.Adapt<PersonDto>();
        person.AddDomainEvent(new PersonCreatedEvent(dto));
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Filo.Common.Telemetry.ApplicationTelemetry.PersonsCreatedCounter.Add(1, 
            new KeyValuePair<string, object?>("gender", person.Gender));

        return person.Adapt<PersonDto>();
    }
}
