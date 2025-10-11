using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;

internal sealed class ScoreEvaluator(IEnumerable<IScorePolicy> policies) : IScoreEvaluator
{
    public void Evaluate(Proposal proposal)
    {
        var policy = policies.FirstOrDefault(p => p.IsApplicable(proposal.Score));
        policy?.Apply(proposal);
    }
}