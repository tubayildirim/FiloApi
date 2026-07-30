using Filo.Application.DTOs;
using Filo.Common.Models;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System.Linq.Expressions;

namespace Filo.Application.Features.VehicleInsurance.Queries;

public sealed class GetPagedVehicleInsuranceQuery : IRequest<PagedList<VehicleInsuranceDto>>
{
    public PaginationParams PaginationParams { get; set; }
    public GetPagedVehicleInsuranceQuery(PaginationParams paginationParams) => PaginationParams = paginationParams;
}

public class GetPagedVehicleInsuranceQueryHandler : IRequestHandler<GetPagedVehicleInsuranceQuery, PagedList<VehicleInsuranceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPagedVehicleInsuranceQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedList<VehicleInsuranceDto>> Handle(GetPagedVehicleInsuranceQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = request.PaginationParams;
        int pageNumber = paginationParams.PageNumber ?? 1;
        int pageSize = paginationParams.PageSize ?? 10;
        
        var (items, count) = await _unitOfWork.VehicleInsurances.GetPagedAsync(pageNumber, pageSize, null, q => q.OrderByDescending(x => x.Id));
        var dtos = items.Adapt<IEnumerable<VehicleInsuranceDto>>();
        return new PagedList<VehicleInsuranceDto>(dtos, count, pageNumber, pageSize);
    }
}
