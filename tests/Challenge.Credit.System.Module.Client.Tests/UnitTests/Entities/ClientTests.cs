using FluentAssertions;

namespace Challenge.Credit.System.Module.Client.Tests.UnitTests.Entities;

public sealed class ClientTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateClient()
    {
        // Arrange
        var name = "Alexandre Dórea";
        var documentNumber = "12345678909";
        var email = "alexandre@teste.com";
        var telephone = "71999999999";
        var dateBirth = new DateTime(1990, 1, 1);
        var monthlyIncome = 5000m;

        // Act
        var client = Client.Core.Domain.Entities.Client.Create(
            name,
            documentNumber,
            email,
            telephone,
            dateBirth,
            monthlyIncome);

        // Assert
        client.Should().NotBeNull();
        client.Id.Should().NotBeEmpty();
        client.Name.Should().Be(name);
        client.Document.Number.Should().Be(documentNumber);
        client.Email.Should().Be(email);
        client.Telephone.Should().Be(telephone);
        client.DateBirth.Should().Be(dateBirth);
        client.MonthlyIncome.Should().Be(monthlyIncome);
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange
        var name = "Alexandre Dórea";
        var documentNumber = "12345678909";
        var email = "alexandre@teste.com";
        var telephone = "71999999999";
        var dateBirth = new DateTime(1990, 1, 1);
        var monthlyIncome = 5000m;

        // Act
        var client1 = Client.Core.Domain.Entities.Client.Create(name, documentNumber, email, telephone, dateBirth, monthlyIncome);
        var client2 = Client.Core.Domain.Entities.Client.Create(name, documentNumber, email, telephone, dateBirth, monthlyIncome);

        // Assert
        client1.Id.Should().NotBe(client2.Id);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        // Arrange
        var nameComEspacos = "  Alexandre Dórea  ";
        var documentNumber = "12345678909";
        var email = "alexandre@teste.com";
        var telephone = "71999999999";
        var dateBirth = new DateTime(1990, 1, 1);
        var monthlyIncome = 5000m;

        // Act
        var client = Client.Core.Domain.Entities.Client.Create(
            nameComEspacos,
            documentNumber,
            email,
            telephone,
            dateBirth,
            monthlyIncome);

        // Assert
        client.Name.Should().Be("Alexandre Dórea");
        client.Name.Should().NotStartWith(" ");
        client.Name.Should().NotEndWith(" ");
    }

    [Fact]
    public void Create_WithInvalidDocument_ShouldLaunchException()
    {
        // Arrange
        var name = "Alexandre Dórea";
        var documentNumberInvalido = "00000000000";
        var email = "alexandre@teste.com";
        var telephone = "71999999999";
        var dateBirth = new DateTime(1990, 1, 1);
        var monthlyIncome = 5000m;

        // Act
        Action act = () => Client.Core.Domain.Entities.Client.Create(
            name,
            documentNumberInvalido,
            email,
            telephone,
            dateBirth,
            monthlyIncome);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("CPF inválido");
    }
}