namespace Core.Interfaces.DataAccess
{
    using Core.Entities;
    using System.Threading.Tasks;

    public interface IUnitOfWork
    {
        #region Transactions
        /// <summary>
        /// starts a new transaction asynchronous.
        /// </summary>
        public Task BeginTransactionAsync();

        /// <summary>
        /// Commits all changes made to the database in the current transaction asynchronously.
        /// </summary>
        public Task CommitTransactionAsync();

        /// <summary>
        /// releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        public Task CloseTransactionAsync();

        /// <summary>
        /// Discards all changes made to the database in the current transaction asynchronously.
        /// </summary>
        public Task RollbackTransactionAsync();
        #endregion

        #region Repositories
        IRepository<Role> RoleRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<UserRole> UserRoleRepository { get; }
        #endregion
    }
}
