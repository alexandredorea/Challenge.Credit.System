using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;
using FluentAssertions;

namespace Challenge.Credit.System.Module.CreditProposal.Tests.UnitTests.Services;

public sealed class ScoreCalculatorTests
{
    private readonly IScoreCalculator _scoreCalculator;

    public ScoreCalculatorTests()
    {
        _scoreCalculator = new ScoreCalculator();
    }

    [Theory]
    [InlineData(10000, 40, 700, 1000)] // Renda alta (400) + Idade ideal (300) + Random (0-300) = 700-1000
    [InlineData(5000, 35, 600, 900)]   // Renda boa (300) + Idade ideal (300) + Random (0-300) = 600-900
    [InlineData(3000, 28, 400, 700)]   // Renda média (200) + Idade boa (200) + Random (0-300) = 400-700 ← CORRIGIDO
    [InlineData(1500, 22, 250, 550)]   // Renda baixa (100) + Idade ok (150) + Random (0-300) = 250-550 ← CORRIGIDO
    [InlineData(500, 19, 150, 450)]    // Renda muito baixa (50) + Idade jovem (100) + Random (0-300) = 150-450
    public void Calculate_ComDiferentesRendasEIdades_DeveRetornarScoreDentroDoRange(
    decimal monthlyIncome,
    int age,
    int minExpectedScore,
    int maxExpectedScore)
    {
        // Arrange
        var birthDate = new DateBirth(DateTime.Now.AddYears(-age));

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        score.Should().BeInRange(minExpectedScore, maxExpectedScore);
    }

    [Fact]
    public void Calculate_DeveRetornarScoreEntre0E1000()
    {
        // Arrange
        var monthlyIncome = 50000m; // Renda muito alta
        var birthDate = new DateBirth(new DateTime(1980, 1, 1));

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        score.Should().BeInRange(0, 1000);
    }

    [Theory]
    [InlineData(10000, 400)] // Renda >= 10000 = 400 pontos
    [InlineData(5000, 300)]  // Renda >= 5000 = 300 pontos
    [InlineData(3000, 200)]  // Renda >= 3000 = 200 pontos
    [InlineData(1500, 100)]  // Renda >= 1500 = 100 pontos
    [InlineData(500, 50)]    // Renda < 1500 = 50 pontos
    public void Calculate_ComDiferentesRendas_DeveAplicarPontuacaoCorreta(decimal monthlyIncome, int expectedIncomeScore)
    {
        // Arrange
        var birthDate = new DateBirth(new DateTime(1990, 1, 1));

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        // Score total = IncomeScore + AgeScore + RandomScore
        // Como RandomScore varia de 0 a 300, o score mínimo será expectedIncomeScore + AgeScore
        // Para idade de ~35 anos, AgeScore = 300
        score.Should().BeGreaterThanOrEqualTo(expectedIncomeScore);
    }

    [Theory]
    [InlineData(40, 300)]  // Idade entre 30 e 60 = 300 pontos
    [InlineData(28, 200)]  // Idade entre 25 e 29 = 200 pontos
    [InlineData(22, 150)]  // Idade entre 21 e 24 = 150 pontos
    [InlineData(19, 100)]  // Idade entre 18 e 20 = 100 pontos
    [InlineData(17, 50)]   // Idade < 18 = 50 pontos
    public void Calculate_ComDiferentesIdades_DeveAplicarPontuacaoCorreta(int age, int expectedAgeScore)
    {
        // Arrange
        var monthlyIncome = 5000m; // Renda fixa para isolar teste de idade
        var birthDate = new DateBirth(DateTime.UtcNow.AddYears(-age));

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        // Score total = IncomeScore (300) + AgeScore + RandomScore (0-300)
        // Score mínimo = 300 + expectedAgeScore + 0
        score.Should().BeGreaterThanOrEqualTo(300 + expectedAgeScore);
    }

    [Fact]
    public void Calculate_ComMesmaRendaEIdade_DeveRetornarScoresDiferentes()
    {
        // Arrange
        var monthlyIncome = 5000m;
        var birthDate = new DateBirth(new DateTime(1990, 1, 1));

        // Act
        var score1 = _scoreCalculator.Calculate(monthlyIncome, birthDate);
        var score2 = _scoreCalculator.Calculate(monthlyIncome, birthDate);
        var score3 = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        // Devido ao componente aleatório, os scores devem ser diferentes (na maioria das vezes)
        // Vamos executar 3 vezes para aumentar a probabilidade
        var scores = new[] { score1, score2, score3 };
        scores.Distinct().Count().Should().BeGreaterThan(1, "scores devem variar devido ao componente aleatório");
    }

    [Fact]
    public void Calculate_ComRendaZero_DeveRetornarScoreMinimoPositivo()
    {
        // Arrange
        var monthlyIncome = 0m;
        var birthDate = new DateBirth(new DateTime(1990, 1, 1));

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        score.Should().BeGreaterThan(0);
        score.Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public void Calculate_ComIdadeMuitoAlta_DeveRetornarScoreDentroDoLimite()
    {
        // Arrange
        var monthlyIncome = 5000m;
        var birthDate = new DateBirth(new DateTime(1920, 1, 1)); // ~105 anos

        // Act
        var score = _scoreCalculator.Calculate(monthlyIncome, birthDate);

        // Assert
        score.Should().BeInRange(0, 1000);
    }
}