using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;

namespace Filo.Application.Features.Vehicles.Queries;

public class GetVehicleByIdQuery : IRequest<VehicleDto>
{
    public int Id { get; set; }
    public GetVehicleByIdQuery(int id) => Id = id;
}

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "vehicle:";

    public GetVehicleByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        var cachedVehicle = await _cacheService.GetAsync<VehicleDto>(cacheKey);
        if (cachedVehicle != null)
        {
            return cachedVehicle;
        }

        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
        if (vehicle == null)
        {
            throw new NotFoundException($"ID'si {request.Id} olan araç bulunamadı.");
        }

        var dto = vehicle.Adapt<VehicleDto>();
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        return dto;
    }
}
