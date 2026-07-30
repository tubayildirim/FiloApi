using Filo.Application.DTOs;
using FluentValidation;

namespace Filo.Application.Validators;

public class CreateVehicleTollRequestValidator : AbstractValidator<VehicleTollDto.CreateRequest>
{
    public CreateVehicleTollRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}

public class UpdateVehicleTollRequestValidator : AbstractValidator<VehicleTollDto.UpdateRequest>
{
    public UpdateVehicleTollRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}
