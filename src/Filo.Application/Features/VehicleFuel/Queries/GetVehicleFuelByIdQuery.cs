using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleFuel.Queries;

public sealed class GetVehicleFuelByIdQuery : IRequest<VehicleFuelDto>
{
    public int Id { get; set; }
    public GetVehicleFuelByIdQuery(int id) => Id = id;
}

public class GetVehicleFuelByIdQueryHandler : IRequestHandler<GetVehicleFuelByIdQuery, VehicleFuelDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "vehiclefuel:";

    public GetVehicleFuelByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleFuelDto> Handle(GetVehicleFuelByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var fuel = await _unitOfWork.VehicleFuels.GetByIdAsync(request.Id);
            if (fuel == null)
            {
                throw new NotFoundException($"ID'si {request.Id} olan yakıt kaydı bulunamadı.");
            }

            return fuel.Adapt<VehicleFuelDto>();
        }, TimeSpan.FromMinutes(5));
    }
}
