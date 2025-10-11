using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

public interface IScoreEvaluator
{
    void Evaluate(Proposal proposal);
}