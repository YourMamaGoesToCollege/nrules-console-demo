using AccountEntities;

namespace AccountBusiness
{
    /// <summary>
    /// Interface for account business logic service.
    /// Makes the business layer injectable and mockable for tests.
    /// </summary>
    public interface IAccountBusinessService
    {
        void ValidateAccountInput(string firstName, string lastName, DateTime birthDate, string emailAddress, string city, int petCount);
        void ValidateAccount(Account account);
        void ValidateAccountId(int accountId);
    Task<Account> CreateAccountAsync(Account account);
    Account PrepareForSave(Account account);
        string NormalizeEmail(string email);
    }
}
