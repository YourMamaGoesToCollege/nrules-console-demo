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
    }
}
