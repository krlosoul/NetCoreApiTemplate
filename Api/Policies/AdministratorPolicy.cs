namespace Api.Policies
{
    using Core.Enums;
    using Microsoft.AspNetCore.Authorization;

    public class AdministratorPolicy : IAuthorizationRequirement { }

    public class AdministratorPolicyHandler : BasePolicyHandler<AdministratorPolicy>
    {
        protected override RoleEnum[] AllowedRoles => new[] { RoleEnum.Administrator };
    } 
}