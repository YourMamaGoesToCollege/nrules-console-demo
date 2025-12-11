using AccountEntities;

namespace AccountRepository
{
    /// <summary>
    /// Repository interface for Account data access operations
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>
        /// Add a new account to the repository
        /// </summary>
        Task<Account> AddAsync(Account account);

        /// <summary>
        /// Get an account by ID
        /// </summary>
        Task<Account?> GetByIdAsync(int accountId);

        /// <summary>
        /// Get an account by email address
        /// </summary>
        Task<Account?> GetByEmailAsync(string emailAddress);

        /// <summary>
        /// Get all accounts
        /// </summary>
        Task<IEnumerable<Account>> GetAllAsync();

        /// <summary>
        /// Get all active accounts
        /// </summary>
        Task<IEnumerable<Account>> GetActiveAsync();

        /// <summary>
        /// Update an existing account
        /// </summary>
        Task<Account> UpdateAsync(Account account);

        /// <summary>
        /// Delete an account by ID
        /// </summary>
        Task<bool> DeleteAsync(int accountId);

        /// <summary>
        /// Check if an email already exists
        /// </summary>
        Task<bool> EmailExistsAsync(string emailAddress);
    }
}
