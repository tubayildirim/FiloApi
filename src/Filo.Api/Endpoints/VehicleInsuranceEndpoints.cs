using Filo.Application.DTOs;
using Filo.Application.Features.VehicleInsurance.Commands;
using Filo.Application.Features.VehicleInsurance.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleInsuranceEndpoints
{
    public static void MapVehicleInsuranceEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-insurances")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleInsurances");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleInsuranceQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleInsuranceDto>>.SuccessResponse(pagedResult, "Kasko/Sigorta kayıtları başarıyla getirildi."));
        })
        .WithName("GetVehicleInsurances");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var entity = await sender.Send(new GetVehicleInsuranceByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleInsuranceDto>.SuccessResponse(entity, "Kasko/Sigorta kaydı başarıyla getirildi."));
        })
        .WithName("GetVehicleInsuranceById");

        group.MapPost("/", async (CreateVehicleInsuranceCommand request, ISender sender, IValidator<VehicleInsuranceDto.CreateRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdEntity = await sender.Send(request);
            return Results.Created($"/vehicle-insurances/{createdEntity.Id}", ApiResponse<VehicleInsuranceDto>.SuccessResponse(createdEntity, "Kasko/Sigorta başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleInsurance");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleInsuranceCommand request, ISender sender, IValidator<VehicleInsuranceDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Kasko/Sigorta başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleInsurance");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleInsuranceCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Kasko/Sigorta başarıyla silindi."));
        })
        .WithName("DeleteVehicleInsurance");
    }
}
