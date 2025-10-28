using FluentValidation;
using MediatR;

namespace SampleProject.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for automatic FluentValidation
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    // Log validation failures for debugging
                    System.Console.WriteLine($"ValidationBehavior: Found {failures.Count} validation failures");
                    foreach (var failure in failures)
                    {
                        System.Console.WriteLine($"ValidationBehavior: {failure.PropertyName} - {failure.ErrorMessage}");
                    }
                    throw new FluentValidation.ValidationException(failures);
                }
            }
            else
            {
                System.Console.WriteLine($"ValidationBehavior: No validators found for {typeof(TRequest).Name}");
            }

            return await next();
        }
    }
}
