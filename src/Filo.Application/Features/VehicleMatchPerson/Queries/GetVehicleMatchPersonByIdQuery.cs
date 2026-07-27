using Filo.Application.DTOs;
using Filo.Application.Exceptions;
using Filo.Domain.Interfaces;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filo.Application.Features.VehicleMatchPerson.Queries;

public sealed class GetVehicleMatchPersonByIdQuery : IRequest<VehicleMatchPersonDto>
{
    public int Id { get; set; }
    public GetVehicleMatchPersonByIdQuery(int id) => Id = id;
}

public class GetVehicleMatchPersonByIdQueryHandler : IRequestHandler<GetVehicleMatchPersonByIdQuery, VehicleMatchPersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "vehiclematch:";

    public GetVehicleMatchPersonByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<VehicleMatchPersonDto> Handle(GetVehicleMatchPersonByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{request.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async ct =>
        {
            var match = await _unitOfWork.VehicleMatches.GetByIdAsync(request.Id);
            if (match == null)
            {
                throw new NotFoundException($"ID'si {request.Id} olan araç-kişi ataması bulunamadı.");
            }

            return match.Adapt<VehicleMatchPersonDto>();
        }, TimeSpan.FromMinutes(5));
    }
}
