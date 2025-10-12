using Challenge.Credit.System.Shared.Models;
using FluentAssertions;

namespace Challenge.Credit.System.Shared.Tests.Models;

/// <summary>
/// Testes unitários para o modelo ApiResult
/// </summary>
public class ApiResultTests
{
    [Fact]
    public void SuccessResult_WithComDados_DeveCriarResultadoComSucesso()
    {
        // Arrange
        var data = "Dados de teste";
        var message = "Operação realizada com sucesso";

        // Act
        var result = ApiResult<string>.SuccessResult(data, message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void SuccessResult_WithoutMessageCustom_ShouldUseMessageDefault()
    {
        // Arrange
        var data = "Dados de teste";

        // Act
        var result = ApiResult<string>.SuccessResult(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Operação realizada com sucesso");
        result.Data.Should().Be(data);
    }

    [Fact]
    public void FailureResult_ComMensagem_DeveCriarResultadoComFalha()
    {
        // Arrange
        var message = "Erro ao processar";

        // Act
        var result = ApiResult<string>.FailureResult(message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void FailureResult_ComMensagemEErros_DeveCriarResultadoComFalhaEErros()
    {
        // Arrange
        var message = "Erro ao processar";
        var errors = new List<ErrorDetail>
        {
            new() { Code = "ERR001", Message = "Erro 1" },
            new() { Code = "ERR002", Message = "Erro 2" }
        };

        // Act
        var result = ApiResult<string>.FailureResult(message, errors);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().HaveCount(2);
        result.Error![0].Code.Should().Be("ERR001");
        result.Error[1].Code.Should().Be("ERR002");
    }

    [Fact]
    public void FailureResult_ComMensagemECodigoErro_DeveCriarResultadoComFalhaEErro()
    {
        // Arrange
        var message = "CPF já cadastrado";
        var errorCode = "DUPLICATE_CPF";

        // Act
        var result = ApiResult<string>.FailureResult(message, errorCode);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().HaveCount(1);
        result.Error![0].Code.Should().Be(errorCode);
        result.Error[0].Message.Should().Be(message);
    }

    [Fact]
    public void SuccessResult_ComDadosNulos_DeveCriarResultadoComSucesso()
    {
        // Act
        var result = ApiResult<string?>.SuccessResult(null);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void SuccessResult_ComObjetoComplexo_DeveCriarResultadoComSucesso()
    {
        // Arrange
        var data = new { Id = 1, Name = "Teste" };

        // Act
        var result = ApiResult<object>.SuccessResult(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void FailureResult_ComListaErrosVazia_DeveCriarResultadoComFalha()
    {
        // Arrange
        var message = "Erro ao processar";
        var errors = new List<ErrorDetail>();

        // Act
        var result = ApiResult<string>.FailureResult(message, errors);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void ErrorDetail_DevePermitirCriacaoComPropriedades()
    {
        // Act
        var error = new ErrorDetail
        {
            Code = "ERR001",
            Message = "Mensagem de erro"
        };

        // Assert
        error.Code.Should().Be("ERR001");
        error.Message.Should().Be("Mensagem de erro");
    }

    [Fact]
    public void ErrorDetail_DeveTerValoresPadraoVazios()
    {
        // Act
        var error = new ErrorDetail();

        // Assert
        error.Code.Should().Be(string.Empty);
        error.Message.Should().Be(string.Empty);
    }
}