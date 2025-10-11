using System.Net.Mime;
using Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;
using Challenge.Credit.System.Module.Client.Core.Application.Services;
using Challenge.Credit.System.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Challenge.Credit.System.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class ClientsController(IClientService clienteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ClientResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var client = await clienteService.GetAllAsync(cancellationToken);
        return Ok(ApiResult<IEnumerable<ClientResponse?>>.SuccessResult(client));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ClientResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<ClientResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await clienteService.GetByIdAsync(id, cancellationToken);

        if (client is null)
            return NotFound(ApiResult<ClientResponse>.FailureResult("Cliente não encontrado.", "NOT_FOUND"));

        return Ok(ApiResult<ClientResponse>.SuccessResult(client));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ClientResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<ClientResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResult<ClientResponse>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => new ErrorDetail { Code = "VALIDATION_ERROR", Message = e.ErrorMessage })
                .ToList();
            return UnprocessableEntity(ApiResult<ClientResponse>.FailureResult("Dados inválidos", errors));
        }

        var result = await clienteService.CreateAsync(request, cancellationToken);

        if (result is null)
            return Conflict(ApiResult<ClientResponse>.FailureResult("CPF ou Email já cadastrado.", "DUPLICATE_ENTRY"));

        return CreatedAtAction(
            actionName: nameof(GetByIdAsync),
            routeValues: new { id = result.Id },
            value: ApiResult<ClientResponse>.SuccessResult(result, "Cliente cadastrado com sucesso."));
    }
}