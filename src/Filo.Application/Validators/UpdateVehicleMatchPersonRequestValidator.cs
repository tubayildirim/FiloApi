using Filo.Application.DTOs;
using FluentValidation;
using System;

namespace Filo.Application.Validators;

public class UpdateVehicleMatchPersonRequestValidator : AbstractValidator<VehicleMatchPersonDto.UpdateRequest>
{
    public UpdateVehicleMatchPersonRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Araç ID'si 0'dan büyük olmalıdır.");

        RuleFor(x => x.PersonId)
            .GreaterThan(0).WithMessage("Kişi ID'si 0'dan büyük olmalıdır.");

        RuleFor(x => x.AssignmentDate)
            .NotEmpty().WithMessage("Atama tarihi boş olamaz.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Atama tarihi gelecek bir tarih olamaz.");

        RuleFor(x => x.AssignmentKm)
            .GreaterThanOrEqualTo(0).WithMessage("Kilometre bilgisi 0 veya daha büyük olmalıdır.");
    }
}
