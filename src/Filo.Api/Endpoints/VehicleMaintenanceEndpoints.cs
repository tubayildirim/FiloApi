using Filo.Application.DTOs;
using Filo.Application.Features.VehicleMaintenance.Commands;
using Filo.Application.Features.VehicleMaintenance.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleMaintenanceEndpoints
{
    public static void MapVehicleMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-maintenances")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleMaintenances");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleMaintenanceQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleMaintenanceDto>>.SuccessResponse(pagedResult, "Araç bakım kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleMaintenances");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var maintenance = await sender.Send(new GetVehicleMaintenanceByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleMaintenanceDto>.SuccessResponse(maintenance, "Araç bakım kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleMaintenanceById");

        group.MapPost("/", async (CreateVehicleMaintenanceCommand request, ISender sender, IValidator<VehicleMaintenanceDto.CreateRequest> validator) =>
        {
            var validationRequest = new VehicleMaintenanceDto.CreateRequest
            {
                VehicleId = request.VehicleId,
                MaintenanceDate = request.MaintenanceDate,
                Odometer = request.Odometer,
                Description = request.Description,
                Cost = request.Cost,
                MaintenanceType = request.MaintenanceType,
                NextMaintenanceDate = request.NextMaintenanceDate,
                NextMaintenanceKm = request.NextMaintenanceKm
            };
            
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdMaintenance = await sender.Send(request);
            return Results.Created($"/vehicle-maintenances/{createdMaintenance.VehicleMaintenanceId}", 
                ApiResponse<VehicleMaintenanceDto>.SuccessResponse(createdMaintenance, "Araç bakım kaydı başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleMaintenance");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleMaintenanceCommand request, ISender sender, IValidator<VehicleMaintenanceDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new VehicleMaintenanceDto.UpdateRequest
            {
                VehicleId = request.VehicleId,
                MaintenanceDate = request.MaintenanceDate,
                Odometer = request.Odometer,
                Description = request.Description,
                Cost = request.Cost,
                MaintenanceType = request.MaintenanceType,
                NextMaintenanceDate = request.NextMaintenanceDate,
                NextMaintenanceKm = request.NextMaintenanceKm
            };

            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç bakım kaydı başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleMaintenance");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleMaintenanceCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç bakım kaydı başarıyla silindi."));
        })
        .WithName("DeleteVehicleMaintenance");
    }
}
