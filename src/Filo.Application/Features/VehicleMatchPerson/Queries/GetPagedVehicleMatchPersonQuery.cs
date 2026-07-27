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

    public GetPagedVehicleMatchPersonQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedList<VehicleMatchPersonDto>> Handle(GetPagedVehicleMatchPersonQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        var (items, count) = await _unitOfWork.VehicleMatches.GetPagedAsync(pageNumber, pageSize);
        var dtos = items.Adapt<IEnumerable<VehicleMatchPersonDto>>();
        return new PagedList<VehicleMatchPersonDto>(dtos, count, pageNumber, pageSize);
    }
}
