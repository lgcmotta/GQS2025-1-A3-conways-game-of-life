using Asp.Versioning;
using Conways.GameOfLife.API.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Conways.GameOfLife.API.Features.CreateBoard;

public static class CreateBoardEndpoint
{
    public static RouteGroupBuilder MapCreateBoardEndpoint(this RouteGroupBuilder group, ApiVersion version)
    {
        group.MapPost("/boards", PostAsync)
            .WithName("CreateNewBoard")
            .WithDisplayName("Create New Board")
            .WithTags("Create a New Board")
            .MapToApiVersion(version)
            .Produces<CreateBoardResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<TracedProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

        return group;
    }

    private static async Task<IResult> PostAsync(
        [FromBody] CreateBoardCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return Results.Ok(response);
    }
}