using AccountEntities;
using Microsoft.EntityFrameworkCore;

namespace AccountRepository
{
    /// <summary>
    /// Repository implementation for Account data access operations
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDbContext _context;

        public AccountRepository(AccountDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Account> AddAsync(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Check if email already exists
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.EmailAddress == account.EmailAddress);

            if (existingAccount != null)
                throw new InvalidOperationException($"An account with email '{account.EmailAddress}' already exists.");

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Query the DB for the persisted entity to guarantee we return an instance with database-generated
            // values populated (works reliably across providers, including InMemory).
            var persistedEntity = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EmailAddress == account.EmailAddress);

            return persistedEntity ?? account;
        }

        public async Task<Account?> GetByIdAsync(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0", nameof(accountId));

            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public async Task<Account?> GetByEmailAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));

            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.EmailAddress == emailAddress);
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetActiveAsync()
        {
            return await _context.Accounts
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == account.AccountId);

            if (existingAccount == null)
                throw new InvalidOperationException($"Account with ID {account.AccountId} not found.");

            // Update properties
            existingAccount.FirstName = account.FirstName;
            existingAccount.LastName = account.LastName;
            existingAccount.BirthDate = account.BirthDate;
            existingAccount.IsActive = account.IsActive;
            existingAccount.City = account.City;
            existingAccount.EmailAddress = account.EmailAddress;
            existingAccount.PetCount = account.PetCount;

            _context.Accounts.Update(existingAccount);
            await _context.SaveChangesAsync();

            return existingAccount;
        }

        public async Task<bool> DeleteAsync(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0", nameof(accountId));

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                return false;

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EmailExistsAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));

            return await _context.Accounts
                .AnyAsync(a => a.EmailAddress == emailAddress);
        }
    }
}
