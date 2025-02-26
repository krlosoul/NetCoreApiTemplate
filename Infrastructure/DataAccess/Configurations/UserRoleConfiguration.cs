namespace Infrastructure.DataAccess.Configurations
{
    using Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> entity)
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId  }).HasName("Pk_UserRole");

            entity.ToTable("UserRole");
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .HasConstraintName("Fk_UserRole_User");

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .HasConstraintName("Fk_UserRole_Role");
        }
    }
}