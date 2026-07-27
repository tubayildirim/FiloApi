using Filo.Application.DTOs;
using Filo.Application.Features.VehicleMatchPerson.Commands;
using Filo.Application.Features.VehicleMatchPerson.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Filo.Api.Endpoints;

public static class VehicleMatchPersonEndpoints
{
    public static void MapVehicleMatchPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/vehicle-matches")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("VehicleMatches");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedVehicleMatchPersonQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<VehicleMatchPersonDto>>.SuccessResponse(pagedResult, "Araç-Kişi atamaları başarıyla getirildi."));
        })
        .WithName("GetVehicleMatches");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var match = await sender.Send(new GetVehicleMatchPersonByIdQuery(id));
            return Results.Ok(ApiResponse<VehicleMatchPersonDto>.SuccessResponse(match, "Araç-Kişi ataması başarıyla getirildi."));
        })
        .WithName("GetVehicleMatchById");

        group.MapPost("/", async (CreateVehicleMatchPersonCommand request, ISender sender, IValidator<VehicleMatchPersonDto.CreateRequest> validator) =>
        {
            var validationRequest = new VehicleMatchPersonDto.CreateRequest
            {
                VehicleId = request.VehicleId,
                PersonId = request.PersonId,
                AssignmentDate = request.AssignmentDate,
                AssignmentKm = request.AssignmentKm
            };
            
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdMatch = await sender.Send(request);
            return Results.Created($"/vehicle-matches/{createdMatch.VehiclePersonId}", 
                ApiResponse<VehicleMatchPersonDto>.SuccessResponse(createdMatch, "Araç-Kişi ataması başarıyla oluşturuldu."));
        })
        .WithName("CreateVehicleMatch");

        group.MapPut("/{id:int}", async (int id, UpdateVehicleMatchPersonCommand request, ISender sender, IValidator<VehicleMatchPersonDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new VehicleMatchPersonDto.UpdateRequest
            {
                VehicleId = request.VehicleId,
                PersonId = request.PersonId,
                AssignmentDate = request.AssignmentDate,
                AssignmentKm = request.AssignmentKm
            };

            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç-Kişi ataması başarıyla güncellendi."));
        })
        .WithName("UpdateVehicleMatch");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteVehicleMatchPersonCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Araç-Kişi ataması başarıyla silindi."));
        })
        .WithName("DeleteVehicleMatch");
    }
}
