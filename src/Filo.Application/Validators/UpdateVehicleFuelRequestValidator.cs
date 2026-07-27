using Filo.Application.DTOs;
using FluentValidation;
using System;

namespace Filo.Application.Validators;

public class UpdateVehicleFuelRequestValidator : AbstractValidator<VehicleFuelDto.UpdateRequest>
{
    public UpdateVehicleFuelRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Araç ID'si 0'dan büyük olmalıdır.");

        RuleFor(x => x.RefuelingDate)
            .NotEmpty().WithMessage("Yakıt alım tarihi boş olamaz.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Yakıt alım tarihi gelecek bir tarih olamaz.");

        RuleFor(x => x.Odometer)
            .GreaterThanOrEqualTo(0).WithMessage("Kilometre bilgisi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Liters)
            .GreaterThan(0).WithMessage("Alınan litre bilgisi 0'dan büyük olmalıdır.");

        RuleFor(x => x.PricePerLiter)
            .GreaterThan(0).WithMessage("Litre fiyatı 0'dan büyük olmalıdır.");

        RuleFor(x => x.ReceiptNumber)
            .MaximumLength(50).WithMessage("Fiş/Fatura numarası en fazla 50 karakter olabilir.");
    }
}
