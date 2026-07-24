using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Queries;

public class GetPersonByIdQuery : IRequest<PersonDto>
{
    public int Id { get; set; }
    public GetPersonByIdQuery(int id) => Id = id;
}

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "person:";

    public GetPersonByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PersonDto> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        var cachedPerson = await _cacheService.GetAsync<PersonDto>(cacheKey);
        if (cachedPerson != null)
        {
            return cachedPerson;
        }

        var person = await _unitOfWork.Person.GetByIdAsync(request.Id);
        if (person == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan kişi bulunamadı.");
        }

        var dto = person.Adapt<PersonDto>();
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        return dto;
    }
}
