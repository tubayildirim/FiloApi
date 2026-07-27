using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleService.Queries;

public sealed class GetVehicleServiceByIdQuery : IRequest<VehicleServiceDto>
{
    public int Id { get; set; }
    public GetVehicleServiceByIdQuery(int id) => Id = id;
}

public class GetVehicleServiceByIdQueryHandler : IRequestHandler<GetVehicleServiceByIdQuery, VehicleServiceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "vehicleservice:";

    public GetVehicleServiceByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleServiceDto> Handle(GetVehicleServiceByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var service = await _unitOfWork.VehicleServices.GetByIdAsync(request.Id);
            if (service == null)
            {
                throw new NotFoundException($"ID'si {request.Id} olan servis kaydı bulunamadı.");
            }

            return service.Adapt<VehicleServiceDto>();
        }, TimeSpan.FromMinutes(5));
    }
}
