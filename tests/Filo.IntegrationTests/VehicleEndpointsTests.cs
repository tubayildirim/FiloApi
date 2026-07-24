using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Filo.Application.DTOs;
using Filo.Application.Features.Vehicles.Commands;
using Filo.Common.Models;
using Xunit;

namespace Filo.IntegrationTests;

public class VehicleEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VehicleEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreateVehicleCommand
        {
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2023,
            PlateNumber = $"34ABC{Guid.NewGuid().ToString()[..5].ToUpper()}",
            Color = "Beyaz",
            FuelType = "Hibrit",
            TransmissionType = "Otomatik"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/vehicles", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(command.Brand, apiResponse.Data.Brand);
        Assert.Equal(command.Model, apiResponse.Data.Model);
        Assert.Equal(command.PlateNumber, apiResponse.Data.PlateNumber);
    }
}
