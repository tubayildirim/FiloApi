using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System.Linq.Expressions;

namespace Filo.Application.Features.VehicleToll.Queries;

public sealed class GetPagedVehicleTollQuery : IRequest<PagedList<VehicleTollDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleTollQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleTollQueryHandler : IRequestHandler<GetPagedVehicleTollQuery, PagedList<VehicleTollDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPagedVehicleTollQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedList<VehicleTollDto>> Handle(GetPagedVehicleTollQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        var (items, count) = await _unitOfWork.VehicleTolls.GetPagedAsync(pageNumber, pageSize, null, q => q.OrderByDescending(x => x.Id));
        var dtos = items.Adapt<IEnumerable<VehicleTollDto>>();
        return new PagedList<VehicleTollDto>(dtos, count, pageNumber, pageSize);
    }
}
