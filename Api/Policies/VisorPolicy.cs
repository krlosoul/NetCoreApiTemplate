namespace Api.Policies
{
    using Core.Enums;
    using Microsoft.AspNetCore.Authorization;

    public class VisorPolicy : IAuthorizationRequirement { }

    public class VisorPolicyHandler : BasePolicyHandler<VisorPolicy>
    {
        protected override RoleEnum[] AllowedRoles => new[] { RoleEnum.Visor };
    } 
}