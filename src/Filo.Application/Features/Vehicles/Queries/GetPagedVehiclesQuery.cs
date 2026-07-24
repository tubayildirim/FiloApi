using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Vehicles.Queries;

public class GetPagedVehiclesQuery : IRequest<PagedList<VehicleDto>>
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
        string cacheKey = $"vehicles_paged_{pageNumber}_{pageSize}";

        var cachedResult = await _cacheService.GetAsync<PagedList<VehicleDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var (items, count) = await _unitOfWork.Vehicles.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<VehicleDto>>();
        var pagedList = new PagedList<VehicleDto>(dtos, count, pageNumber, pageSize);

        await _cacheService.SetAsync(cacheKey, pagedList, TimeSpan.FromMinutes(2));

        return pagedList;
    }
}
