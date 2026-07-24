using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Queries;

public class GetPagedPersonQuery : IRequest<PagedList<PersonDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedPersonQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedPersonQueryHandler : IRequestHandler<GetPagedPersonQuery, PagedList<PersonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetPagedPersonQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PagedList<PersonDto>> Handle(GetPagedPersonQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        string cacheKey = $"person_paged_{pageNumber}_{pageSize}";

        var cachedResult = await _cacheService.GetAsync<PagedList<PersonDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var (items, count) = await _unitOfWork.Person.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<PersonDto>>();
        var pagedList = new PagedList<PersonDto>(dtos, count, pageNumber, pageSize);

        await _cacheService.SetAsync(cacheKey, pagedList, TimeSpan.FromMinutes(2));

        return pagedList;
    }
}
