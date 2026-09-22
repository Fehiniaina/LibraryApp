using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Authors.Queries.GetAllAuthors;
using Library.Application.Authors.Queries.SearchAuthors;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Playground.Mvc.Filters;

namespace Playground.Mvc.Controllers;

[Route("api/authors")]
[TypeFilter(typeof(WrapJsonResultFilter))]
[TypeFilter(typeof(BlockNegativePriceFilter))]
[TypeFilter(typeof(TrimStringsActionFilter))]
[Authorize]
[ApiController]
public class AuthorsApiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsApiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var authors = await _mediator.Send(new GetAllAuthorsQuery());

        return Ok(authors);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword)
    {
        Console.WriteLine($">>> Action reçoit: '{keyword}'");

        var result = await _mediator.Send(new SearchAuthorsQuery(keyword, 1, 20));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAuthorCommand command)
    {
        Console.WriteLine(">>> L'ACTION s'exécute !");
        var id = await _mediator.Send(command);

        return Ok(id);
    }

    [HttpGet("crash")]
    [TypeFilter(typeof(ActionExecutedInspectorFilter))]
    public Task<IActionResult> Crash()
    {
        throw new InvalidOperationException("Erreur simulée pour tester OnActionExecuted");
    }
}
