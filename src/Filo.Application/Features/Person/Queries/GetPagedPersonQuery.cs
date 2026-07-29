using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Person.Queries;

public sealed class GetPagedPersonQuery : IRequest<PagedList<PersonDto>>
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
        
        string searchTerm = paginationParams.SearchTerm?.Trim().ToLower() ?? string.Empty;
        string sortCol = paginationParams.SortColumn?.Trim().ToLower() ?? string.Empty;
        string sortDir = paginationParams.SortDirection?.Trim().ToLower() ?? string.Empty;

        string cacheKey = $"person_paged_{pageNumber}_{pageSize}_{searchTerm}_{sortCol}_{sortDir}";

        
            // Filtreleme
            System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.Person, bool>>? predicate = null;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                predicate = p => p.Name.ToLower().Contains(searchTerm) || 
                                 p.Surname.ToLower().Contains(searchTerm) || 
                                 p.Tckn.ToLower().Contains(searchTerm);
            }

            // Sıralama
            Func<IQueryable<Filo.Domain.Entities.Person>, IOrderedQueryable<Filo.Domain.Entities.Person>>? orderBy = null;
            if (!string.IsNullOrEmpty(sortCol))
            {
                bool isDesc = sortDir == "desc";
                orderBy = sortCol switch
                {
                    "name" => q => isDesc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
                    "surname" => q => isDesc ? q.OrderByDescending(p => p.Surname) : q.OrderBy(p => p.Surname),
                    "tckn" => q => isDesc ? q.OrderByDescending(p => p.Tckn) : q.OrderBy(p => p.Tckn),
                    "age" => q => isDesc ? q.OrderByDescending(p => p.Age) : q.OrderBy(p => p.Age),
                    _ => q => isDesc ? q.OrderByDescending(p => p.Id) : q.OrderBy(p => p.Id)
                };
            }

            var (items, count) = await _unitOfWork.Person.GetPagedAsync(pageNumber, pageSize, predicate, orderBy);
            var dtos = items.Adapt<IEnumerable<PersonDto>>();
            return new PagedList<PersonDto>(dtos, count, pageNumber, pageSize);
        
    }
}
