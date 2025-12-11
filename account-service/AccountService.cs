using AccountEntities;
using AccountRepository;
using AccountBusiness;

namespace AccountService
{
    /// <summary>
    /// Service for managing account operations and business logic
    /// Orchestrates repository operations with business layer validation and rules
    /// </summary>
    public class AccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountBusinessService _businessService;

        // Primary constructor for DI
        public AccountService(IAccountRepository accountRepository, IAccountBusinessService businessService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));
        }

        // Convenience constructor to preserve existing callers
        public AccountService(IAccountRepository accountRepository)
            : this(accountRepository, new AccountBusinessService(
                new AccountBusiness.Actions.DefaultActionFactory(null),
                accountRepository))
        {
        }

        /// <summary>
        /// Create a new account with validation
        /// </summary>
        /// Model-only create API

        /// <summary>
        /// Create account using an Account entity (business-layer friendly)
        /// </summary>
        public async Task<Account> CreateAccountAsync(Account account)
        {
            //TODO: DETERMINE IN BLL WITH VALIDATORS AND RULES
            // if (account == null) throw new ArgumentNullException(nameof(account)); 

            // Delegate to business layer which may execute actions and normalize the model
            var prepared = await _businessService.CreateAccountAsync(account);

            // Persist via repository and return persisted entity (with AccountId populated)
            return await _accountRepository.AddAsync(prepared);
        }

        /// <summary>
        /// Get an account by ID
        /// </summary>
        public async Task<Account?> GetAccountAsync(int accountId)
        {
            _businessService.ValidateAccountId(accountId);
            return await _accountRepository.GetByIdAsync(accountId);
        }

        /// <summary>
        /// Get an account by email
        /// </summary>
        public async Task<Account?> GetAccountByEmailAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));

            // Normalize email for lookup
            var normalizedEmail = emailAddress.Trim().ToLower();
            return await _accountRepository.GetByEmailAsync(normalizedEmail);
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return await _accountRepository.GetAllAsync();
        }

        /// <summary>
        /// Get all active accounts
        /// </summary>
        public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
        {
            return await _accountRepository.GetActiveAsync();
        }

        /// <summary>
        /// Update an account
        /// </summary>
        // Model-only update API below

        /// <summary>
        /// Update account using an Account entity
        /// </summary>
        public async Task<Account> UpdateAccountAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            // Validate id
            _businessService.ValidateAccountId(account.AccountId);

            var existing = await _accountRepository.GetByIdAsync(account.AccountId);
            if (existing == null)
                throw new InvalidOperationException($"Account with ID {account.AccountId} not found.");

            // Check if new email is already taken by another account
            var normalizedNewEmail = _businessService.NormalizeEmail(account.EmailAddress);
            if (existing.EmailAddress != normalizedNewEmail)
            {
                if (await _accountRepository.EmailExistsAsync(normalizedNewEmail))
                    throw new InvalidOperationException($"An account with email '{account.EmailAddress}' already exists.");
            }

            // Apply incoming fields
            existing.FirstName = account.FirstName;
            existing.LastName = account.LastName;
            existing.BirthDate = account.BirthDate;
            existing.EmailAddress = account.EmailAddress;
            existing.City = account.City;
            existing.PetCount = account.PetCount;
            existing.IsActive = account.IsActive;

            var prepared = _businessService.PrepareForSave(existing);
            return await _accountRepository.UpdateAsync(prepared);
        }

        /// <summary>
        /// Delete an account
        /// </summary>
        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            _businessService.ValidateAccountId(accountId);
            return await _accountRepository.DeleteAsync(accountId);
        }
    }
}
