using Microsoft.AspNetCore.Mvc;

namespace Conways.GameOfLife.API.Diagnostics;

public sealed class TracedProblemDetails : ProblemDetails
{
    public string? TraceId { get; init; }
}