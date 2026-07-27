using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleService.Queries;

public sealed class GetPagedVehicleServiceQuery : IRequest<PagedList<VehicleServiceDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleServiceQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleServiceQueryHandler : IRequestHandler<GetPagedVehicleServiceQuery, PagedList<VehicleServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetPagedVehicleServiceQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PagedList<VehicleServiceDto>> Handle(GetPagedVehicleServiceQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        string cacheKey = $"vehicleservices_paged_{pageNumber}_{pageSize}";

        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var (items, count) = await _unitOfWork.VehicleServices.GetPagedAsync(pageNumber, pageSize);
            var dtos = items.Adapt<IEnumerable<VehicleServiceDto>>();
            return new PagedList<VehicleServiceDto>(dtos, count, pageNumber, pageSize);
        }, TimeSpan.FromMinutes(2));
    }
}
