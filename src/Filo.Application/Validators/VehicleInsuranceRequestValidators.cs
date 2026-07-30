using Filo.Application.DTOs;
using FluentValidation;

namespace Filo.Application.Validators;

public class CreateVehicleInsuranceRequestValidator : AbstractValidator<VehicleInsuranceDto.CreateRequest>
{
    public CreateVehicleInsuranceRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}

public class UpdateVehicleInsuranceRequestValidator : AbstractValidator<VehicleInsuranceDto.UpdateRequest>
{
    public UpdateVehicleInsuranceRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}
