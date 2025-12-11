using System;
using NRules.Fluent.Dsl;
using AccountEntities;

namespace AccountBusiness.Rules
{
    /// <summary>
    /// NRules validation rule for Account FirstName
    /// </summary>
    public class FirstNameRequiredRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => string.IsNullOrWhiteSpace(a.FirstName));

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("FirstName is required.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Account FirstName max length
    /// </summary>
    public class FirstNameMaxLengthRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => !string.IsNullOrWhiteSpace(a.FirstName) && a.FirstName.Length > 100);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("FirstName must be 100 characters or less.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Account LastName
    /// </summary>
    public class LastNameRequiredRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => string.IsNullOrWhiteSpace(a.LastName));

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("LastName is required.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Account LastName max length
    /// </summary>
    public class LastNameMaxLengthRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => !string.IsNullOrWhiteSpace(a.LastName) && a.LastName.Length > 100);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("LastName must be 100 characters or less.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Email Address required
    /// </summary>
    public class EmailAddressRequiredRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => string.IsNullOrWhiteSpace(a.EmailAddress));

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Email address is required.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Email Address format
    /// </summary>
    public class EmailAddressFormatRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a =>
                    !string.IsNullOrWhiteSpace(a.EmailAddress) &&
                    (!a.EmailAddress.Contains("@") || !a.EmailAddress.Contains(".")));

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Email address must be a valid email format.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Email Address max length
    /// </summary>
    public class EmailAddressMaxLengthRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a =>
                    !string.IsNullOrWhiteSpace(a.EmailAddress) &&
                    a.EmailAddress.Length > 255);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Email address must be 255 characters or less.")));
        }
    }

    /// <summary>
    /// NRules validation rule for BirthDate required
    /// </summary>
    public class BirthDateRequiredRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => a.BirthDate == default);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Birth date is required.")));
        }
    }

    /// <summary>
    /// NRules validation rule for minimum age
    /// </summary>
    public class MinimumAgeRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a =>
                    a.BirthDate != default && CalculateAge(a.BirthDate) < 18);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Account holder must be at least 18 years old.")));
        }

        private int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (birthDate.Date > DateTime.UtcNow.AddYears(-age).Date)
            {
                age--;
            }
            return age;
        }
    }

    /// <summary>
    /// NRules validation rule for maximum age
    /// </summary>
    public class MaximumAgeRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a =>
                    a.BirthDate != default && CalculateAge(a.BirthDate) > 120);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Birth date must be realistic (age cannot exceed 120 years).")));
        }

        private int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (birthDate.Date > DateTime.UtcNow.AddYears(-age).Date)
            {
                age--;
            }
            return age;
        }
    }

    /// <summary>
    /// NRules validation rule for Pet Count negative check
    /// </summary>
    public class PetCountNonNegativeRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => a.PetCount < 0);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Pet count must be a non-negative number.")));
        }
    }

    /// <summary>
    /// NRules validation rule for Pet Count maximum
    /// </summary>
    public class PetCountMaximumRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a => a.PetCount > 100);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("Pet count seems unrealistic (maximum 100).")));
        }
    }

    /// <summary>
    /// NRules validation rule for City max length
    /// </summary>
    public class CityMaxLengthRule : Rule
    {
        public override void Define()
        {
            Account account = null!;

            When()
                .Match<Account>(() => account, a =>
                    !string.IsNullOrWhiteSpace(a.City) &&
                    a.City.Length > 100);

            Then()
                .Do(ctx => ctx.Insert(new ValidationError("City must be 100 characters or less.")));
        }
    }

    /// <summary>
    /// Represents a validation error found by NRules
    /// </summary>
    public class ValidationError
    {
        public string Message { get; }

        public ValidationError(string message)
        {
            Message = message;
        }
    }
}
