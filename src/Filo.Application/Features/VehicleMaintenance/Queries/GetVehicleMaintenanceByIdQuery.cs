using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMaintenance.Queries;

public sealed class GetVehicleMaintenanceByIdQuery : IRequest<VehicleMaintenanceDto>
{
    public int Id { get; set; }
    public GetVehicleMaintenanceByIdQuery(int id) => Id = id;
}

public class GetVehicleMaintenanceByIdQueryHandler : IRequestHandler<GetVehicleMaintenanceByIdQuery, VehicleMaintenanceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "vehiclemaintenance:";

    public GetVehicleMaintenanceByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleMaintenanceDto> Handle(GetVehicleMaintenanceByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var maintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
            if (maintenance == null)
            {
                throw new NotFoundException($"ID'si {request.Id} olan bakım kaydı bulunamadı.");
            }

            return maintenance.Adapt<VehicleMaintenanceDto>();
        }, TimeSpan.FromMinutes(5));
    }
}
