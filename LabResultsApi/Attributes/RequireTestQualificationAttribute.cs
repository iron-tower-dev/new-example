using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LabResultsApi.Services;
using System.Security.Claims;

namespace LabResultsApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireTestQualificationAttribute : Attribute, IAsyncActionFilter
{
    private readonly short _testStandId;
    private readonly string _requiredLevel;

    public RequireTestQualificationAttribute(short testStandId, string requiredLevel = "TRAIN")
    {
        _testStandId = testStandId;
        _requiredLevel = requiredLevel;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasAccess = await authorizationService.CanAccessTestAsync(user, _testStandId, _requiredLevel);
        
        if (!hasAccess)
        {
            var employeeId = authorizationService.GetEmployeeId(user);
            var userRole = authorizationService.GetUserRole(user);
            
            context.Result = new ObjectResult(new 
            { 
                message = $"Insufficient qualification level for test stand {_testStandId}",
                requiredLevel = _requiredLevel,
                userRole = userRole,
                testStandId = _testStandId,
                employeeId = employeeId
            })
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}

// Extension method to make it easier to use with minimal APIs
public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder RequireTestQualification(this RouteHandlerBuilder builder, short testStandId, string requiredLevel = "TRAIN")
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var hasAccess = await authorizationService.CanAccessTestAsync(user, testStandId, requiredLevel);
            
            if (!hasAccess)
            {
                var employeeId = authorizationService.GetEmployeeId(user);
                var userRole = authorizationService.GetUserRole(user);
                
                return Results.Problem(
                    detail: $"Insufficient qualification level for test stand {testStandId}. Required: {requiredLevel}",
                    statusCode: 403,
                    title: "Insufficient Test Qualification",
                    extensions: new Dictionary<string, object?>
                    {
                        ["requiredLevel"] = requiredLevel,
                        ["testStandId"] = testStandId,
                        ["userRole"] = userRole,
                        ["employeeId"] = employeeId
                    }
                );
            }

            return await next(context);
        });
    }
}