using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.Metadata;
using System.Reflection;

namespace DiscussionForum.Server.Filters;

public static class ValidatorFactory
{
    public static RouteGroupBuilder AddFluentValidation(this RouteGroupBuilder builder)
    {
        return builder.WithParameterValidation();
    }

    private static TBuilder WithParameterValidation<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(eb =>
        {
            MethodInfo? methodInfo = eb.Metadata.OfType<MethodInfo>().FirstOrDefault();
            ArgumentNullException.ThrowIfNull(methodInfo);
            ParameterInfo[] parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type validatorType = typeof(IValidator<>).MakeGenericType(parameters[i].ParameterType);
                IValidator? validator = eb.ApplicationServices.GetService(validatorType) as IValidator;
                if (validator is not null)
                {
                    eb.Metadata.Add(new ProducesResponseTypeMetadata(typeof(HttpValidationProblemDetails), 400, new[] { "application/problem+json" }));
                    eb.FilterFactories.Add((_, next) =>
                    {
                        ValidationDescriptor validationDescriptor = new(i, parameters[i].ParameterType, validator);
                        return invocationContext =>
                        {
                            object? argument = invocationContext.Arguments[validationDescriptor.ArgumentIndex];
                            if (argument is not null)
                            {
                                ValidationResult validationResult = validationDescriptor.Validator.Validate(new ValidationContext<object>(argument));
                                if (!validationResult.IsValid)
                                {
                                    return ValueTask.FromResult<object?>(TypedResults.ValidationProblem(validationResult.ToDictionary()));
                                }
                            }
                            return next(invocationContext);
                        };
                    });
                    return;
                }
            }
        });
        return builder;
    }

    private sealed record ProducesResponseTypeMetadata(Type Type, int StatusCode, IEnumerable<string> ContentTypes) : IProducesResponseTypeMetadata;
    private sealed record ValidationDescriptor(int ArgumentIndex, Type ArgumentType, IValidator Validator);
}

