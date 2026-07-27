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

    public GetPagedVehicleFuelQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedList<VehicleFuelDto>> Handle(GetPagedVehicleFuelQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        var (items, count) = await _unitOfWork.VehicleFuels.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<VehicleFuelDto>>();
        return new PagedList<VehicleFuelDto>(dtos, count, pageNumber, pageSize);
    }
}
