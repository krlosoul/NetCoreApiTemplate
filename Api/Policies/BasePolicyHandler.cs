namespace Api.Policies
{
    using System.Net;
    using System.Security.Claims;
    using Core.Enums;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Core.Dtos.ResponsesDto;

    public abstract class BasePolicyHandler<TRequirement> : AuthorizationHandler<TRequirement> where TRequirement : IAuthorizationRequirement
    {
        protected virtual RoleEnum[]? AllowedRoles { get; }

        protected async Task HandleForbiddenAccess(string error, int codigoResponse, HttpContext? httpContext)
        {
            if (!httpContext!.Items.ContainsKey("ErrorAuth"))
            {
                httpContext.Items.Add("ErrorAuth", error);
            }

            if (!httpContext.Items.ContainsKey("TipoError"))
            {
                httpContext.Response.StatusCode = codigoResponse;
                httpContext.Response.Headers.Append("WWW-Authenticate", $"Bearer error='{error}'");
                await httpContext.Response.WriteAsJsonAsync(Result.Success(error));
                await httpContext.Response.CompleteAsync();
            }
        }

        protected static bool IsAuthenticated(ClaimsPrincipal user)
        {
            return user.Identity != null && user.Identity.IsAuthenticated;
        }

        protected static bool IsTokenNotExpired(ClaimsPrincipal user)
        {
            var tokenExpirationSeconds = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (tokenExpirationSeconds != null && long.TryParse(tokenExpirationSeconds, out long expirationSeconds))
            {
                var expirationDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expirationSeconds).UtcDateTime;
                return expirationDateTimeUtc >= DateTime.UtcNow;
            }
            return false;
        }

        protected static bool HasRole(ClaimsPrincipal user)
        {
            return user.Claims.Any(c => c.Type == "UserRoles");
        }

        protected static bool HasRequiredRole(ClaimsPrincipal user, RoleEnum[] allowedRoles)
        {
            var rolesClaim = user.Claims.FirstOrDefault(c => c.Type == "UserRoles");
            if (rolesClaim == null) return false;
            var userRoles = rolesClaim.Value.Split(',');
            return allowedRoles.Any(role => userRoles.Contains(((int)role).ToString()));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
        {
            if (!IsAuthenticated(context.User))
            {
                context.Fail();
                await HandleForbiddenAccess("Authentication Error", (int)HttpStatusCode.Unauthorized, context.Resource as HttpContext);
            }

            if (!IsTokenNotExpired(context.User))
            {
                context.Fail();
                await HandleForbiddenAccess("Token Expired", (int)HttpStatusCode.Unauthorized, context.Resource as HttpContext);
            }

            if (!HasRole(context.User))
            {
                context.Fail();
                await HandleForbiddenAccess("Role Error", (int)HttpStatusCode.Forbidden, context.Resource as HttpContext);
            }

            if (!HasRequiredRole(context.User, AllowedRoles!))
            {
                context.Fail();
                await HandleForbiddenAccess("Required Role", (int)HttpStatusCode.Forbidden, context.Resource as HttpContext);
            }

            context.Succeed(requirement);
            await Task.CompletedTask;
        }
    }
}