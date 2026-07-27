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

        var (items, count) = await _unitOfWork.VehicleMaintenances.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<VehicleMaintenanceDto>>();
        return new PagedList<VehicleMaintenanceDto>(dtos, count, pageNumber, pageSize);
    }
}
