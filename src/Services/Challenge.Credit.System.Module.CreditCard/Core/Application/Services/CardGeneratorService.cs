using System.Text;

namespace Challenge.Credit.System.Module.CreditCard.Core.Application.Services;

public interface ICardGeneratorService
{
    string GenerateCardNumber();

    string GenerateCvv();

    DateTime GenerateExpirationDate();
}

internal sealed class CardGeneratorService : ICardGeneratorService
{
    private const byte lenghtCard = 16;

    // Gerar numero de cartao de 16 dígitos, apenas para fins didatico do desafio
    // Geralmente se usa o algoritmo de Luhn para BIN específico do banco
    public string GenerateCardNumber()
    {
        var cardNumber = new StringBuilder(lenghtCard);
        for (int index = 0; index < lenghtCard; index++)
        {
            cardNumber.Append(Random.Shared.Next(0, 10));
        }
        return cardNumber.ToString();
    }

    public string GenerateCvv()
        => Random.Shared.Next(100, 1000).ToString();

    public DateTime GenerateExpirationDate()
        => DateTime.UtcNow.AddYears(5);
}