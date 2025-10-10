using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

public interface IScoreCalculator
{
    int Calculate(decimal monthlyIncome, DateBirth birthDate);
}