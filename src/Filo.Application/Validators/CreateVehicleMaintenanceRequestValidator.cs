using Filo.Application.DTOs;
using FluentValidation;
using System;

namespace Filo.Application.Validators;

public class CreateVehicleMaintenanceRequestValidator : AbstractValidator<VehicleMaintenanceDto.CreateRequest>
{
    public CreateVehicleMaintenanceRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Araç ID'si 0'dan büyük olmalıdır.");

        RuleFor(x => x.MaintenanceDate)
            .NotEmpty().WithMessage("Bakım tarihi boş olamaz.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Bakım tarihi gelecek bir tarih olamaz.");

        RuleFor(x => x.Odometer)
            .GreaterThanOrEqualTo(0).WithMessage("Kilometre bilgisi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Bakım açıklaması boş olamaz.")
            .MaximumLength(500).WithMessage("Bakım açıklaması en fazla 500 karakter olabilir.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Bakım maliyeti 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.MaintenanceType)
            .NotEmpty().WithMessage("Bakım türü boş olamaz.")
            .MaximumLength(50).WithMessage("Bakım türü en fazla 50 karakter olabilir.");

        RuleFor(x => x.NextMaintenanceKm)
            .GreaterThan(x => x.Odometer)
            .When(x => x.NextMaintenanceKm.HasValue)
            .WithMessage("Bir sonraki bakım kilometresi, mevcut bakım kilometresinden büyük olmalıdır.");

        RuleFor(x => x.NextMaintenanceDate)
            .GreaterThan(x => x.MaintenanceDate)
            .When(x => x.NextMaintenanceDate.HasValue)
            .WithMessage("Bir sonraki bakım tarihi, mevcut bakım tarihinden ileri bir tarih olmalıdır.");
    }
}
