using Filo.Application.DTOs;
using Filo.Application.Features.VehicleTrafficFine.Commands;
using Filo.Application.Features.VehicleTrafficFine.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleTrafficFineEndpoints
{
    public static void MapVehicleTrafficFineEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-traffic-fines")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleTrafficFines");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleTrafficFineQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleTrafficFineDto>>.SuccessResponse(pagedResult, "Trafik Cezası kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleTrafficFines");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var entity = await sender.Send(new GetVehicleTrafficFineByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleTrafficFineDto>.SuccessResponse(entity, "Trafik Cezası kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleTrafficFineById");

        group.MapPost("/", async (CreateVehicleTrafficFineCommand request, ISender sender, IValidator<VehicleTrafficFineDto.CreateRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdEntity = await sender.Send(request);
            return Results.Created($"/vehicle-traffic-fines/{createdEntity.Id}", ApiResponse<VehicleTrafficFineDto>.SuccessResponse(createdEntity, "Trafik Cezası başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleTrafficFine");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleTrafficFineCommand request, ISender sender, IValidator<VehicleTrafficFineDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Trafik Cezası başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleTrafficFine");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleTrafficFineCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Trafik Cezası başarıyla silindi."));
        })
        .WithName("DeleteVehicleTrafficFine");
    }
}
