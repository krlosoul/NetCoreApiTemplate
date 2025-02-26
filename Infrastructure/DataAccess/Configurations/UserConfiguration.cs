namespace Infrastructure.DataAccess.Configurations
{
    using Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    { 
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.Id).HasName("Pk_User");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "Uq_User_Email").IsUnique();
            entity.HasIndex(e => e.PhotoName, "Uq_User_PhotoName").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PhotoName).HasMaxLength(1000);
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        }
    }
}