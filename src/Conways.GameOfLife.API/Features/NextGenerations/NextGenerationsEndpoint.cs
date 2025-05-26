using Asp.Versioning;
using Conways.GameOfLife.API.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Conways.GameOfLife.API.Features.NextGenerations;

public static class NextGenerationsEndpoint
{
    public static RouteGroupBuilder MapNextGenerationsEndpoint(this RouteGroupBuilder group, ApiVersion version)
    {
        group.MapGet("/boards/{boardId}/generations/{generations:int}", GetAsync)
            .WithName("GetNextGenerations")
            .WithDisplayName("Get Board Next x Generations")
            .WithTags("Get Board's Next x Generation")
            .MapToApiVersion(version)
            .Produces<NextGenerationsResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<TracedProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

        return group;
    }

    private static async Task<IResult> GetAsync(
        string boardId,
        int generations,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new NextGenerationsQuery(boardId, generations), cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return Results.Ok(response);
    }
}