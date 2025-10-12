using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditProposal.Tests.UnitTests.ValueObjects;

public sealed class DateBirthTests
{
    [Fact]
    public void Constructor_ComDataValida_DeveCriarDateBirth()
    {
        // Arrange
        var date = DateTime.Now.AddYears(-43).Date;

        // Act
        var dateBirth = new DateBirth(date);

        // Assert
        dateBirth.Should().NotBeNull();
        dateBirth.Value.Should().Be(date);
    }

    [Fact]
    public void GetAge_ComDataNascimento30AnosAtras_DeveRetornar30()
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(-40);
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(40);
    }

    [Fact]
    public void GetAge_ComDataNascimentoHoje_DeveRetornar0()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(0);
    }

    [Fact]
    public void GetAge_ComDataNascimento25AnosEMeioAtras_DeveRetornar25()
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(-25).AddMonths(-6);
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(25);
    }

    [Fact]
    public void GetAge_ComDataNascimentoFutura_DeveLancarExcessao()
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(5);

        // Act
        Action act = () => new DateBirth(date);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Data de nascimento inválida");
    }

    [Theory]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(40)]
    [InlineData(65)]
    [InlineData(100)]
    public void GetAge_ComDiferentesIdades_DeveCalcularCorretamente(int expectedAge)
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(-expectedAge);
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(expectedAge);
    }

    [Fact]
    public void GetAge_ComAniversarioHoje_DeveContarAnoCompleto()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var date = new DateTime(today.Year - 30, today.Month, today.Day);
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(30);
    }

    [Fact]
    public void GetAge_ComAniversarioAmanha_NaoDeveContarAnoCompleto()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var date = new DateTime(tomorrow.Year - 30, tomorrow.Month, tomorrow.Day);
        var dateBirth = new DateBirth(date);

        // Act
        var age = dateBirth.GetAge();

        // Assert
        age.Should().Be(29);
    }
}