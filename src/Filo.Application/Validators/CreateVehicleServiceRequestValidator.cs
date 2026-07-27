using Filo.Application.DTOs;
using FluentValidation;
using System;

namespace Filo.Application.Validators;

public class CreateVehicleServiceRequestValidator : AbstractValidator<VehicleServiceDto.CreateRequest>
{
    public CreateVehicleServiceRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Araç ID'si 0'dan büyük olmalıdır.");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("Servise giriş tarihi boş olamaz.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Servise giriş tarihi gelecek bir tarih olamaz.");

        RuleFor(x => x.Odometer)
            .GreaterThanOrEqualTo(0).WithMessage("Kilometre bilgisi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ServiceCompany)
            .NotEmpty().WithMessage("Servis firması adı boş olamaz.")
            .MaximumLength(200).WithMessage("Servis firması adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.FailureDescription)
            .NotEmpty().WithMessage("Arıza/Hasar açıklaması boş olamaz.")
            .MaximumLength(1000).WithMessage("Arıza/Hasar açıklaması en fazla 1000 karakter olabilir.");
    }
}
