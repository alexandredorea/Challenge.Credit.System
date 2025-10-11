using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;

internal sealed class HighScorePolicy : IScorePolicy
{
    public void Apply(Proposal proposal)
    {
        proposal.Approve(limit: 5000m, cards: 2);
    }

    public bool IsApplicable(int score) => score > 500;
}