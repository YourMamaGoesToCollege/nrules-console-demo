using AccountEntities;

namespace AccountBusiness
{
    /// <summary>
    /// Interface for account business logic service.
    /// Makes the business layer injectable and mockable for tests.
    /// </summary>
    public interface IAccountBusinessService
    {
        Task<Account> CreateAccountAsync(Account account);
    }
}
