using Filo.Application.DTOs;
using Filo.Common.Models;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Filo.IntegrationTests;

public class VehicleServiceEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VehicleServiceEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateVehicleService_AndActiveServiceAssignmentLock_ShouldWorkAsExpected()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Create a Vehicle
        var vehicleRequest = new VehicleDto.CreateRequest
        {
            Brand = "Ford",
            Model = "Transit",
            Year = 2021,
            PlateNumber = $"34SER{Guid.NewGuid().ToString()[..3]}",
            Color = "Red",
            FuelType = "Diesel",
            TransmissionType = "Manual"
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/v1/vehicles", vehicleRequest);
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var createdVehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Assert.NotNull(createdVehicleResult?.Data);
        var vehicleId = createdVehicleResult.Data.Id;

        // 2. Create a Person
        var personRequest = new PersonDto.CreateRequest
        {
            Name = "Mustafa",
            Surname = "Kaya",
            Tckn = $"12345{Guid.NewGuid().ToString()[..6]}",
            Age = 35,
            Gender = "Male"
        };
        var personResponse = await client.PostAsJsonAsync("/api/v1/persons", personRequest);
        Assert.Equal(HttpStatusCode.Created, personResponse.StatusCode);
        var createdPersonResult = await personResponse.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        Assert.NotNull(createdPersonResult?.Data);
        var personId = createdPersonResult.Data.Id;

        // 3. Put Vehicle into Service (Active)
        var serviceCommand = new
        {
            VehicleId = vehicleId,
            EntryDate = DateTime.UtcNow,
            Odometer = 100,
            ServiceCompany = "Ford Yetkili Servis",
            FailureDescription = "Kaporta Hasarı Onarımı"
        };
        var serviceResponse = await client.PostAsJsonAsync("/api/v1/vehicle-services", serviceCommand);
        Assert.Equal(HttpStatusCode.Created, serviceResponse.StatusCode);
        var serviceResult = await serviceResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleServiceDto>>();
        Assert.NotNull(serviceResult?.Data);
        Assert.Equal("Aktif", serviceResult.Data.Status);
        var serviceId = serviceResult.Data.VehicleServiceId;

        // 4. Try to assign the driver (should fail due to active service lock)
        var matchRequestFailed = new
        {
            VehicleId = vehicleId,
            PersonId = personId,
            AssignmentDate = DateTime.UtcNow,
            AssignmentKm = 100
        };
        var matchResponseFailed = await client.PostAsJsonAsync("/api/v1/vehicle-match-persons", matchRequestFailed);
        Assert.Equal(HttpStatusCode.BadRequest, matchResponseFailed.StatusCode);

        // 5. Complete the service (Status = Tamamlandı)
        var updateCommand = new
        {
            VehicleId = vehicleId,
            EntryDate = DateTime.UtcNow,
            ExitDate = DateTime.UtcNow.AddDays(2),
            Odometer = 100,
            ServiceCompany = "Ford Yetkili Servis",
            FailureDescription = "Kaporta Hasarı Onarımı",
            Cost = 15000.00m,
            Status = "Tamamlandı",
            InvoiceNumber = "INV-SERVICE-123"
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/vehicle-services/{serviceId}", updateCommand);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 6. Try to assign the driver again (should succeed since service is completed)
        var matchResponseSuccess = await client.PostAsJsonAsync("/api/v1/vehicle-match-persons", matchRequestFailed);
        Assert.Equal(HttpStatusCode.Created, matchResponseSuccess.StatusCode);
    }
}
