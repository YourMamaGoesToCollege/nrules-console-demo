using AccountEntities;

namespace AccountBusiness
{
    /// <summary>
    /// Business rules service for account operations
    /// Encapsulates data normalization and business logic transformations
    /// </summary>
    public class AccountBusinessRules
    {
        /// <summary>
        /// Normalizes account data according to business rules
        /// Applies trimming, casing, and other standardization rules
        /// </summary>
        /// <param name="account">The account to normalize</param>
        /// <returns>The normalized account</returns>
        public Account NormalizeAccountData(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Normalize string fields: trim whitespace and apply case rules
            account.FirstName = NormalizeText(account.FirstName);
            account.LastName = NormalizeText(account.LastName);
            account.EmailAddress = NormalizeEmail(account.EmailAddress);
            account.City = NormalizeText(account.City);

            return account;
        }

        /// <summary>
        /// Normalizes text fields: trim and preserve casing
        /// </summary>
        private string NormalizeText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Trim();
        }

        /// <summary>
        /// Normalizes email: trim and convert to lowercase for consistency
        /// </summary>
        private string NormalizeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            return email.Trim().ToLower();
        }

        /// <summary>
        /// Applies default values to account fields
        /// </summary>
        /// <param name="account">The account to initialize with defaults</param>
        public void ApplyDefaults(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Apply defaults only if not already set
            if (string.IsNullOrEmpty(account.City))
                account.City = string.Empty;

            if (account.PetCount < 0)
                account.PetCount = 0;

            // Do not override IsActive here. Leave as provided by caller.
        }

        /// <summary>
        /// Validates that an account has required fields after normalization
        /// </summary>
        /// <param name="account">The account to validate</param>
        /// <returns>true if account is valid, false otherwise</returns>
        public bool IsValidForSave(Account account)
        {
            if (account == null)
                return false;

            // Check required fields
            if (string.IsNullOrWhiteSpace(account.FirstName))
                return false;

            if (string.IsNullOrWhiteSpace(account.LastName))
                return false;

            if (string.IsNullOrWhiteSpace(account.EmailAddress))
                return false;

            if (account.BirthDate == default)
                return false;

            return true;
        }
    }
}
