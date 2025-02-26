namespace Application.Features.User.Dtos
{
    using Core.Dtos.Blobs;
    using Microsoft.AspNetCore.Http;

    public class CreateUserDto: UploadBlobDto<IFormFile>
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string Password { get; set; } = null!;

        public IList<int>? RolesId { get; set; }
    }
}