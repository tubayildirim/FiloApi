using Filo.Application.DTOs;
using Filo.Application.Features.VehicleFuel.Commands;
using Filo.Application.Features.VehicleFuel.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleFuelEndpoints
{
    public static void MapVehicleFuelEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-fuels")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleFuels");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleFuelQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleFuelDto>>.SuccessResponse(pagedResult, "Araç yakıt kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleFuels");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var fuel = await sender.Send(new GetVehicleFuelByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleFuelDto>.SuccessResponse(fuel, "Araç yakıt kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleFuelById");

        group.MapPost("/", async (CreateVehicleFuelCommand request, ISender sender, IValidator<VehicleFuelDto.CreateRequest> validator) =>
        {
            var validationRequest = new VehicleFuelDto.CreateRequest
            {
                VehicleId = request.VehicleId,
                RefuelingDate = request.RefuelingDate,
                Odometer = request.Odometer,
                Liters = request.Liters,
                PricePerLiter = request.PricePerLiter,
                ReceiptNumber = request.ReceiptNumber
            };
            
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdFuel = await sender.Send(request);
            return Results.Created($"/vehicle-fuels/{createdFuel.VehicleFuelId}", 
                ApiResponse<VehicleFuelDto>.SuccessResponse(createdFuel, "Araç yakıt kaydı başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleFuel");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleFuelCommand request, ISender sender, IValidator<VehicleFuelDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new VehicleFuelDto.UpdateRequest
            {
                VehicleId = request.VehicleId,
                RefuelingDate = request.RefuelingDate,
                Odometer = request.Odometer,
                Liters = request.Liters,
                PricePerLiter = request.PricePerLiter,
                ReceiptNumber = request.ReceiptNumber
            };

            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç yakıt kaydı başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleFuel");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleFuelCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç yakıt kaydı başarıyla silindi."));
        })
        .WithName("DeleteVehicleFuel");
    }
}
