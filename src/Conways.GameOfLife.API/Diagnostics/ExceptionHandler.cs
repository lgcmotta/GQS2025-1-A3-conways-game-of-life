using Conways.GameOfLife.API.Extensions;
using Conways.GameOfLife.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;

namespace Conways.GameOfLife.API.Diagnostics;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var instance = context.Request.Path.Value ?? string.Empty;

        var response = exception switch
        {
            ArgumentNullException e => e.ToProblemDetails(instance),
            ArgumentOutOfRangeException e => e.ToProblemDetails(instance),
            ArgumentException e => e.ToProblemDetails(instance),
            ValidationException e => e.ToProblemDetails(instance),
            BoardNotFoundException e => e.ToProblemDetails(instance),
            UnstableBoardException e => e.ToProblemDetails(instance),
            _ => exception.ToProblemDetails(instance)
        };

        context.Response.StatusCode = response.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        await context.Response.WriteAsJsonAsync(response, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return true;
    }
}