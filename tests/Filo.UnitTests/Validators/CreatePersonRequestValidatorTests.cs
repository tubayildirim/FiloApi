using Filo.Application.DTOs;
using Filo.Application.Validators;
using Xunit;

namespace Filo.UnitTests.Validators;

public class CreatePersonRequestValidatorTests
{
    private readonly CreatePersonRequestValidator _validator;

    public CreatePersonRequestValidatorTests()
    {
        _validator = new CreatePersonRequestValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameIsEmpty()
    {
        var request = new PersonDto.CreateRequest
        {
            Name = "",
            Surname = "Yildirim",
            Tckn = "12345678901",
            Age = 30,
            Gender = "Erkek"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Name));
    }

    [Fact]
    public void Validator_ShouldBeValid_WhenRequestIsCorrect()
    {
        var request = new PersonDto.CreateRequest
        {
            Name = "Ahmet",
            Surname = "Yildirim",
            Tckn = "12345678901",
            Age = 30,
            Gender = "Erkek"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
