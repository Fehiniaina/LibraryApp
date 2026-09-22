using Library.Application.Authors.Queries.GetAllAuthors;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Playground.Mvc.Filters;
using Playground.Mvc.Services;

namespace Playground.Mvc.Controllers;

[TypeFilter(typeof(TimingActionFilter))]
[TypeFilter(typeof(TrackerLoggingFilter))]
public class AuthorsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IRequestTracker _tracker;

    public AuthorsController(IMediator mediator, IRequestTracker tracker)
    {
        _mediator = mediator;
        _tracker = tracker;
    }

    public async Task<IActionResult> Index()
    {
        Console.WriteLine($">>> [Controller] RequestId: {_tracker.RequestId}");
        var authors = await _mediator.Send(new GetAllAuthorsQuery());

        return View(authors);
    }
}
