using Filo.Application.Common.Interfaces;
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
    private readonly IRbacService _rbacService;
    public GetPagedVehicleTrafficFineQueryHandler(IUnitOfWork unitOfWork, IRbacService rbacService) { _unitOfWork = unitOfWork; _rbacService = rbacService; }

    public async Task<PagedList<VehicleTrafficFineDto>> Handle(GetPagedVehicleTrafficFineQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.VehicleTrafficFine, bool>>? predicate = null;
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
        
        var (items, count) = await _unitOfWork.VehicleTrafficFines.GetPagedAsync(pageNumber, pageSize, predicate, q => q.OrderByDescending(x => x.Id));
        var dtos = items.Adapt<IEnumerable<VehicleTrafficFineDto>>();
        return new PagedList<VehicleTrafficFineDto>(dtos, count, pageNumber, pageSize);
    }
}
