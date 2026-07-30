using Filo.Application.Common.Interfaces;
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
    private readonly IRbacService _rbacService;
    public GetPagedVehicleTollQueryHandler(IUnitOfWork unitOfWork, IRbacService rbacService) { _unitOfWork = unitOfWork; _rbacService = rbacService; }

    public async Task<PagedList<VehicleTollDto>> Handle(GetPagedVehicleTollQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.VehicleToll, bool>>? predicate = null;
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
        
        var (items, count) = await _unitOfWork.VehicleTolls.GetPagedAsync(pageNumber, pageSize, null, q => q.OrderByDescending(x => x.Id));
        var dtos = items.Adapt<IEnumerable<VehicleTollDto>>();
        return new PagedList<VehicleTollDto>(dtos, count, pageNumber, pageSize);
    }
}
