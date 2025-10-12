using Challenge.Credit.System.Module.CreditCard.Core.Application.Services;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditCard.Tests.UnitTests.Services;

public sealed class CardGeneratorServiceTests
{
    private readonly ICardGeneratorService _cardGenerator;

    public CardGeneratorServiceTests()
    {
        _cardGenerator = new CardGeneratorService();
    }

    [Fact]
    public void GenerateCardNumber_DeveRetornarNumeroComExatamente16Digitos()
    {
        // Act
        var cardNumber = _cardGenerator.GenerateCardNumber();

        // Assert
        cardNumber.Should().HaveLength(16);
        cardNumber.Should().MatchRegex(@"^\d{16}$", "deve conter apenas dígitos");
    }

    [Fact]
    public void GenerateCardNumber_DeveGerarNumerosUnicos()
    {
        // Act
        var cardNumbers = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            cardNumbers.Add(_cardGenerator.GenerateCardNumber());
        }

        // Assert
        cardNumbers.Should().HaveCount(100, "todos os números devem ser únicos");
    }

    [Fact]
    public void GenerateCardNumber_DeveSomenteConterDigitos()
    {
        // Act
        var cardNumber = _cardGenerator.GenerateCardNumber();

        // Assert
        cardNumber.All(char.IsDigit).Should().BeTrue();
    }

    [Fact]
    public void GenerateCardNumber_DeveGerarNumerosValidos()
    {
        // Act
        var cardNumber = _cardGenerator.GenerateCardNumber();

        // Assert
        cardNumber.Should().NotBeNullOrEmpty();
        cardNumber.Length.Should().Be(16);
        long.TryParse(cardNumber, out _).Should().BeTrue("deve ser um número válido");
    }

    [Fact]
    public void GenerateCardNumber_ExecutadoMultiplasVezes_DeveRetornarNumerosDiferentes()
    {
        // Act
        var cardNumber1 = _cardGenerator.GenerateCardNumber();
        var cardNumber2 = _cardGenerator.GenerateCardNumber();
        var cardNumber3 = _cardGenerator.GenerateCardNumber();

        // Assert
        cardNumber1.Should().NotBe(cardNumber2);
        cardNumber2.Should().NotBe(cardNumber3);
        cardNumber1.Should().NotBe(cardNumber3);
    }

    [Fact]
    public void GenerateCvv_DeveRetornarNumeroComExatamente3Digitos()
    {
        // Act
        var cvv = _cardGenerator.GenerateCvv();

        // Assert
        cvv.Should().HaveLength(3);
        cvv.Should().MatchRegex(@"^\d{3}$", "deve conter apenas 3 dígitos");
    }

    [Fact]
    public void GenerateCvv_DeveSomenteConterDigitos()
    {
        // Act
        var cvv = _cardGenerator.GenerateCvv();

        // Assert
        cvv.All(char.IsDigit).Should().BeTrue();
    }

    [Fact]
    public void GenerateCvv_DeveGerarNumeroEntre000E999()
    {
        // Act
        var cvv = _cardGenerator.GenerateCvv();
        var cvvNumber = int.Parse(cvv);

        // Assert
        cvvNumber.Should().BeInRange(0, 999);
    }

    [Fact]
    public void GenerateCvv_DeveGerarNumerosVariados()
    {
        // Act
        var cvvs = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            cvvs.Add(_cardGenerator.GenerateCvv());
        }

        // Assert
        cvvs.Should().HaveCountGreaterThan(1, "deve gerar CVVs variados");
    }

    [Fact]
    public void GenerateExpirationDate_DeveRetornarDataFutura()
    {
        // Act
        var expirationDate = _cardGenerator.GenerateExpirationDate();

        // Assert
        expirationDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateExpirationDate_DeveRetornar5AnosNoFuturo()
    {
        // Act
        var expirationDate = _cardGenerator.GenerateExpirationDate();
        var expectedDate = DateTime.UtcNow.AddYears(5);

        // Assert
        expirationDate.Year.Should().Be(expectedDate.Year);
        expirationDate.Month.Should().Be(expectedDate.Month);
    }


    [Fact]
    public void GenerateExpirationDate_ExecutadoMultiplasVezes_DeveRetornarMesmaData()
    {
        // Act
        var date1 = _cardGenerator.GenerateExpirationDate();
        var date2 = _cardGenerator.GenerateExpirationDate();

        // Assert
        date1.Date.Should().Be(date2.Date);
    }
}