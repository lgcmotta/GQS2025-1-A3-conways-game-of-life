using Asp.Versioning;
using Conways.GameOfLife.API.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Conways.GameOfLife.API.Features.FinalGeneration;

public static class FinalGenerationEndpoint
{
    public static RouteGroupBuilder MapFinalGenerationEndpoint(this RouteGroupBuilder group, ApiVersion version)
    {
        group.MapGet("/boards/{boardId}/generations/final", GetAsync)
            .WithName("GetFinalGeneration")
            .WithDisplayName("Get Board Final Generation After x Attempts")
            .WithTags("Get Board's Final Generation")
            .MapToApiVersion(version)
            .Produces<FinalGenerationResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<TracedProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status422UnprocessableEntity, MediaTypeNames.Application.ProblemJson)
            .Produces<TracedProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

        return group;
    }

    private static async Task<IResult> GetAsync(
        string boardId,
        [FromQuery] int? maxAttempts,
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        maxAttempts ??= configuration.GetValue<int>("Board:MaxAttempts");

        var response = await mediator.Send(new FinalGenerationQuery(boardId, maxAttempts.Value), cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return Results.Ok(response);
    }
}