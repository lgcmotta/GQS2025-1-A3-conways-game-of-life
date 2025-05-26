using FluentValidation;
using MediatR;

namespace Conways.GameOfLife.API.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationTasks = _validators.Select(validator => validator.ValidateAsync(request, cancellationToken));

        var validationResults = await Task.WhenAll(validationTasks).ConfigureAwait(continueOnCapturedContext: false);

        var validationFailures = validationResults.SelectMany(result => result.Errors).ToList();

        if (validationFailures.Count == 0)
        {
            return await next(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        throw new ValidationException(validationFailures);
    }
}