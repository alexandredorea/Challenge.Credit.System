using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;

internal sealed class MediumScorePolicy : IScorePolicy
{
    public void Apply(Proposal proposal)
    {
        proposal.Approve(limit: 1000m, cards: 1);
    }

    public bool IsApplicable(int score) => score is > 100 and <= 500;
}