namespace Challenge.Credit.System.Module.CreditCard.Core.Domain.Entities;

public sealed class Card
{
    public Guid Id { get; private set; }
    public Guid ProposalId { get; private set; }
    public Guid ClientId { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty; // TODO: Value Object com as validacoes
    public string Cvv { get; private set; } = string.Empty;    // TODO: Value Object com as validacoes
    public DateTime ExpirationDate { get; private set; }
    public decimal AvailableLimit { get; private set; }        // TODO: Value Object com as validacoes
    public decimal TotalLimit { get; private set; }            // TODO: Value Object com as validacoes
    public CardStatus Status { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? BlockDate { get; private set; }

    private Card()
    { }

    private Card(
        Guid proposalId,
        Guid clientId,
        string clientName,
        string number,
        string cvv,
        DateTime expirationDate,
        decimal totalLimit)
    {
        Id = Guid.NewGuid();
        ProposalId = proposalId;
        ClientId = clientId;
        ClientName = clientName;
        Number = number;
        Cvv = cvv;
        ExpirationDate = expirationDate;
        TotalLimit = totalLimit;
        AvailableLimit = totalLimit; // Limite disponível começa como o total
        Status = CardStatus.Issued;
        IssueDate = DateTime.UtcNow;
    }

    public static Card Create(
        Guid proposalId,
        Guid clientId,
        string clientName,
        string number,
        string cvv,
        DateTime expirationDate,
        decimal totalLimit)
    {
        //if (expirationDate < DateTime.UtcNow)
        //    throw new ArgumentException("A data de expiração não pode ser no passado.", nameof(expirationDate));

        return new Card(proposalId, clientId, clientName, number, cvv, expirationDate, totalLimit);
    }

    public void Activate()
    {
        switch (Status)
        {
            case CardStatus.Activated:
                return;

            case CardStatus.Issued:
            case CardStatus.Blocked:
                Status = CardStatus.Activated;
                ActivationDate = DateTime.UtcNow;
                break;

            case CardStatus.Canceled:
                throw new InvalidOperationException("Não é possível ativar um cartão cancelado.");

            default:
                throw new InvalidOperationException($"Não é possível ativar um cartão com status '{Status}'.");
        }
    }

    public void Use(decimal amount)
    {
        if (Status != CardStatus.Activated)
            throw new InvalidOperationException("O cartão precisa estar ativo para ser usado.");

        if (AvailableLimit < amount)
            throw new InvalidOperationException($"Limite insuficiente.");

        // Debita o valor
        AvailableLimit -= amount;
    }
}

public enum CardStatus
{
    Issued = 0,
    Activated = 1,
    Blocked = 2,
    Canceled = 3
}