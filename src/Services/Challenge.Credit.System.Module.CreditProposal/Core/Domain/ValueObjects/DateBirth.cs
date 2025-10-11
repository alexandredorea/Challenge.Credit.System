namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;

public sealed class DateBirth
{
    public DateTime Value { get; }

    public DateBirth(DateTime value)
    {
        if (value > DateTime.UtcNow)
            throw new ArgumentException("Data de nascimento inválida");

        Value = value;
    }

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - Value.Year;

        if (Value.Date > today.AddYears(-age))
            age--;

        return age;
    }
}