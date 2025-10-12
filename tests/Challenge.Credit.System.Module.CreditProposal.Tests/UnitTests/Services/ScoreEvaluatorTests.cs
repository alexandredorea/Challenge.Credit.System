using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditProposal.Tests.UnitTests.Services;

public sealed class ScoreEvaluatorTests
{
    private readonly IScoreEvaluator _scoreEvaluator;

    public ScoreEvaluatorTests()
    {
        var policies = new List<IScorePolicy>
        {
            new LowScorePolicy(),
            new MediumScorePolicy(),
            new HighScorePolicy()
        };

        _scoreEvaluator = new ScoreEvaluator(policies);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Evaluate_ComScoreBaixo_DeveRejeitarProposta(int score)
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 1000m,
            score: score);

        // Act
        _scoreEvaluator.Evaluate(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Rejected);
        proposal.AvaliableLimit.Should().Be(0);
        proposal.CardsAllowed.Should().Be(0);
        proposal.RejectionReason.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(300)]
    [InlineData(500)]
    public void Evaluate_ComScoreMedio_DeveAprovarCom1Cartao(int score)
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alice Dórea",
            monthlyIncome: 3000m,
            score: score);

        // Act
        _scoreEvaluator.Evaluate(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Approved);
        proposal.AvaliableLimit.Should().Be(1000m);
        proposal.CardsAllowed.Should().Be(1);
        proposal.RejectionReason.Should().BeNull();
    }

    [Theory]
    [InlineData(501)]
    [InlineData(750)]
    [InlineData(1000)]
    public void Evaluate_ComScoreAlto_DeveAprovarCom2Cartoes(int score)
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 10000m,
            score: score);

        // Act
        _scoreEvaluator.Evaluate(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Approved);
        proposal.AvaliableLimit.Should().Be(5000m);
        proposal.CardsAllowed.Should().Be(2);
        proposal.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Evaluate_DeveDefinirDataDeAvaliacao()
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 5000m,
            score: 600);

        // Act
        _scoreEvaluator.Evaluate(proposal);

        // Assert
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Evaluate_ComPropostaPendente_DeveAlterarStatus()
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 5000m,
            score: 600);

        // Assert - Status inicial
        proposal.Status.Should().Be(StatusProposal.Pending);

        // Act
        _scoreEvaluator.Evaluate(proposal);

        // Assert - Status após avaliação
        proposal.Status.Should().NotBe(StatusProposal.Pending);
    }

    [Fact]
    public void Evaluate_ComMultiplasPropostas_DeveAplicarPoliticaCorretaParaCada()
    {
        // Arrange
        var proposalLowScore = Proposal.Create(Guid.NewGuid(), "Cliente 1", 1000m, 50);
        var proposalMediumScore = Proposal.Create(Guid.NewGuid(), "Cliente 2", 3000m, 300);
        var proposalHighScore = Proposal.Create(Guid.NewGuid(), "Cliente 3", 10000m, 800);

        // Act
        _scoreEvaluator.Evaluate(proposalLowScore);
        _scoreEvaluator.Evaluate(proposalMediumScore);
        _scoreEvaluator.Evaluate(proposalHighScore);

        // Assert
        proposalLowScore.Status.Should().Be(StatusProposal.Rejected);
        proposalLowScore.CardsAllowed.Should().Be(0);

        proposalMediumScore.Status.Should().Be(StatusProposal.Approved);
        proposalMediumScore.CardsAllowed.Should().Be(1);

        proposalHighScore.Status.Should().Be(StatusProposal.Approved);
        proposalHighScore.CardsAllowed.Should().Be(2);
    }
}