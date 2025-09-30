using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Server.Filters;

public class ExceptionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (BusinessException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (ForbiddenException)
        {
            return TypedResults.Unauthorized();
        }
        catch (ConflictException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred. Please try again later.");
        }
        //catch (ValidationException ex)
        //{
        //    return TypedResults.ValidationProblem(ex.ValidationResult.ToDictionary());
        //}
    }
}
