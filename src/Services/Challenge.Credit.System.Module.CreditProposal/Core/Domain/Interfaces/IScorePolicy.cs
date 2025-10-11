using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;

public interface IScorePolicy
{
    void Apply(Proposal proposal);

    bool IsApplicable(int score);
}