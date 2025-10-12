using Challenge.Credit.System.Module.Client.Core.Domain.ValueObjects;

namespace Challenge.Credit.System.Module.Client.Core.Domain.Entities;

public sealed class Client
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Document Document { get; private set; } = default!;
    public string Email { get; private set; } = string.Empty; //TODO: adicionar value object para validar email, caso as validacoes anteriores passem
    public string Telephone { get; private set; } = string.Empty;
    public DateTime DateBirth { get; private set; }
    public decimal MonthlyIncome { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Client()
    { }

    private Client(
        string name,
        string documentNumber,
        string email,
        string telephone,
        DateTime dateBirth,
        decimal monthlyIncome)
    {
        Id = Guid.NewGuid();
        Name = name;
        Document = Document.Create(documentNumber);
        Email = email;
        Telephone = telephone;
        DateBirth = dateBirth;
        MonthlyIncome = monthlyIncome;
        CreatedAt = DateTime.UtcNow;
    }

    public static Client Create(
        string name,
        string documentNumber,
        string email,
        string telephone,
        DateTime dateBirth,
        decimal monthlyIncome)
    {
        var client = new Client(
            name.Trim(),
            documentNumber,
            email,
            telephone,
            dateBirth,
            monthlyIncome
        );

        return client;
    }
}