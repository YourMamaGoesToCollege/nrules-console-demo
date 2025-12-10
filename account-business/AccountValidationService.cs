using System.Text.RegularExpressions;

namespace AccountBusiness
{
    /// <summary>
    /// Business logic validation service for account operations
    /// Encapsulates all account-related validation rules
    /// </summary>
    public class AccountValidationService
    {
        /// <summary>
        /// Validates all account input fields
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        public void ValidateAccountInput(
            string firstName,
            string lastName,
            DateTime birthDate,
            string emailAddress,
            string city,
            int petCount)
        {
            ValidateFirstName(firstName);
            ValidateLastName(lastName);
            ValidateBirthDate(birthDate);
            ValidateEmailAddress(emailAddress);
            ValidateCity(city);
            ValidatePetCount(petCount);
        }

        /// <summary>
        /// Validates first name
        /// </summary>
        public void ValidateFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));

            if (firstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters", nameof(firstName));
        }

        /// <summary>
        /// Validates last name
        /// </summary>
        public void ValidateLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            if (lastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters", nameof(lastName));
        }

        /// <summary>
        /// Validates birth date
        /// </summary>
        public void ValidateBirthDate(DateTime birthDate)
        {
            if (birthDate == default)
                throw new ArgumentException("Birth date must be provided", nameof(birthDate));

            if (birthDate > DateTime.Now)
                throw new ArgumentException("Birth date cannot be in the future", nameof(birthDate));

            // Check if person is at least 1 day old (reasonable validation)
            if (birthDate.Date == DateTime.Now.Date)
                throw new ArgumentException("Birth date must be in the past", nameof(birthDate));
        }

        /// <summary>
        /// Validates email address format
        /// </summary>
        public void ValidateEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));

            var trimmed = emailAddress.Trim();

            if (trimmed.Length > 255)
                throw new ArgumentException("Email address cannot exceed 255 characters", nameof(emailAddress));

            // Basic email validation using regex
            if (!IsValidEmailFormat(trimmed))
                throw new ArgumentException("Invalid email format", nameof(emailAddress));
        }

        /// <summary>
        /// Validates city name
        /// </summary>
        public void ValidateCity(string city)
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            if (city.Length > 100)
                throw new ArgumentException("City cannot exceed 100 characters", nameof(city));
        }

        /// <summary>
        /// Validates pet count
        /// </summary>
        public void ValidatePetCount(int petCount)
        {
            if (petCount < 0)
                throw new ArgumentException("Pet count cannot be negative", nameof(petCount));
        }

        /// <summary>
        /// Validates account ID
        /// </summary>
        public void ValidateAccountId(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0", nameof(accountId));
        }

        /// <summary>
        /// Check if email format is valid using regex
        /// </summary>
        private bool IsValidEmailFormat(string email)
        {
            try
            {
                // RFC 5322 simplified regex pattern
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
