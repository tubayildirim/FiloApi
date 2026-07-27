using Filo.Application.DTOs;
using Filo.Common.Models;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Filo.IntegrationTests;

public class VehicleMaintenanceEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VehicleMaintenanceEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateVehicleMaintenance_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Create a Vehicle
        var vehicleRequest = new VehicleDto.CreateRequest
        {
            Brand = "Toyota",
            Model = "Proace",
            Year = 2022,
            PlateNumber = $"34PRO{Guid.NewGuid().ToString()[..3]}",
            Color = "White",
            FuelType = "Diesel",
            TransmissionType = "Manual"
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/v1/vehicles", vehicleRequest);
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var createdVehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Assert.NotNull(createdVehicleResult?.Data);
        var vehicleId = createdVehicleResult.Data.Id;

        // 2. Add a Refueling log at 2000 KM
        var fuelCommand = new
        {
            VehicleId = vehicleId,
            RefuelingDate = DateTime.UtcNow.AddHours(-1),
            Odometer = 2000,
            Liters = 55.0,
            PricePerLiter = 41.20,
            ReceiptNumber = "R-FUEL"
        };
        var fuelResponse = await client.PostAsJsonAsync("/api/v1/vehicle-fuels", fuelCommand);
        Assert.Equal(HttpStatusCode.Created, fuelResponse.StatusCode);

        // 3. Try to add maintenance at 1999 KM (should fail because of fuel odometer)
        var maintenanceFailed = new
        {
            VehicleId = vehicleId,
            MaintenanceDate = DateTime.UtcNow,
            Odometer = 1999, // less than 2000
            Description = "Yağ Değişimi",
            Cost = 3500.00m,
            MaintenanceType = "Periyodik",
            NextMaintenanceDate = DateTime.UtcNow.AddMonths(6),
            NextMaintenanceKm = 12000
        };
        var responseFailed = await client.PostAsJsonAsync("/api/v1/vehicle-maintenances", maintenanceFailed);
        Assert.Equal(HttpStatusCode.BadRequest, responseFailed.StatusCode);

        // 4. Add maintenance at 2010 KM (should succeed)
        var maintenanceSuccess1 = new
        {
            VehicleId = vehicleId,
            MaintenanceDate = DateTime.UtcNow,
            Odometer = 2010, // greater than 2000
            Description = "Fren Balata Değişimi",
            Cost = 4500.00m,
            MaintenanceType = "Onarım",
            NextMaintenanceDate = DateTime.UtcNow.AddMonths(12),
            NextMaintenanceKm = 30000
        };
        var responseSuccess1 = await client.PostAsJsonAsync("/api/v1/vehicle-maintenances", maintenanceSuccess1);
        Assert.Equal(HttpStatusCode.Created, responseSuccess1.StatusCode);
        var apiResponse = await responseSuccess1.Content.ReadFromJsonAsync<ApiResponse<VehicleMaintenanceDto>>();
        Assert.NotNull(apiResponse?.Data);
        Assert.Equal(2010, apiResponse.Data.Odometer);

        // 5. Try to add maintenance at 2009 KM (should fail because of previous maintenance odometer)
        var maintenanceFailed2 = new
        {
            VehicleId = vehicleId,
            MaintenanceDate = DateTime.UtcNow.AddMinutes(5),
            Odometer = 2009, // less than 2010
            Description = "Far Ampul Değişimi",
            Cost = 250.00m,
            MaintenanceType = "Düzeltici"
        };
        var responseFailed2 = await client.PostAsJsonAsync("/api/v1/vehicle-maintenances", maintenanceFailed2);
        Assert.Equal(HttpStatusCode.BadRequest, responseFailed2.StatusCode);
    }
}
