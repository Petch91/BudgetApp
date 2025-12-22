using Entities.Contracts.Forms;
using Entities.Domain.Models;

namespace Entities.Domain.Mappers;

public static class MapperForm
{
    public static DepenseFixe ToDb(this DepenseFixeForm depenseFixe)
    {
        return new DepenseFixe
        {
            Intitule = depenseFixe.Intitule,
            Montant = depenseFixe.Montant,
            Frequence = depenseFixe.Frequence,
            EstDomiciliee = depenseFixe.EstDomiciliee,
            ReminderDaysBefore = depenseFixe.ReminderDaysBefore,
        };
    }
    public static DepenseFixe ToDb(this DepenseFixe depense, DepenseFixeForm depenseFixe)
    {
        depense.Intitule = depenseFixe.Intitule;
        depense.Montant = depenseFixe.Montant;
        depense.Frequence = depenseFixe.Frequence;
        depense.EstDomiciliee = depenseFixe.EstDomiciliee;
        depense.ReminderDaysBefore = depenseFixe.ReminderDaysBefore;
        depense.CategorieId = depenseFixe.Categorie.Id;
        return depense;
    }
    
    public static TransactionVariable ToDb(this TransactionVariableForm transactionVariable)
    {
        return new TransactionVariable
        {
            Intitule = transactionVariable.Intitule,
            Montant = transactionVariable.Montant,
            Date = transactionVariable.Date,
            TransactionType = transactionVariable.TransactionType,
            CategorieId = transactionVariable.CategorieId,
        };
    }
    public static TransactionVariable ToDb(this TransactionVariable transaction, TransactionVariableForm transactionVariable)
    {
        transaction.Intitule = transactionVariable.Intitule;
        transaction.Montant = transactionVariable.Montant;
        transaction.Date = transactionVariable.Date;
        transaction.TransactionType = transactionVariable.TransactionType;
        transaction.CategorieId = transactionVariable.CategorieId;
        return transaction;
    }
}