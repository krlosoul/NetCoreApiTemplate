namespace Infrastructure.DataAccess
{
    using Core.Dtos.SecretsDto;
    using Infrastructure.Extensions;
    using Microsoft.EntityFrameworkCore;

    public partial class SampleContext(DbContextOptions<SampleContext> options, DataBaseSecretDto dataBaseSecretDto) : DbContext(options)
    {
        #region Properties
        private readonly DataBaseSecretDto _dataBaseDto = dataBaseSecretDto;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.HasAnnotation("Relational:Collation", "Modern_Spanish_CI_AS");
           modelBuilder.ApplyAllConfigurations();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!string.IsNullOrEmpty(_dataBaseDto.ConnectionString)) optionsBuilder.UseSqlServer(_dataBaseDto.ConnectionString);
        }
    }  
}
