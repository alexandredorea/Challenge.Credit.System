using System.Reflection;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditProposal.Tests.UnitTests.Entities;

public sealed class ProposalTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarProposta()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var clientName = "Alexandre Dórea";
        var monthlyIncome = 5000m;
        var score = 600;

        // Act
        var proposal = Proposal.Create(clientId, clientName, monthlyIncome, score);

        // Assert
        proposal.Should().NotBeNull();
        proposal.Id.Should().NotBeEmpty();
        proposal.ClientId.Should().Be(clientId);
        proposal.ClientName.Should().Be(clientName);
        proposal.MonthlyIncome.Should().Be(monthlyIncome);
        proposal.Score.Should().Be(score);
        proposal.Status.Should().Be(StatusProposal.Pending);
        proposal.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        proposal.AvaliableLimit.Should().Be(0);
        proposal.CardsAllowed.Should().Be(0);
        proposal.RejectionReason?.Should().BeNull();
    }

    [Fact]
    public void Create_DeveGerarIdUnico()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var clientName = "Alexandre Dórea";
        var monthlyIncome = 5000m;
        var score = 600;

        // Act
        var proposal1 = Proposal.Create(clientId, clientName, monthlyIncome, score);
        var proposal2 = Proposal.Create(clientId, clientName, monthlyIncome, score);

        // Assert
        proposal1.Id.Should().NotBe(proposal2.Id);
    }

    [Fact]
    public void Create_DeveIniciarComStatusPending()
    {
        // Arrange & Act
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", 5000m, 600);

        // Assert
        proposal.Status.Should().Be(StatusProposal.Pending);
    }

    [Fact]
    public void Approve_DeveAlterarStatusParaApproved()
    {
        // Arrange
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", 5000m, 600);

        // Act
        // estou usando reflection para chamar método que é internal
        var approveMethod = typeof(Proposal).GetMethod("Approve", BindingFlags.NonPublic | BindingFlags.Instance);
        approveMethod?.Invoke(proposal, new object[] { 5000m, 2 });

        // Assert
        proposal.Status.Should().Be(StatusProposal.Approved);
        proposal.AvaliableLimit.Should().Be(5000m);
        proposal.CardsAllowed.Should().Be(2);
        proposal.RejectionReason.Should().BeNull();
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Reject_DeveAlterarStatusParaRejected()
    {
        // Arrange
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", 1000m, 50);
        var rejectionReason = "Score insuficiente";

        // Act
        var rejectMethod = typeof(Proposal).GetMethod("Reject", BindingFlags.NonPublic | BindingFlags.Instance);
        rejectMethod?.Invoke(proposal, new object[] { rejectionReason });

        // Assert
        proposal.Status.Should().Be(StatusProposal.Rejected);
        proposal.AvaliableLimit.Should().Be(0);
        proposal.CardsAllowed.Should().Be(0);
        proposal.RejectionReason.Should().Be(rejectionReason);
        proposal.EvaluationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ComScoreNegativo_DeveCriarProposta()
    {
        // Arrange
        var score = -100;

        // Act
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", 5000m, score);

        // Assert
        proposal.Should().NotBeNull();
        proposal.Score.Should().Be(score);
    }

    [Fact]
    public void Create_ComScoreAcimaDe1000_DeveCriarProposta()
    {
        // Arrange
        var score = 1500;

        // Act
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", 5000m, score);

        // Assert
        proposal.Should().NotBeNull();
        proposal.Score.Should().Be(score);
    }

    [Fact]
    public void Create_ComRendaZero_DeveCriarProposta()
    {
        // Arrange
        var monthlyIncome = 0m;

        // Act
        var proposal = Proposal.Create(Guid.NewGuid(), "Alexandre Dórea", monthlyIncome, 600);

        // Assert
        proposal.Should().NotBeNull();
        proposal.MonthlyIncome.Should().Be(monthlyIncome);
    }
}