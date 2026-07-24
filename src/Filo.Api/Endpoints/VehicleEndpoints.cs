using Filo.Application.DTOs;
using Filo.Application.Features.Vehicles.Commands;
using Filo.Application.Features.Vehicles.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Filo.Api.Endpoints;

public static class VehicleEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicles")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("Vehicles");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehiclesQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleDto>>.SuccessResponse(pagedResult, "Araçlar başarıyla getirildi."));
        })
        .WithName("GetVehicles");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var vehicle = await sender.Send(new GetVehicleByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Araç başarıyla getirildi."));
        })
        .WithName("GetVehicleById");

        group.MapPost("/", async (CreateVehicleCommand request, ISender sender, IValidator<VehicleDto.CreateRequest> validator) =>
        {
            var validationRequest = new VehicleDto.CreateRequest 
            { 
                Brand = request.Brand, 
                Model = request.Model, 
                Year = request.Year, 
                PlateNumber = request.PlateNumber,
                Color = request.Color,
                FuelType = request.FuelType,
                TransmissionType = request.TransmissionType,
                EngineNumber = request.EngineNumber,
                ChassisNumber = request.ChassisNumber,
                RegistrationDate = request.RegistrationDate,
                PersonId = request.PersonId
            };
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdVehicle = await sender.Send(request);
            return Results.Created($"/vehicles/{createdVehicle.Id}", ApiResponse<VehicleDto>.SuccessResponse(createdVehicle, "Araç başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicle");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleCommand request, ISender sender, IValidator<VehicleDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new VehicleDto.UpdateRequest 
            { 
                Brand = request.Brand, 
                Model = request.Model, 
                Year = request.Year, 
                PlateNumber = request.PlateNumber,
                Color = request.Color,
                FuelType = request.FuelType,
                TransmissionType = request.TransmissionType,
                EngineNumber = request.EngineNumber,
                ChassisNumber = request.ChassisNumber,
                RegistrationDate = request.RegistrationDate,
                PersonId = request.PersonId
            };
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç başarıyla güncellendi."));
        })
        .WithName("UpdateVehicle");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç başarıyla silindi."));
        })
        .WithName("DeleteVehicle");
    }
}
