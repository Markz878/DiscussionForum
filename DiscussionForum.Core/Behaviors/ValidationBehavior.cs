using MediatR.Pipeline;

namespace DiscussionForum.Core.Behaviors;
public sealed class ValidationBehavior<TRequest>(IServiceProvider serviceProvider) : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        IValidator<TRequest>? validator = serviceProvider.GetService(typeof(IValidator<TRequest>)) as IValidator<TRequest>;
        if (validator is { })
        {
            validator.ValidateAndThrow(request);
        }
        return Task.CompletedTask;
    }
}
