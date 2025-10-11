namespace Challenge.Credit.System.Module.CreditProposal.Core.Domain.Entities;

public sealed class Proposal
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public decimal MonthlyIncome { get; private set; }
    public int Score { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime EvaluationDate { get; private set; }
    public StatusProposal Status { get; private set; }
    public decimal AvaliableLimit { get; private set; }
    public int CardsAllowed { get; private set; }
    public string? RejectionReason { get; private set; }

    private Proposal()
    { }

    private Proposal(Guid clientId, string clientName, decimal monthlyIncome, int score)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        ClientName = clientName;
        MonthlyIncome = monthlyIncome;
        Score = score;
        CreatedAt = DateTime.UtcNow;
        Status = StatusProposal.Pending;
    }

    public static Proposal Create(
        Guid clientId,
        string clientName,
        decimal monthlyIncome,
        int score)
    {
        return new Proposal(clientId, clientName, monthlyIncome, score);
    }

    // Método interno usado pelas policies (servicos de domínio)
    internal void Approve(decimal limit, int cards)
    {
        EvaluationDate = DateTime.UtcNow;
        Status = StatusProposal.Approved;
        AvaliableLimit = limit;
        CardsAllowed = cards;
        RejectionReason = null;
    }

    // Método interno usado pelas policies (servicos de domínio)
    internal void Reject(string reason)
    {
        EvaluationDate = DateTime.UtcNow;
        Status = StatusProposal.Rejected;
        AvaliableLimit = 0;
        CardsAllowed = 0;
        RejectionReason = reason;
    }

    //private void CalculateScore(DateBirth dateBirth)
    //{
    //int score = 0;

    //if (MonthlyIncome >= 10000)
    //    score += 400;
    //else if (MonthlyIncome >= 5000)
    //    score += 300;
    //else if (MonthlyIncome >= 3000)
    //    score += 200;
    //else if (MonthlyIncome >= 1500)
    //    score += 100;
    //else
    //    score += 50;

    //var age = dateBirth.GetAge();
    //if (age >= 30 && age <= 60)
    //    score += 300;
    //else if (age >= 25 && age < 30)
    //    score += 200;
    //else if (age >= 21 && age < 25)
    //    score += 150;
    //else if (age >= 18 && age < 21)
    //    score += 100;
    //else
    //    score += 50;

    //var random = new Random();
    //score += random.Next(0, 301);

    //Score = Math.Clamp(score, 0, 1000);
    //}

    //private void Evaluate()
    //{
    //    EvaluationDate = DateTime.UtcNow;

    //    //if (Score <= 100)
    //    //{
    //    //    Status = StatusProposal.Rejected;
    //    //    AvaliableLimit = 0;
    //    //    CardsAllowed = 0;
    //    //    RejectionReason = "Score insuficiente para aprovação de crédito";
    //    //}
    //    //else if (Score <= 500)
    //    //{
    //    //    Status = StatusProposal.Approved;
    //    //    AvaliableLimit = 1000.00m;
    //    //    CardsAllowed = 1;
    //    //}
    //    //else
    //    //{
    //    //    Status = StatusProposal.Approved;
    //    //    AvaliableLimit = 5000.00m;
    //    //    CardsAllowed = 2;
    //    //}
    //}
}

public enum StatusProposal
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}