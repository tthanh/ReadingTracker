using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ReadingTracker.Api.Models;

namespace ReadingTracker.Api.Filters;

public class ValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new ValidationError
                {
                    Field = x.Key,
                    Message = string.Join("; ", x.Value!.Errors.Select(e => e.ErrorMessage))
                })
                .ToList();

            var response = new ValidationErrorResponse
            {
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }

        base.OnActionExecuting(context);
    }
}

public class ModelValidationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = ApiResponse.ErrorResult(errors);
            context.Result = new BadRequestObjectResult(response);
        }

        base.OnActionExecuting(context);
    }
}
