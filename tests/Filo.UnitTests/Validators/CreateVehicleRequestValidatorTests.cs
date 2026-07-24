using Filo.Application.DTOs;
using Filo.Application.Validators;
using Xunit;

namespace Filo.UnitTests.Validators;

public class CreateVehicleRequestValidatorTests
{
    private readonly CreateVehicleRequestValidator _validator;

    public CreateVehicleRequestValidatorTests()
    {
        _validator = new CreateVehicleRequestValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenBrandIsEmpty()
    {
        // Arrange
        var request = new VehicleDto.CreateRequest
        {
            Brand = "",
            Model = "Focus",
            Year = 2020,
            PlateNumber = "34ABC123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Brand));
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenModelIsEmpty()
    {
        // Arrange
        var request = new VehicleDto.CreateRequest
        {
            Brand = "Ford",
            Model = "",
            Year = 2020,
            PlateNumber = "34ABC123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Model));
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenYearIsInvalid()
    {
        // Arrange
        var request = new VehicleDto.CreateRequest
        {
            Brand = "Ford",
            Model = "Focus",
            Year = 1899,
            PlateNumber = "34ABC123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Year));
    }

    [Fact]
    public void Validator_ShouldBeValid_WhenRequestIsCorrect()
    {
        // Arrange
        var request = new VehicleDto.CreateRequest
        {
            Brand = "Ford",
            Model = "Focus",
            Year = 2020,
            PlateNumber = "34ABC123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
