using Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;

namespace Challenge.Credit.System.Module.Client.Tests.Configurations.Builders;

internal sealed class CreateClientRequestBuilder
{
    private string _name = "Alexandre Dórea";
    private string _documentNumber = "25077583501";
    private string _email = "alexandre@test.com";
    private string _telephone = "71999999999";
    private DateTime _dateBirth = new DateTime(1982, 1, 1);
    private decimal _monthlyIncome = 5000m;

    public CreateClientRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateClientRequestBuilder WithDocument(string documentNumber)
    {
        _documentNumber = documentNumber;
        return this;
    }

    public CreateClientRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CreateClientRequestBuilder WithTelephone(string telephone)
    {
        _telephone = telephone;
        return this;
    }

    public CreateClientRequestBuilder WithDateBirth(DateTime dateBirth)
    {
        _dateBirth = dateBirth;
        return this;
    }

    public CreateClientRequestBuilder WithDateBirth(int year, int month, int day)
    {
        _dateBirth = new DateTime(year, month, day);
        return this;
    }

    public CreateClientRequestBuilder WithMonthlyIncome(decimal monthlyIncome)
    {
        _monthlyIncome = monthlyIncome;
        return this;
    }

    public CreateClientRequestBuilder WithAge(int age)
    {
        _dateBirth = DateTime.Today.AddYears(-age);
        return this;
    }

    public CreateClientRequest Build()
    {
        return new CreateClientRequest
        {
            Name = _name,
            DateBirth = _dateBirth,
            DocumentNumber = _documentNumber,
            Email = _email,
            Telephone = _telephone,
            MonthlyIncome = _monthlyIncome
        };
    }
}