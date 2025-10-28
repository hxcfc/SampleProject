using System.Linq.Expressions;

namespace SampleProject.Application.Interfaces.Persistence
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Queryable collection of entities</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Gets entities by predicate
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        /// <returns>Queryable collection of entities</returns>
        IQueryable<T> GetBy(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets first entity by predicate
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds entity to the repository
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities to the repository
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns>Added entities</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates entity in the repository
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes entity from the repository
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Deletes entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteByIdAsync(Guid id);

        /// <summary>
        /// Checks if entity exists by predicate
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        /// <returns>True if entity exists</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Counts entities by predicate
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        /// <returns>Number of entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}

