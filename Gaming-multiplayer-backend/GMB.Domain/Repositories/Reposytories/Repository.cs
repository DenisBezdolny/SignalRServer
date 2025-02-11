using GMB.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GMB.Domain.Repositories.Reposytories
{
    /// <summary>
    /// Provides a generic repository implementation for basic CRUD operations on entities of type T.
    /// </summary>
    /// <typeparam name="T">The type of entity that this repository manages. T must be a class.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly GMB_DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class using the specified database context.
        /// </summary>
        /// <param name="dbContext">The Entity Framework Core database context used for data access operations.</param>
        public Repository(GMB_DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        /// <summary>
        /// Retrieves all entities of type T from the database.
        /// </summary>
        /// <returns>A collection of all entities of type T.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Finds and returns an entity with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// The entity if found; otherwise, null.
        /// </returns>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Adds a new entity to the database and saves the changes.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing entity in the database and saves the changes.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an entity with the specified unique identifier from the database and saves the changes.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
