using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;

namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;


public sealed class ScoreCalculator : IScoreCalculator
{
    public int Calculate(decimal monthlyIncome, DateBirth birthDate)
    {
        int score = IncomeScore(monthlyIncome)
                  + AgeScore(birthDate.GetAge())
                  + RandomScore();

        // Garantir que o score está entre 0 e 1000
        return Math.Clamp(score, 0, 1000);
    }
    /**
     * Importante: 
     * O desafio não dizia como calcular o score de um cliente, apenas informava que ele teria um score de: 
     * - De 0 a 100 – Não é permitido liberação de cartão de crédito; 
     * - De 101 a 500 – Permitido liberação de cartão de crédito (com limite de R$ 1.000,00); 
     * - De 501 a 1000 – Permitido liberação de até 2 cartão de crédito (com limite de R$ 5.000, 00) cada. 
     * 
     * Então, criei uma lógica simplificada de cálculo de score baseado na renda, idade e uma tentativa de 
     * simular variabilidades (histórico de crédito, etc.). 
     * 
     * Em um cenário financeiro, isso é muito mais complexo e envolve consultas a bureaus de crédito, etc.
     */

    // Score baseado na renda mensal
    private static int IncomeScore(decimal income) =>
        income switch
        {
            >= 10000 => 400,
            >= 5000 => 300,
            >= 3000 => 200,
            >= 1500 => 100,
            _ => 50
        };

    // Score baseado na idade
    private static int AgeScore(int age) =>
        age switch
        {
            >= 30 and <= 60 => 300,
            >= 25 and < 30 => 200,
            >= 21 and < 25 => 150,
            >= 18 and < 21 => 100,
            _ => 50
        };

    // Score baseado na aleatoriedade (histórico de crédito, etc.)
    private static int RandomScore() => 
        new Random().Next(0, 301);
}
