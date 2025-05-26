using Conways.GameOfLife.API.Diagnostics;
using Conways.GameOfLife.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using System.Diagnostics;
using System.Text.Json;

namespace Conways.GameOfLife.API.Extensions;

internal static class TracedProblemDetailsExtensions
{
    internal static TracedProblemDetails ToProblemDetails(this ValidationException exception,
        string instance,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return exception.Errors.ToProblemDetails(instance, statusCode);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal static TracedProblemDetails ToProblemDetails(
        this IEnumerable<ValidationFailure> errors,
        string instance,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        var problemDetails = new TracedProblemDetails
        {
            Title = "Validation Error",
            Status = statusCode,
            Detail = "One or more properties have errors",
            Instance = instance,
            TraceId = Activity.Current?.TraceId.ToString(),
            Extensions =
            {
                ["errors"] = errors
                    .GroupBy(failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName))
                    .ToDictionary(
                        grouping => grouping.Key,
                        grouping => grouping.Select(failure => failure.ErrorMessage).ToArray()
                    )
            }
        };

        return problemDetails;
    }

    internal static TracedProblemDetails ToProblemDetails<TException>(
        this TException exception,
        string instance,
        int statusCode = StatusCodes.Status400BadRequest)
        where TException : ArgumentException
    {
        return exception.ToProblemDetails(
            title: "Bad input when processing request",
            detail: exception.Message,
            instance: instance,
            statusCode
        );
    }

    internal static TracedProblemDetails ToProblemDetails(
        this BoardNotFoundException exception,
        string instance,
        int statusCode = StatusCodes.Status404NotFound)
    {
        return exception.ToProblemDetails(
            title: "Board not found",
            detail: exception.Message,
            instance: instance,
            statusCode
        );
    }

    internal static TracedProblemDetails ToProblemDetails(
        this UnstableBoardException exception,
        string instance,
        int statusCode = StatusCodes.Status422UnprocessableEntity)
    {
        return exception.ToProblemDetails(
            title: "Unstable Board State",
            detail: exception.Message,
            instance: instance,
            statusCode
        );
    }

    internal static TracedProblemDetails ToProblemDetails(
        this Exception _,
        string instance)
    {
        return _.ToProblemDetails(
            title: "Unexpected error",
            detail: "An error occurred, please contact an administrator",
            instance: instance,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    private static TracedProblemDetails ToProblemDetails(
        this Exception _,
        string title,
        string detail,
        string instance,
        int statusCode)
    {
        return new TracedProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = instance,
            TraceId = Activity.Current?.TraceId.ToString(),
        };
    }
}