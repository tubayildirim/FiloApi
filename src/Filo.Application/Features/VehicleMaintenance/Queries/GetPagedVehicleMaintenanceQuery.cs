using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMaintenance.Queries;

public sealed class GetPagedVehicleMaintenanceQuery : IRequest<PagedList<VehicleMaintenanceDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleMaintenanceQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleMaintenanceQueryHandler : IRequestHandler<GetPagedVehicleMaintenanceQuery, PagedList<VehicleMaintenanceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPagedVehicleMaintenanceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedList<VehicleMaintenanceDto>> Handle(GetPagedVehicleMaintenanceQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        string sortCol = paginationParams.SortColumn?.Trim().ToLower() ?? string.Empty;
        string sortDir = paginationParams.SortDirection?.Trim().ToLower() ?? string.Empty;

        // Sıralama
        System.Func<System.Linq.IQueryable<Filo.Domain.Entities.VehicleMaintenance>, System.Linq.IOrderedQueryable<Filo.Domain.Entities.VehicleMaintenance>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortCol))
        {
            bool isDesc = sortDir == "desc";
            orderBy = sortCol switch
            {
                "date" => q => isDesc ? q.OrderByDescending(m => m.MaintenanceDate) : q.OrderBy(m => m.MaintenanceDate),
                "km" => q => isDesc ? q.OrderByDescending(m => m.Odometer) : q.OrderBy(m => m.Odometer),
                "type" => q => isDesc ? q.OrderByDescending(m => m.MaintenanceType) : q.OrderBy(m => m.MaintenanceType),
                "cost" => q => isDesc ? q.OrderByDescending(m => m.Cost) : q.OrderBy(m => m.Cost),
                _ => q => isDesc ? q.OrderByDescending(m => m.VehicleMaintenanceId) : q.OrderBy(m => m.VehicleMaintenanceId)
            };
        }

        var (items, count) = await _unitOfWork.VehicleMaintenances.GetPagedAsync(pageNumber, pageSize, null, orderBy);
        var dtos = items.Adapt<IEnumerable<VehicleMaintenanceDto>>();
        return new PagedList<VehicleMaintenanceDto>(dtos, count, pageNumber, pageSize);
    }
}
