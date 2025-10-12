using Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditCard.Tests.UnitTests.Entities;

public sealed class CardTests
{
    [Fact]
    public void Create_DeveCriarCartao_ComDadosValidos()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientName = "Alexandre Dórea";
        var cardNumber = "1111111111111111";
        var cvv = "123";
        var expirationDate = DateTime.UtcNow.AddYears(5);
        var limit = 5000m;

        // Act
        var card = Card.Create(proposalId, clientId, clientName, cardNumber, cvv, expirationDate, limit);

        // Assert
        card.Should().NotBeNull();
        card.Id.Should().NotBeEmpty();
        card.ProposalId.Should().Be(proposalId);
        card.ClientId.Should().Be(clientId);
        card.ClientName.Should().Be(clientName);
        card.Number.Should().Be(cardNumber);
        card.Cvv.Should().Be(cvv);
        card.ExpirationDate.Should().Be(expirationDate);
        card.TotalLimit.Should().Be(limit);
        card.AvailableLimit.Should().Be(limit);
        card.Status.Should().Be(CardStatus.Issued);
        card.IssueDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_DeveCriarCartao_ComIdUnico()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientName = "Alexandre Dórea";
        var cardNumber = "1111111111111111";
        var cvv = "123";
        var expirationDate = DateTime.UtcNow.AddYears(5);
        var limit = 5000m;

        // Act
        var card1 = Card.Create(proposalId, clientId, clientName, cardNumber, cvv, expirationDate, limit);
        var card2 = Card.Create(proposalId, clientId, clientName, cardNumber, cvv, expirationDate, limit);

        // Assert
        card1.Id.Should().NotBe(card2.Id);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComStatusCriado()
    {
        // Arrange & Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            "1111111111111111",
            "123",
            DateTime.UtcNow.AddYears(5),
            5000m);

        // Assert
        card.Status.Should().Be(CardStatus.Issued);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComLimiteInformado()
    {
        // Arrange
        var limit = 5000m;

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            "1111111111111111",
            "123",
            DateTime.UtcNow.AddYears(5),
            limit);

        // Assert
        card.Should().NotBeNull();
        card.AvailableLimit.Should().Be(limit);
        card.TotalLimit.Should().Be(limit);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComDataExpiracaoInformada()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddYears(-1);

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            "1111111111111111",
            "123",
            expirationDate,
            5000m);

        // Assert
        card.Should().NotBeNull();
        card.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComNumeroCartaoInvalido()
    {
        // Arrange
        var invalidCardNumber = "123"; // Número inválido

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            invalidCardNumber,
            "123",
            DateTime.UtcNow.AddYears(5),
            5000m);

        // Assert
        card.Should().NotBeNull();
        // A entidade não valida formato do cartão, isso deve ser responsabilidade do CardGeneratorService
        card.Number.Should().Be(invalidCardNumber);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComCvvInvalido()
    {
        // Arrange
        var invalidCvv = "1"; // CVV inválido

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            "1111111111111111",
            invalidCvv,
            DateTime.UtcNow.AddYears(5),
            5000m);

        // Assert
        // A entidade não valida formato do CVV, isso deve ser responsabilidade do CardGeneratorService
        card.Should().NotBeNull();
        card.Cvv.Should().Be(invalidCvv);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComNomeVazio()
    {
        // Arrange
        var emptyName = string.Empty;

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            emptyName,
            "1111111111111111",
            "123",
            DateTime.UtcNow.AddYears(5),
            5000m);

        // Assert
        card.Should().NotBeNull();
        card.ClientName.Should().Be(emptyName);
    }

    [Fact]
    public void Create_DeveCriarCartao_ComLimiteNegativo()
    {
        // Arrange
        var negativeLimit = -1000m;

        // Act
        var card = Card.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alexandre Dórea",
            "1111111111111111",
            "123",
            DateTime.UtcNow.AddYears(5),
            negativeLimit);

        // Assert
        // A entidade não valida limite negativo
        card.Should().NotBeNull();
        card.TotalLimit.Should().Be(negativeLimit);
        card.AvailableLimit.Should().Be(negativeLimit);
    }
}