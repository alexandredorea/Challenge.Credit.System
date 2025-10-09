using System.ComponentModel.DataAnnotations;

namespace Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;

public sealed class CreateClientRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 200 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
    public string DocumentNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório")]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone deve conter 10 ou 11 dígitos")]
    public string Telephone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DateBirth { get; init; }

    [Required(ErrorMessage = "Renda mensal é obrigatória")]
    [Range(0, double.MaxValue, ErrorMessage = "Renda mensal deve ser maior ou igual a zero")]
    public decimal MonthlyIncome { get; init; }
}