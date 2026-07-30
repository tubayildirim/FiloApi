using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System.Linq.Expressions;

namespace Filo.Application.Features.VehicleTrafficFine.Queries;

public sealed class GetPagedVehicleTrafficFineQuery : IRequest<PagedList<VehicleTrafficFineDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleTrafficFineQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleTrafficFineQueryHandler : IRequestHandler<GetPagedVehicleTrafficFineQuery, PagedList<VehicleTrafficFineDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPagedVehicleTrafficFineQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedList<VehicleTrafficFineDto>> Handle(GetPagedVehicleTrafficFineQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        var (items, count) = await _unitOfWork.VehicleTrafficFines.GetPagedAsync(pageNumber, pageSize, null, q => q.OrderByDescending(x => x.Id));
        var dtos = items.Adapt<IEnumerable<VehicleTrafficFineDto>>();
        return new PagedList<VehicleTrafficFineDto>(dtos, count, pageNumber, pageSize);
    }
}
