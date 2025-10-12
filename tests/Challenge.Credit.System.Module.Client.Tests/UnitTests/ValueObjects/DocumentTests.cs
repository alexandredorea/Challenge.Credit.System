using Challenge.Credit.System.Module.Client.Core.Domain.ValueObjects;
using FluentAssertions;

namespace Challenge.Credit.System.Module.Client.Tests.UnitTests.ValueObjects;

public sealed class DocumentTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("11144477735")]
    [InlineData("52998224725")]
    public void Create_WithCpfValide_ShouldCreateDocument(string cpf)
    {
        // Act
        var document = Document.Create(cpf);

        // Assert
        document.Should().NotBeNull();
        document.Number.Should().Be(cpf);
        document.Type.Should().Be(DocumentType.CPF);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    [InlineData("99999999999")]
    public void Create_WithCpfInvalide_ShouldLaunchException(string cpfInvalido)
    {
        // Act
        Action act = () => Document.Create(cpfInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CPF inválido");
    }

    [Theory]
    [InlineData("12345678000195")]
    [InlineData("11222333000181")]
    public void Create_WithCnpjValide_ShouldCreateDocument(string cnpj)
    {
        // Act
        var document = Document.Create(cnpj);

        // Assert
        document.Should().NotBeNull();
        document.Number.Should().Be(cnpj);
        document.Type.Should().Be(DocumentType.CNPJ);
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    public void Create_WithCnpjValide_ShouldLaunchException(string cnpjInvalido)
    {
        // Act
        Action act = () => Document.Create(cnpjInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CNPJ inválido");
    }

    [Fact]
    public void Create_WithCpfFormatted_ShouldRemoveFormat()
    {
        // Arrange
        var cpfFormatado = "123.456.789-09";

        // Act
        var document = Document.Create(cpfFormatado);

        // Assert
        document.Number.Should().Be("12345678909");
        document.Number.Should().NotContain(".");
        document.Number.Should().NotContain("-");
    }

    [Fact]
    public void Create_WithCnojFormatted_ShouldRemoveFormat()
    {
        // Arrange
        var cnpjFormatado = "12.345.678/0001-95";

        // Act
        var document = Document.Create(cnpjFormatado);

        // Assert
        document.Number.Should().Be("12345678000195");
        document.Number.Should().NotContain(".");
        document.Number.Should().NotContain("/");
        document.Number.Should().NotContain("-");
    }
}