using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Queries;

public sealed class GetPersonByIdQuery : IRequest<PersonDto>
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
        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var person = await _unitOfWork.Person.GetByIdAsync(request.Id);
            if (person == null)
            {
                throw new NotFoundException($"ID'si {request.Id} olan kişi bulunamadı.");
            }

            return person.Adapt<PersonDto>();
        }, TimeSpan.FromMinutes(5));
    }
}
