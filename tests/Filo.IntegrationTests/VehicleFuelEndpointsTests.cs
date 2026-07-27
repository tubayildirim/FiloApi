using Filo.Application.DTOs;
using Filo.Common.Models;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Filo.IntegrationTests;

public class VehicleFuelEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VehicleFuelEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateVehicleFuel_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Create a Vehicle
        var vehicleRequest = new VehicleDto.CreateRequest
        {
            Brand = "Tesla",
            Model = "Model Y",
            Year = 2023,
            PlateNumber = $"34TES{Guid.NewGuid().ToString()[..3]}",
            Color = "Red",
            FuelType = "Electric",
            TransmissionType = "Automatic"
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/v1/vehicles", vehicleRequest);
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var createdVehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Assert.NotNull(createdVehicleResult?.Data);
        var vehicleId = createdVehicleResult.Data.Id;

        // 2. Create first VehicleFuel entry
        var fuelCommand1 = new
        {
            VehicleId = vehicleId,
            RefuelingDate = DateTime.UtcNow,
            Odometer = 500,
            Liters = 45.0,
            PricePerLiter = 42.50,
            ReceiptNumber = "R-10001"
        };

        // Act
        var response1 = await client.PostAsJsonAsync("/api/v1/vehicle-fuels", fuelCommand1);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        var apiResponse1 = await response1.Content.ReadFromJsonAsync<ApiResponse<VehicleFuelDto>>();
        Assert.NotNull(apiResponse1?.Data);
        Assert.True(apiResponse1.Success);
        Assert.Equal(vehicleId, apiResponse1.Data.VehicleId);
        Assert.Equal(500, apiResponse1.Data.Odometer);
        Assert.Equal(45.0, apiResponse1.Data.Liters);
        Assert.Equal(1912.50m, apiResponse1.Data.TotalPrice); // 45 * 42.50 = 1912.50

        // 3. Create second VehicleFuel entry with LOWER odometer (should fail)
        var fuelCommandLowerOdometer = new
        {
            VehicleId = vehicleId,
            RefuelingDate = DateTime.UtcNow,
            Odometer = 499, // less than 500
            Liters = 20.0,
            PricePerLiter = 43.00,
            ReceiptNumber = "R-10002"
        };

        var responseFailed = await client.PostAsJsonAsync("/api/v1/vehicle-fuels", fuelCommandLowerOdometer);
        Assert.Equal(HttpStatusCode.BadRequest, responseFailed.StatusCode);
    }
}
