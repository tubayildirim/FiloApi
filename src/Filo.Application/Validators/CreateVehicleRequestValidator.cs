using Filo.Application.DTOs;
using FluentValidation;

namespace Filo.Application.Validators;

public class CreateVehicleRequestValidator : AbstractValidator<VehicleDto.CreateRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka boş olamaz.")
            .MaximumLength(50).WithMessage("Marka en fazla 50 karakter olabilir.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model boş olamaz.")
            .MaximumLength(50).WithMessage("Model en fazla 50 karakter olabilir.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.Now.Year + 1).WithMessage($"Yıl 1900 ile {DateTime.Now.Year + 1} arasında olmalıdır.");

        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("Plaka boş olamaz.")
            .MaximumLength(15).WithMessage("Plaka en fazla 15 karakter olabilir.");
    }
}
