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

namespace Filo.Application.Features.VehicleService.Queries;

public sealed class GetPagedVehicleServiceQuery : IRequest<PagedList<VehicleServiceDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleServiceQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleServiceQueryHandler : IRequestHandler<GetPagedVehicleServiceQuery, PagedList<VehicleServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRbacService _rbacService;

    public GetPagedVehicleServiceQueryHandler(IUnitOfWork unitOfWork, IRbacService rbacService)
    {
        _unitOfWork = unitOfWork;
        _rbacService = rbacService;
    }

    public async Task<PagedList<VehicleServiceDto>> Handle(GetPagedVehicleServiceQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;

        string sortCol = paginationParams.SortColumn?.Trim().ToLower() ?? string.Empty;
        string sortDir = paginationParams.SortDirection?.Trim().ToLower() ?? string.Empty;

        // Sıralama
        System.Func<System.Linq.IQueryable<Filo.Domain.Entities.VehicleService>, System.Linq.IOrderedQueryable<Filo.Domain.Entities.VehicleService>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortCol))
        {
            bool isDesc = sortDir == "desc";
            orderBy = sortCol switch
            {
                "entrydate" => q => isDesc ? q.OrderByDescending(s => s.EntryDate) : q.OrderBy(s => s.EntryDate),
                "exitdate" => q => isDesc ? q.OrderByDescending(s => s.ExitDate) : q.OrderBy(s => s.ExitDate),
                "company" => q => isDesc ? q.OrderByDescending(s => s.ServiceCompany) : q.OrderBy(s => s.ServiceCompany),
                "cost" => q => isDesc ? q.OrderByDescending(s => s.Cost) : q.OrderBy(s => s.Cost),
                _ => q => isDesc ? q.OrderByDescending(s => s.VehicleServiceId) : q.OrderBy(s => s.VehicleServiceId)
            };
        }

        System.Linq.Expressions.Expression<Func<Filo.Domain.Entities.VehicleService, bool>>? predicate = null;
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

        var (items, count) = await _unitOfWork.VehicleServices.GetPagedAsync(pageNumber, pageSize, predicate, orderBy);
        var dtos = items.Adapt<IEnumerable<VehicleServiceDto>>();
        return new PagedList<VehicleServiceDto>(dtos, count, pageNumber, pageSize);
    }
}
