namespace Challenge.Credit.System.Module.Client.Core.Domain.ValueObjects;

public sealed class Document
{
    public string Number { get; }
    public DocumentType Type { get; }

    private Document(string value, DocumentType type)
    {
        Number = value;
        Type = type;
    }

    public static Document Create(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length == 11)
        {
            if (!IsValidCPF(digits))
                throw new ArgumentException("CPF inválido");

            return new Document(digits, DocumentType.CPF);
        }

        if (digits.Length == 14)
        {
            if (!IsValidCNPJ(digits))
                throw new ArgumentException("CNPJ inválido");

            return new Document(digits, DocumentType.CNPJ);
        }

        throw new ArgumentException("Documento inválido");
    }

    private static bool IsValidCPF(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;

        var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf.Substring(0, 9);
        var soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        var digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    private static bool IsValidCNPJ(string cnpj)
    {
        return cnpj.Distinct().Count() > 1;
    }
}

public enum DocumentType
{
    CPF,
    CNPJ
}