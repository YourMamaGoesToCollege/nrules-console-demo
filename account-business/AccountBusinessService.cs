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
    private readonly AccountValidationService _validator;
    private readonly AccountBusinessRules _rules;
    private readonly AccountBusiness.Actions.IActionFactory _actionFactory;

        public AccountBusinessService()
            : this(new AccountBusiness.Actions.DefaultActionFactory(null))
        {
        }

        public AccountBusinessService(AccountBusiness.Actions.IActionFactory actionFactory)
        {
            _validator = new AccountValidationService();
            _rules = new AccountBusinessRules();
            _actionFactory = actionFactory ?? new AccountBusiness.Actions.DefaultActionFactory(null);
        }

        public async Task<Account> CreateAccountAsync(Account account){
            // if (account == null) throw new ArgumentNullException(nameof(account));

            // // Validate using business service
            // _validator.ValidateAccountInput(account.FirstName, account.LastName, account.BirthDate, account.EmailAddress, account.City, account.PetCount);

            // // Prepare and return
            // var prepared = PrepareForSave(account);
            // return prepared;

            // Validate input first (throws ArgumentException on bad input)
            ValidateAccount(account);

            var action = _actionFactory.Create<CreateAccountAction>(account);
            var createdAccount = await action.ExecuteAsync();

            // Map action result back onto the Account model so callers get meaningful fields set
            if (createdAccount != null)
            {
                // Map email and timestamps
                account.EmailAddress = createdAccount.Email ?? account.EmailAddress;
                account.CreatedAt = createdAccount.CreatedAt;
                account.UpdatedAt = createdAccount.CreatedAt;

                // Try to split username into first/last name ("First Last")
                if (!string.IsNullOrWhiteSpace(createdAccount.Username))
                {
                    var parts = createdAccount.Username.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        account.FirstName = parts[0];
                    }
                    else if (parts.Length >= 2)
                    {
                        account.FirstName = parts[0];
                        account.LastName = string.Join(' ', parts.Skip(1));
                    }
                }
            }

            // Action executed (e.g., logging, side-effects). Return prepared Account for persistence.
            var prepared = PrepareForSave(account);
            return prepared;
        }

        /// <summary>
        /// Validate primitives used to create/update an account.
        /// Throws ArgumentException on invalid input.
        /// </summary>
        public void ValidateAccountInput(string firstName, string lastName, DateTime birthDate, string emailAddress, string city, int petCount)
        {
            _validator.ValidateAccountInput(firstName, lastName, birthDate, emailAddress, city, petCount);
        }

        /// <summary>
        /// Validate an Account entity directly (convenience wrapper).
        /// </summary>
        public void ValidateAccount(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            _validator.ValidateAccountInput(account.FirstName, account.LastName, account.BirthDate, account.EmailAddress, account.City, account.PetCount);
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
            _validator.ValidateAccountId(accountId);
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
