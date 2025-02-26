namespace Api.Policies
{
    using Core.Enums;
    using Microsoft.AspNetCore.Authorization;

    public class EditorPolicy : IAuthorizationRequirement { }

    public class EditorPolicyHandler : BasePolicyHandler<EditorPolicy>
    {
        protected override RoleEnum[] AllowedRoles => new[] { RoleEnum.Editor };
    }
}