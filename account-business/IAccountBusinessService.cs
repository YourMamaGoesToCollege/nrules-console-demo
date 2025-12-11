using AccountEntities;

namespace AccountBusiness
{
    /// <summary>
    /// Interface for account business logic service.
    /// Makes the business layer injectable and mockable for tests.
    /// </summary>
    public interface IAccountBusinessService
    {
        void ValidateAccountId(int accountId);
        Task<Account> CreateAccountAsync(Account account);
        Account PrepareForSave(Account account);
        string NormalizeEmail(string email);
    }
}
