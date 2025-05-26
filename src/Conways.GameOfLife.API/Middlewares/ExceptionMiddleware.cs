using Microsoft.AspNetCore.Diagnostics;

namespace Conways.GameOfLife.API.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    private readonly IExceptionHandler _handler;

    public ExceptionMiddleware(IExceptionHandler handler)
    {
        _handler = handler;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception exception)
        {
            await _handler.TryHandleAsync(context, exception, context.RequestAborted)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}