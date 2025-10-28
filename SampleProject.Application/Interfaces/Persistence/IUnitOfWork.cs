using Microsoft.EntityFrameworkCore.Storage;

namespace SampleProject.Application.Interfaces.Persistence
{
    /// <summary>
    /// Unit of Work interface for managing database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets repository for specified entity type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>Repository instance</returns>
        IRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>Database transaction</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="transaction">Transaction to commit</param>
        Task CommitTransactionAsync(IDbContextTransaction transaction);

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="transaction">Transaction to rollback</param>
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
    }
}

