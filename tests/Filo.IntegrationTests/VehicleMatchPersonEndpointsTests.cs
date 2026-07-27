using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Filo.Application.DTOs;
using Filo.Application.Features.VehicleMatchPerson.Commands;
using Filo.Common.Models;
using Xunit;

namespace Filo.IntegrationTests;

public class VehicleMatchPersonEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VehicleMatchPersonEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateVehicleMatch_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Create a Person
        var personRequest = new PersonDto.CreateRequest
        {
            Name = "John",
            Surname = "Doe",
            Tckn = $"1234{Guid.NewGuid().ToString()[..7]}", // ensure unique Tckn
            Age = 35,
            Gender = "Erkek"
        };
        var personResponse = await client.PostAsJsonAsync("/api/v1/person", personRequest);
        Assert.Equal(HttpStatusCode.Created, personResponse.StatusCode);
        var createdPersonResult = await personResponse.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        Assert.NotNull(createdPersonResult?.Data);
        int personId = createdPersonResult.Data.Id;

        // 2. Create a Vehicle
        var vehicleRequest = new VehicleDto.CreateRequest
        {
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2023,
            PlateNumber = $"34ABC{Guid.NewGuid().ToString()[..5].ToUpper()}",
            Color = "Beyaz",
            FuelType = "Hibrit",
            TransmissionType = "Otomatik"
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/v1/vehicles", vehicleRequest);
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var createdVehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Assert.NotNull(createdVehicleResult?.Data);
        int vehicleId = createdVehicleResult.Data.Id;

        // 3. Create the Match
        var matchCommand = new CreateVehicleMatchPersonCommand
        {
            VehicleId = vehicleId,
            PersonId = personId,
            AssignmentDate = DateTime.UtcNow.Date,
            AssignmentKm = 15000
        };

        // Act
        var matchResponse = await client.PostAsJsonAsync("/api/v1/vehicle-matches", matchCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);
        var apiResponse = await matchResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleMatchPersonDto>>();
        Assert.NotNull(apiResponse?.Data);
        Assert.True(apiResponse.Success);
        Assert.Equal(vehicleId, apiResponse.Data.VehicleId);
        Assert.Equal(personId, apiResponse.Data.PersonId);
        Assert.Equal(matchCommand.AssignmentKm, apiResponse.Data.AssignmentKm);
    }
}
