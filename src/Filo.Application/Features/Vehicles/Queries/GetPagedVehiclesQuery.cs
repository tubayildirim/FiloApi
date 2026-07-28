using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Vehicles.Queries;

public sealed class GetPagedVehiclesQuery : IRequest<PagedList<VehicleDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehiclesQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehiclesQueryHandler : IRequestHandler<GetPagedVehiclesQuery, PagedList<VehicleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetPagedVehiclesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PagedList<VehicleDto>> Handle(GetPagedVehiclesQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        string searchTerm = paginationParams.SearchTerm?.Trim().ToLower() ?? string.Empty;
        string sortCol = paginationParams.SortColumn?.Trim().ToLower() ?? string.Empty;
        string sortDir = paginationParams.SortDirection?.Trim().ToLower() ?? string.Empty;

        // Benzersiz Cache Anahtarı
        string cacheKey = $"vehicles_paged_{pageNumber}_{pageSize}_{searchTerm}_{sortCol}_{sortDir}";

        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            // Filtreleme (Arama) Mantığı
            System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.Vehicle, bool>>? predicate = null;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                predicate = v => v.PlateNumber.ToLower().Contains(searchTerm) || 
                                 v.Brand.ToLower().Contains(searchTerm) || 
                                 v.Model.ToLower().Contains(searchTerm);
            }

            // Sıralama (Sorting) Mantığı
            Func<IQueryable<Filo.Domain.Entities.Vehicle>, IOrderedQueryable<Filo.Domain.Entities.Vehicle>>? orderBy = null;
            if (!string.IsNullOrEmpty(sortCol))
            {
                bool isDesc = sortDir == "desc";
                orderBy = sortCol switch
                {
                    "platenumber" => q => isDesc ? q.OrderByDescending(v => v.PlateNumber) : q.OrderBy(v => v.PlateNumber),
                    "brand" => q => isDesc ? q.OrderByDescending(v => v.Brand) : q.OrderBy(v => v.Brand),
                    "year" => q => isDesc ? q.OrderByDescending(v => v.Year) : q.OrderBy(v => v.Year),
                    _ => q => isDesc ? q.OrderByDescending(v => v.Id) : q.OrderBy(v => v.Id)
                };
            }

            var (items, count) = await _unitOfWork.Vehicles.GetPagedAsync(pageNumber, pageSize, predicate, orderBy);
            var dtos = items.Adapt<IEnumerable<VehicleDto>>();
            return new PagedList<VehicleDto>(dtos, count, pageNumber, pageSize);
        }, TimeSpan.FromMinutes(2));
    }
}
