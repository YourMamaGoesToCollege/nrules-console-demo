using AccountEntities;
using AccountBusiness.Actions;

namespace AccountBusiness
{
    /// <summary>
    /// Single façade service for account business logic.
    /// Combines validation and normalization so callers (e.g. AccountService)
    /// can delegate all business-layer work to one class.
    /// </summary>
    public class AccountBusinessService : IAccountBusinessService
    {
        private readonly AccountBusinessRules _rules;
        private readonly AccountBusiness.Actions.IActionFactory _actionFactory;
        private readonly AccountRepository.IAccountRepository? _repository;

        public AccountBusinessService()
            : this(new AccountBusiness.Actions.DefaultActionFactory(null), null)
        {
        }

        public AccountBusinessService(
            AccountBusiness.Actions.IActionFactory actionFactory,
            AccountRepository.IAccountRepository? repository = null)
        {
            _rules = new AccountBusinessRules();
            _actionFactory = actionFactory ?? new AccountBusiness.Actions.DefaultActionFactory(null);
            _repository = repository;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            // If repository is not injected, throw exception
            if (_repository == null)
            {
                throw new InvalidOperationException(
                        "IAccountRepository must be injected into AccountBusinessService for CreateAccountAsync operations.");
            }

            // Execute action which validates with NRules and prepares account for persistence
            // Factory will resolve IAccountRepository and ILogger from DI if available
            var action = _actionFactory.Create<CreateAccountAction>(account);
            var preparedAccount = await action.ExecuteAsync();

            // Return prepared account (caller will persist it)
            return preparedAccount;
        }



        /// <summary>
        /// Normalize an email address according to business rules (trim + lower).
        /// </summary>
        public string NormalizeEmail(string email)
        {
            if (email == null) return string.Empty;
            return email.Trim().ToLower();
        }

        /// <summary>
        /// Validate an account id
        /// </summary>
        public void ValidateAccountId(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0", nameof(accountId));
        }

        /// <summary>
        /// Prepare an Account entity for persistence: apply defaults, normalize fields and check for required post-normalization invariants.
        /// Throws ArgumentException when required fields are missing after normalization.
        /// </summary>
        public Account PrepareForSave(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            // Apply defaults and normalization
            _rules.ApplyDefaults(account);
            var normalized = _rules.NormalizeAccountData(account);

            // Final sanity check
            if (!_rules.IsValidForSave(normalized))
                throw new ArgumentException("Account is missing required fields after normalization.");

            return normalized;
        }
    }
}
