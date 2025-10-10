using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

public interface IScorePolicy
{
    bool IsApplicable(int score);
    void Apply(Proposal proposal);
}
