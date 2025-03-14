using Application.Features.Role.Dtos;
using Core.Entities;

namespace Test.Stub
{
    public static class RoleStub
    {
        public static Role CreateRole()
        {
            return new Role
            {
              Description = "TestRole"  
            };
        }

        public static CreateRoleDto CreateRoleDto()
        {
            return new CreateRoleDto
            {
                Description = "Admin"
            };
        }
    }
}