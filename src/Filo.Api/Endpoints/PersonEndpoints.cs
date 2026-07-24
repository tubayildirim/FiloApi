using Filo.Application.DTOs;
using Filo.Application.Features.Person.Commands;
using Filo.Application.Features.Person.Queries;
using Filo.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Filo.Api.Endpoints;

public static class PersonEndpoints
{
    public static void MapPersonEndpoints(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/person")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("Person");

        group.MapGet("/", async (ISender sender, [AsParameters] PaginationParams paginationParams) =>
        {
            var pagedResult = await sender.Send(new GetPagedPersonQuery(paginationParams));
            return Results.Ok(ApiResponse<PagedList<PersonDto>>.SuccessResponse(pagedResult, "Kişiler başarıyla getirildi."));
        })
        .WithName("GetPerson");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
        {
            var person = await sender.Send(new GetPersonByIdQuery(id));
            return Results.Ok(ApiResponse<PersonDto>.SuccessResponse(person, "Kişi başarıyla getirildi."));
        })
        .WithName("GetPersonById");

        group.MapPost("/", async (CreatePersonCommand request, ISender sender, IValidator<PersonDto.CreateRequest> validator) =>
        {
            var validationRequest = new PersonDto.CreateRequest
            {
                Name = request.Name,
                Surname = request.Surname,
                Tckn = request.Tckn,
                Age = request.Age,
                Gender = request.Gender
            };
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            var createdPerson = await sender.Send(request);
            return Results.Created($"/person/{createdPerson.Id}", ApiResponse<PersonDto>.SuccessResponse(createdPerson, "Kişi başarıyla oluşturuldu."));
        })
        .WithName("CreatePerson");

        group.MapPut("/{id:int}", async (int id, UpdatePersonCommand request, ISender sender, IValidator<PersonDto.UpdateRequest> validator) =>
        {
            request.Id = id;
            var validationRequest = new PersonDto.UpdateRequest
            {
                Name = request.Name,
                Surname = request.Surname,
                Tckn = request.Tckn,
                Age = request.Age,
                Gender = request.Gender
            };
            var validationResult = await validator.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new Application.Exceptions.ValidationException(errors);
            }

            await sender.Send(request);
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Kişi başarıyla güncellendi."));
        })
        .WithName("UpdatePerson");

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeletePersonCommand(id));
            return Results.Ok(ApiResponse<object>.SuccessResponse(new { }, "Kişi başarıyla silindi."));
        })
        .WithName("DeletePerson");
    }
}

