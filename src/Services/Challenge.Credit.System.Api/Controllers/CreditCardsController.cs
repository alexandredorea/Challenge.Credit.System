using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Challenge.Credit.System.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class CreditCardsController : ControllerBase
{
}