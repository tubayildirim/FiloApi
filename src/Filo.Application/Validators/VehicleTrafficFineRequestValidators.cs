using Filo.Application.DTOs;
using FluentValidation;

namespace Filo.Application.Validators;

public class CreateVehicleTrafficFineRequestValidator : AbstractValidator<VehicleTrafficFineDto.CreateRequest>
{
    public CreateVehicleTrafficFineRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}

public class UpdateVehicleTrafficFineRequestValidator : AbstractValidator<VehicleTrafficFineDto.UpdateRequest>
{
    public UpdateVehicleTrafficFineRequestValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Araç ID zorunludur.");
    }
}
