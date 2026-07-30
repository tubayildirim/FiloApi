using Filo.Application.DTOs;
using Filo.Application.Features.VehicleToll.Commands;
using Filo.Application.Features.VehicleToll.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleTollEndpoints
{
    public static void MapVehicleTollEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-tolls")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleTolls");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleTollQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleTollDto>>.SuccessResponse(pagedResult, "HGS/OGS Geçiş kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleTolls");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var entity = await sender.Send(new GetVehicleTollByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleTollDto>.SuccessResponse(entity, "HGS/OGS Geçiş kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleTollById");

        group.MapPost("/", async (CreateVehicleTollCommand request, ISender sender, IValidator<VehicleTollDto.CreateRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdEntity = await sender.Send(request);
            return Results.Created($"/vehicle-tolls/{createdEntity.Id}", ApiResponse<VehicleTollDto>.SuccessResponse(createdEntity, "HGS/OGS Geçiş başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleToll");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleTollCommand request, ISender sender, IValidator<VehicleTollDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "HGS/OGS Geçiş başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleToll");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleTollCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "HGS/OGS Geçiş başarıyla silindi."));
        })
        .WithName("DeleteVehicleToll");
    }
}
