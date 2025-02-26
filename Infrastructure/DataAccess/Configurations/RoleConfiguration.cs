namespace Infrastructure.DataAccess.Configurations
{
    using Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
        public void Configure(EntityTypeBuilder<Role> entity)
        {
            entity.HasKey(e => e.Id).HasName("Pk_Role");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Description, "Uq_Role_Description").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(50);
        }
    }
}