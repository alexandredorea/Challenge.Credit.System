namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.ValueObjects;

public sealed class DateBirth
{
    public DateTime Value { get; }

    public DateBirth(DateTime value)
    {
        var dateOnly = value.Date;

        if (dateOnly > DateTime.Today)
            throw new ArgumentException("Data de nascimento inválida");

        Value = dateOnly;
    }

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - Value.Year;

        if (Value.Date > today.AddYears(-age))
            age--;

        return age;
    }

    public override string ToString() => Value.ToString("dd/MM/yyyy");
}