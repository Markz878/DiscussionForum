using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DiscussionForum.Server.Filters;

public class FluentValidationSwaggerFilter : IRequestBodyFilter
{
    private readonly IServiceProvider services;
    public FluentValidationSwaggerFilter(IServiceProvider services)
    {
        this.services = services;
    }
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        Type validatorType = typeof(IValidator<>).MakeGenericType(context.BodyParameterDescription.Type);
        IValidator? validator = services.GetService(validatorType) as IValidator;
        if (validator is not null)
        {
            OpenApiSchema schema = context.SchemaRepository.Schemas[context.BodyParameterDescription.Type.Name];
            IValidatorDescriptor descriptor = validator.CreateDescriptor();
            ILookup<string, (IPropertyValidator Validator, IRuleComponent Options)> validationRules = descriptor.GetMembersWithValidators();
            foreach (IGrouping<string, (IPropertyValidator Validator, IRuleComponent Options)> validationRule in validationRules)
            {
                string property = validationRule.Key[..1].ToLower() + validationRule.Key[1..];
                foreach (IPropertyValidator propertyValidator in validationRule.Select(x => x.Validator))
                {
                    switch (propertyValidator)
                    {
                        case INotEmptyValidator:
                            schema.Properties[property].Nullable = false;
                            break;
                        case IMinimumLengthValidator minLengthValidator:
                            schema.Properties[property].MinLength = minLengthValidator.Min;
                            break;
                        case IMaximumLengthValidator maxLengthValidator:
                            schema.Properties[property].MaxLength = maxLengthValidator.Max;
                            break;
                        case IBetweenValidator betweenValidator:
                            schema.Properties[property].Minimum = Convert.ToDecimal(betweenValidator.From);
                            schema.Properties[property].Maximum = Convert.ToDecimal(betweenValidator.To);
                            schema.Properties[property].ExclusiveMinimum = betweenValidator.Name.Contains("exclusive", StringComparison.OrdinalIgnoreCase);
                            schema.Properties[property].ExclusiveMaximum = betweenValidator.Name.Contains("exclusive", StringComparison.OrdinalIgnoreCase);
                            break;
                        case IComparisonValidator comparisonValidator:
                            if (comparisonValidator.Comparison == Comparison.LessThan)
                            {
                                schema.Properties[property].Maximum = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                                schema.Properties[property].ExclusiveMaximum = true;
                            }
                            else if (comparisonValidator.Comparison == Comparison.LessThanOrEqual)
                            {
                                schema.Properties[property].Maximum = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                                schema.Properties[property].ExclusiveMaximum = false;
                            }
                            else if (comparisonValidator.Comparison == Comparison.GreaterThan)
                            {
                                schema.Properties[property].Minimum = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                                schema.Properties[property].ExclusiveMaximum = true;
                            }
                            else if (comparisonValidator.Comparison == Comparison.GreaterThanOrEqual)
                            {
                                schema.Properties[property].Minimum = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                                schema.Properties[property].ExclusiveMaximum = false;
                            }
                            break;
                    }
                }
            }
        }
    }
}