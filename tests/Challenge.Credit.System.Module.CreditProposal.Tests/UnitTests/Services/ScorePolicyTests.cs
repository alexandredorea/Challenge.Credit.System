using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditProposal.Tests.UnitTests.Services;

public class ScorePolicyTests
{
    #region Testes para score baixo

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void LowScorePolicy_IsApplicable_ComScoreAte100_DeveRetornarTrue(int score)
    {
        // Arrange
        IScorePolicy policy = new LowScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    [InlineData(1000)]
    public void LowScorePolicy_IsApplicable_ComScoreAcimaDe100_DeveRetornarFalse(int score)
    {
        // Arrange
        IScorePolicy policy = new LowScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeFalse();
    }

    [Fact]
    public void LowScorePolicy_Apply_DeveRejeitarProposta()
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 1000m,
            score: 50);

        IScorePolicy policy = new LowScorePolicy();

        // Act
        policy.Apply(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Rejected);
        proposal.AvaliableLimit.Should().Be(0);
        proposal.CardsAllowed.Should().Be(0);
        proposal.RejectionReason.Should().Be("Score insuficiente para aprovação de crédito");
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion Testes para score baixo

    #region Testes para score medio ou intermediario

    [Theory]
    [InlineData(101)]
    [InlineData(300)]
    [InlineData(500)]
    public void MediumScorePolicy_IsApplicable_ComScoreEntre101E500_DeveRetornarTrue(int score)
    {
        // Arrange
        IScorePolicy policy = new MediumScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeTrue();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(501)]
    [InlineData(1000)]
    public void MediumScorePolicy_IsApplicable_ComScoreForaDoRange_DeveRetornarFalse(int score)
    {
        // Arrange
        IScorePolicy policy = new MediumScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeFalse();
    }

    [Fact]
    public void MediumScorePolicy_Apply_DeveAprovarPropostaCom1CartaoELimite1000()
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Maria Silva",
            monthlyIncome: 3000m,
            score: 300);

        IScorePolicy policy = new MediumScorePolicy();

        // Act
        policy.Apply(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Approved);
        proposal.AvaliableLimit.Should().Be(1000m);
        proposal.CardsAllowed.Should().Be(1);
        proposal.RejectionReason.Should().BeNull();
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion Testes para score medio ou intermediario

    #region Testes para score alto

    [Theory]
    [InlineData(501)]
    [InlineData(750)]
    [InlineData(1000)]
    public void HighScorePolicy_IsApplicable_ComScoreAcimaDe500_DeveRetornarTrue(int score)
    {
        // Arrange
        IScorePolicy policy = new HighScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    public void HighScorePolicy_IsApplicable_ComScoreAte500_DeveRetornarFalse(int score)
    {
        // Arrange
        IScorePolicy policy = new HighScorePolicy();

        // Act
        var isApplicable = policy.IsApplicable(score);

        // Assert
        isApplicable.Should().BeFalse();
    }

    [Fact]
    public void HighScorePolicy_Apply_DeveAprovarPropostaCom2CartoesELimite5000()
    {
        // Arrange
        var proposal = Proposal.Create(
            clientId: Guid.NewGuid(),
            clientName: "Alexandre Dórea",
            monthlyIncome: 10000m,
            score: 800);

        IScorePolicy policy = new HighScorePolicy();

        // Act
        policy.Apply(proposal);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Approved);
        proposal.AvaliableLimit.Should().Be(5000m);
        proposal.CardsAllowed.Should().Be(2);
        proposal.RejectionReason.Should().BeNull();
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion Testes para score alto

    #region Testes com casos extremos

    [Fact]
    public void Policies_ComScore100_DeveSomenteAplicarLowScorePolicy()
    {
        // Arrange
        var lowPolicy = new LowScorePolicy();
        var mediumPolicy = new MediumScorePolicy();
        var highPolicy = new HighScorePolicy();
        var score = 100;

        // Act & Assert
        lowPolicy.IsApplicable(score).Should().BeTrue();
        mediumPolicy.IsApplicable(score).Should().BeFalse();
        highPolicy.IsApplicable(score).Should().BeFalse();
    }

    [Fact]
    public void Policies_ComScore101_DeveSomenteAplicarMediumScorePolicy()
    {
        // Arrange
        var lowPolicy = new LowScorePolicy();
        var mediumPolicy = new MediumScorePolicy();
        var highPolicy = new HighScorePolicy();
        var score = 101;

        // Act & Assert
        lowPolicy.IsApplicable(score).Should().BeFalse();
        mediumPolicy.IsApplicable(score).Should().BeTrue();
        highPolicy.IsApplicable(score).Should().BeFalse();
    }

    [Fact]
    public void Policies_ComScore500_DeveSomenteAplicarMediumScorePolicy()
    {
        // Arrange
        var lowPolicy = new LowScorePolicy();
        var mediumPolicy = new MediumScorePolicy();
        var highPolicy = new HighScorePolicy();
        var score = 500;

        // Act & Assert
        lowPolicy.IsApplicable(score).Should().BeFalse();
        mediumPolicy.IsApplicable(score).Should().BeTrue();
        highPolicy.IsApplicable(score).Should().BeFalse();
    }

    [Fact]
    public void Policies_ComScore501_DeveSomenteAplicarHighScorePolicy()
    {
        // Arrange
        var lowPolicy = new LowScorePolicy();
        var mediumPolicy = new MediumScorePolicy();
        var highPolicy = new HighScorePolicy();
        var score = 501;

        // Act & Assert
        lowPolicy.IsApplicable(score).Should().BeFalse();
        mediumPolicy.IsApplicable(score).Should().BeFalse();
        highPolicy.IsApplicable(score).Should().BeTrue();
    }

    #endregion Testes com casos extremos
}