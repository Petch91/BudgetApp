using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentValidation;

namespace Entities.Contracts.Validations;

public class RegisterUserValidator : AbstractValidator<RegisterForm>
{
    public RegisterUserValidator()
    {
        RuleFor(p => p.Username).NotEmpty().WithMessage("Le nom d'utilisateur est requis.");
        RuleFor(p => p.Email).NotEmpty().WithMessage("L'email est requis.").EmailAddress().WithMessage("L'email doit être valide.");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Le mot de passe est requis.").MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une lettre majuscule.")
            .Matches("[a-z]").WithMessage("Le mot de passe doit contenir au moins une lettre minuscule.")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial.")
            .Equal(p => p.ConfirmPassword).WithMessage("Les mots de passe ne correspondent pas.");
    }
}