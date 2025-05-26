using Asp.Versioning;
using Conways.GameOfLife.API.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Conways.GameOfLife.API.Features.NextGeneration;

public static class NextGenerationEndpoint
{
    public static RouteGroupBuilder MapNextGenerationEndpoint(this RouteGroupBuilder group, ApiVersion version)
    {
        group.MapGet(pattern: "/boards/{boardId}/generations/next", GetAsync)
            .WithName(endpointName: "GetNextGeneration")
            .WithDisplayName(displayName: "Get Board Next Generation")
            .WithTags(tags: "Get Board's Next Generation")
            .MapToApiVersion(apiVersion: version)
            .Produces<NextGenerationResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<TracedProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

        return group;
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] string boardId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new NextGenerationQuery(boardId), cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return Results.Ok(response);
    }
}