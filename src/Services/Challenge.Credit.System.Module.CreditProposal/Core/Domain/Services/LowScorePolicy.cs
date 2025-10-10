using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;

internal sealed class LowScorePolicy : IScorePolicy
{
    public bool IsApplicable(int score) => score <= 100;
    public void Apply(Proposal proposal)
    {
        proposal.Reject("Score insuficiente para aprovação de crédito");        
    }
}
