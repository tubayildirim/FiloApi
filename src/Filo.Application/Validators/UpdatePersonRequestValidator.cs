using Filo.Application.DTOs;
using FluentValidation;

namespace Filo.Application.Validators;

public class UpdatePersonRequestValidator : AbstractValidator<PersonDto.UpdateRequest>
{
    public UpdatePersonRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ad boş olamaz.");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Soyad boş olamaz.");

        RuleFor(x => x.Tckn)
            .NotEmpty().WithMessage("TC Kimlik No boş olamaz.")
            .Length(11).WithMessage("TC Kimlik No 11 haneli olmalıdır.");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120).WithMessage("Yaş 1 ile 120 arasında olmalıdır.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Cinsiyet boş olamaz.");
    }
}
