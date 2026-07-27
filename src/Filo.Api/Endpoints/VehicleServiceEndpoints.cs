using Filo.Application.DTOs;
using Filo.Application.Features.VehicleService.Commands;
using Filo.Application.Features.VehicleService.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleServiceEndpoints
{
    public static void MapVehicleServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-services")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleServices");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleServiceQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleServiceDto>>.SuccessResponse(pagedResult, "Araç servis kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleServices");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var service = await sender.Send(new GetVehicleServiceByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleServiceDto>.SuccessResponse(service, "Araç servis kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleServiceById");

        group.MapPost("/", async (CreateVehicleServiceCommand request, ISender sender, IValidator<VehicleServiceDto.CreateRequest> validator) =>
        {
            var validationRequest = new VehicleServiceDto.CreateRequest
            {
                VehicleId = request.VehicleId,
                EntryDate = request.EntryDate,
                Odometer = request.Odometer,
                ServiceCompany = request.ServiceCompany,
                FailureDescription = request.FailureDescription
            };
            
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdService = await sender.Send(request);
            return Results.Created($"/vehicle-services/{createdService.VehicleServiceId}", 
                ApiResponse<VehicleServiceDto>.SuccessResponse(createdService, "Araç servis kaydı başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleService");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleServiceCommand request, ISender sender, IValidator<VehicleServiceDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new VehicleServiceDto.UpdateRequest
            {
                VehicleId = request.VehicleId,
                EntryDate = request.EntryDate,
                ExitDate = request.ExitDate,
                Odometer = request.Odometer,
                ServiceCompany = request.ServiceCompany,
                FailureDescription = request.FailureDescription,
                Cost = request.Cost,
                Status = request.Status,
                InvoiceNumber = request.InvoiceNumber
            };

            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç servis kaydı başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleService");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleServiceCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç servis kaydı başarıyla silindi."));
        })
        .WithName("DeleteVehicleService");
    }
}
