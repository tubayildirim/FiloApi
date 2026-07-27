using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Queries;

public sealed class GetPagedVehicleMatchPersonQuery : IRequest<PagedList<VehicleMatchPersonDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleMatchPersonQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleMatchPersonQueryHandler : IRequestHandler<GetPagedVehicleMatchPersonQuery, PagedList<VehicleMatchPersonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetPagedVehicleMatchPersonQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PagedList<VehicleMatchPersonDto>> Handle(GetPagedVehicleMatchPersonQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        string cacheKey = $"vehiclematches_paged_{pageNumber}_{pageSize}";

        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var (items, count) = await _unitOfWork.VehicleMatches.GetPagedAsync(pageNumber, pageSize);
            var dtos = items.Adapt<IEnumerable<VehicleMatchPersonDto>>();
            return new PagedList<VehicleMatchPersonDto>(dtos, count, pageNumber, pageSize);
        }, TimeSpan.FromMinutes(2));
    }
}
