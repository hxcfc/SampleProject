using Microsoft.EntityFrameworkCore.Storage;
using SampleProject.Persistence.Data;
using SampleProject.Persistence.Repositories;
using SampleProject.Application.Interfaces.Persistence;

namespace SampleProject.Persistence.UnitOfWork
{
    /// <summary>
    /// Unit of Work implementation for managing database transactions
    /// Implements interface from Application layer to maintain clean architecture
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class
        /// </summary>
        /// <param name="context">Database context</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
        }

        /// <inheritdoc />
        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<T>(_context);
            }

            return (IRepository<T>)_repositories[type];
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
        {
            await transaction.RollbackAsync();
        }

        /// <summary>
        /// Disposes the unit of work and releases resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the unit of work and releases resources
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
