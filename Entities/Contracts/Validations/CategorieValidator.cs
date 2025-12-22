using Entities.Domain.Models;
using FluentValidation;

namespace Entities.Contracts.Validations;

public class CategorieValidator :  AbstractValidator<Categorie>
{
    public CategorieValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Le nom est requis.").MaximumLength(50).WithMessage("Le nom ne peut pas dépasser 50 caractères");
        RuleFor(p => p.Icon).MaximumLength(25).WithMessage("Pas plus de 25 caractères");
    }
}