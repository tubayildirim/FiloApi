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

    public GetPagedVehicleServiceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedList<VehicleServiceDto>> Handle(GetPagedVehicleServiceQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        var (items, count) = await _unitOfWork.VehicleServices.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<VehicleServiceDto>>();
        return new PagedList<VehicleServiceDto>(dtos, count, pageNumber, pageSize);
    }
}
