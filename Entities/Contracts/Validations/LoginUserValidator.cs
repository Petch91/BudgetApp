using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentValidation;

namespace Entities.Contracts.Validations;

public class LoginUserValidator : AbstractValidator<LoginForm>
{
    public LoginUserValidator()
    {
        RuleFor(p => p.Email).NotEmpty().WithMessage("L'email est requis.").EmailAddress().WithMessage("L'email doit être valide.");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Le mot de passe est requis.");
    }
}