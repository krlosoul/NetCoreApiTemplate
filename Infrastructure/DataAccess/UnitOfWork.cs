namespace Infrastructure.DataAccess
{
    using Core.Interfaces.DataAccess;
    using Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Threading.Tasks;

    public class UnitOfWork(SampleContext dbContext) : IUnitOfWork
    {

        #region Properties
        private DbContext DbContext { get; set; } = dbContext;
        private IDbContextTransaction? _transaction;
        private IRepository<Role>? _roleRepository;
        private IRepository<User>? _userRepository;
        private IRepository<UserRole>? _userRoleRepository;
        #endregion

        #region Transactions
        public async Task BeginTransactionAsync()
        {
            _transaction ??= await DbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await SaveAsync();
            }
        }

        public async Task CloseTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }
        #endregion

        #region Repositories
        public IRepository<Role> RoleRepository
        {
            get
            {
                return _roleRepository ??= new Repository<Role>(DbContext);
            }
        }
        public IRepository<User> UserRepository
        {
            get
            {
                return _userRepository ??= new Repository<User>(DbContext);
            }
        }
        public IRepository<UserRole> UserRoleRepository
        {
            get
            {
                return _userRoleRepository ??= new Repository<UserRole>(DbContext);
            }
        }
        #endregion

        #region Private Methods
        private async Task SaveAsync()
        {
            await DbContext.SaveChangesAsync();
        }
        #endregion
    }
}