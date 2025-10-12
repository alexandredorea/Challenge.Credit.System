using Challenge.Credit.System.Module.Client.Core.Domain.ValueObjects;

namespace Challenge.Credit.System.Module.Client.Tests.Configurations.Builders;

internal sealed class ClientBuilder
{
    private string _name = "Alexandre Dórea";
    private Document _document = Document.Create("25077583501"); //TODO: Builder aqui também
    private string _email = "alexandre@teste.com";
    private string _telephone = "71999999999";
    private DateTime _dateBirth = new DateTime(1982, 5, 6);
    private decimal _monthlyIncome = 5000m;

    public ClientBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ClientBuilder WithDocument(string document)
    {
        _document = Document.Create(document);
        return this;
    }

    public ClientBuilder WithDocument(Document document)
    {
        _document = document;
        return this;
    }

    public ClientBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ClientBuilder WithTelephone(string telephone)
    {
        _telephone = telephone;
        return this;
    }

    public ClientBuilder WithDateBirth(DateTime dateBirth)
    {
        _dateBirth = dateBirth;
        return this;
    }

    public ClientBuilder WithMonthlyIncome(decimal income)
    {
        _monthlyIncome = income;
        return this;
    }

    public Core.Domain.Entities.Client Build()
    {
        return Core.Domain.Entities.Client.Create(
            _name,
            _document.Number,
            _email,
            _telephone,
            _dateBirth,
            _monthlyIncome);
    }
}