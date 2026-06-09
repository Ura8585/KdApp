using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Kdbapp.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
        {
            context.Result = new ForbidResult();
        }
    }
}