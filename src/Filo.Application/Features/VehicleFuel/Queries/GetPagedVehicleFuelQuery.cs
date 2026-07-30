using Filo.Application.Common.Interfaces;
using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleFuel.Queries;

public sealed class GetPagedVehicleFuelQuery : IRequest<PagedList<VehicleFuelDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleFuelQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleFuelQueryHandler : IRequestHandler<GetPagedVehicleFuelQuery, PagedList<VehicleFuelDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRbacService _rbacService;

    public GetPagedVehicleFuelQueryHandler(IUnitOfWork unitOfWork, IRbacService rbacService)
    {
        _unitOfWork = unitOfWork;
        _rbacService = rbacService;
    }

    public async Task<PagedList<VehicleFuelDto>> Handle(GetPagedVehicleFuelQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        string sortCol = paginationParams.SortColumn?.Trim().ToLower() ?? string.Empty;
        string sortDir = paginationParams.SortDirection?.Trim().ToLower() ?? string.Empty;

        // Sıralama
        System.Func<System.Linq.IQueryable<Filo.Domain.Entities.VehicleFuel>, System.Linq.IOrderedQueryable<Filo.Domain.Entities.VehicleFuel>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortCol))
        {
            bool isDesc = sortDir == "desc";
            orderBy = sortCol switch
            {
                "date" => q => isDesc ? q.OrderByDescending(f => f.RefuelingDate) : q.OrderBy(f => f.RefuelingDate),
                "km" => q => isDesc ? q.OrderByDescending(f => f.Odometer) : q.OrderBy(f => f.Odometer),
                "liters" => q => isDesc ? q.OrderByDescending(f => f.Liters) : q.OrderBy(f => f.Liters),
                "price" => q => isDesc ? q.OrderByDescending(f => f.PricePerLiter) : q.OrderBy(f => f.PricePerLiter),
                "total" => q => isDesc ? q.OrderByDescending(f => f.TotalPrice) : q.OrderBy(f => f.TotalPrice),
                _ => q => isDesc ? q.OrderByDescending(f => f.VehicleFuelId) : q.OrderBy(f => f.VehicleFuelId)
            };
        }

        System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.VehicleFuel, bool>>? predicate = null;
            var allowedIds = await _rbacService.GetAllowedVehicleIdsAsync();
            if (allowedIds != null)
            {
                if (predicate == null)
                    predicate = v => allowedIds.Contains(v.VehicleId);
                else
                {
                    var oldPredicate = predicate;
                    predicate = v => allowedIds.Contains(v.VehicleId) && oldPredicate.Compile()(v);
                }
            }

        var (items, count) = await _unitOfWork.VehicleFuels.GetPagedAsync(pageNumber, pageSize, predicate, orderBy);
        var dtos = items.Adapt<IEnumerable<VehicleFuelDto>>();
        return new PagedList<VehicleFuelDto>(dtos, count, pageNumber, pageSize);
    }
}
