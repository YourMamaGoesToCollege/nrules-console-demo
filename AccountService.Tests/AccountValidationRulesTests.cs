using AccountBusiness.Rules.Validation;
using AccountEntities;
using FluentAssertions;
using NRules.Testing;

namespace AccountService.Tests
{
    /// <summary>
    /// Unit tests for NRules validation rules using NRules.Testing framework.
    /// Each test validates a single rule in isolation to ensure proper rule behavior.
    /// </summary>
    public class AccountValidationRulesTests
    {
        #region FirstName Tests

        [Fact]
        public void FirstNameRequiredRule_WhenFirstNameIsNull_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            var account = new Account
            {
                FirstName = null!,
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("FirstName is required.");
        }

        [Fact]
        public void FirstNameRequiredRule_WhenFirstNameIsEmpty_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            var account = new Account
            {
                FirstName = "",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("FirstName is required.");
        }

        [Fact]
        public void FirstNameRequiredRule_WhenFirstNameIsWhitespace_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            var account = new Account
            {
                FirstName = "   ",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("FirstName is required.");
        }

        [Fact]
        public void FirstNameRequiredRule_WhenFirstNameIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void FirstNameMaxLengthRule_WhenFirstNameIs100Characters_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameMaxLengthRule>();
            var account = new Account
            {
                FirstName = new string('A', 100),
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void FirstNameMaxLengthRule_WhenFirstNameIs101Characters_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<FirstNameMaxLengthRule>();
            var account = new Account
            {
                FirstName = new string('A', 101),
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("FirstName must be 100 characters or less.");
        }

        #endregion

        #region LastName Tests

        [Fact]
        public void LastNameRequiredRule_WhenLastNameIsNull_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<LastNameRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = null!,
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("LastName is required.");
        }

        [Fact]
        public void LastNameRequiredRule_WhenLastNameIsEmpty_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<LastNameRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("LastName is required.");
        }

        [Fact]
        public void LastNameRequiredRule_WhenLastNameIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<LastNameRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void LastNameMaxLengthRule_WhenLastNameIs100Characters_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<LastNameMaxLengthRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = new string('B', 100),
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void LastNameMaxLengthRule_WhenLastNameIs101Characters_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<LastNameMaxLengthRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = new string('B', 101),
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("LastName must be 100 characters or less.");
        }

        #endregion

        #region EmailAddress Tests

        [Fact]
        public void EmailAddressRequiredRule_WhenEmailIsNull_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = null!,
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address is required.");
        }

        [Fact]
        public void EmailAddressRequiredRule_WhenEmailIsEmpty_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address is required.");
        }

        [Fact]
        public void EmailAddressRequiredRule_WhenEmailIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void EmailAddressFormatRule_WhenEmailHasNoAtSymbol_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressFormatRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "johnexample.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address must contain @ and . characters.");
        }

        [Fact]
        public void EmailAddressFormatRule_WhenEmailHasNoDot_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressFormatRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@examplecom",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address must contain @ and . characters.");
        }

        [Fact]
        public void EmailAddressFormatRule_WhenEmailIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressFormatRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailHasValidSpecialCharacters_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john+test_123@example-domain.co.uk",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailHasConsecutiveDots_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john..doe@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address does not match valid email syntax.");
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailStartsWithDot_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = ".john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address does not match valid email syntax.");
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailHasSpaces_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john doe@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address does not match valid email syntax.");
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenEmailHasInvalidCharacters_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john#doe@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address does not match valid email syntax.");
        }

        [Fact]
        public void EmailAddressSyntaxRule_WhenDomainIsTooShort_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.c",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address does not match valid email syntax.");
        }

        [Fact]
        public void EmailAddressMaxLengthRule_WhenEmailIs255Characters_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressMaxLengthRule>();
            // Create email: 243 'a' chars + '@' + 11 chars 'example.com' = 255 total
            var longEmail = new string('a', 243) + "@example.com";
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = longEmail,
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void EmailAddressMaxLengthRule_WhenEmailIs256Characters_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<EmailAddressMaxLengthRule>();
            // Create email: 244 'a' chars + '@' + 11 chars 'example.com' = 256 total
            var longEmail = new string('a', 244) + "@example.com";
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = longEmail,
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Email address must be 255 characters or less.");
        }

        #endregion

        #region BirthDate Tests

        [Fact]
        public void BirthDateRequiredRule_WhenBirthDateIsDefault_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<BirthDateRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = default
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Birth date is required.");
        }

        [Fact]
        public void BirthDateRequiredRule_WhenBirthDateIsValid_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<BirthDateRequiredRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        #endregion

        #region Age Validation Tests

        [Fact]
        public void MinimumAgeRule_WhenAge17_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MinimumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-17).AddDays(-1)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Account holder must be at least 18 years old.");
        }

        [Fact]
        public void MinimumAgeRule_WhenAge18_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MinimumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-18)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MinimumAgeRule_WhenAge19_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MinimumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-19)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MaximumAgeRule_WhenAge119_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MaximumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-119)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MaximumAgeRule_WhenAge120_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MaximumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-120)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MaximumAgeRule_WhenAge121_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<MaximumAgeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-121)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Birth date must be realistic (age cannot exceed 120 years).");
        }

        #endregion

        #region PetCount Tests

        [Fact]
        public void PetCountNonNegativeRule_WhenPetCountIsNegative_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountNonNegativeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = -1
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Pet count must be a non-negative number.");
        }

        [Fact]
        public void PetCountNonNegativeRule_WhenPetCountIsZero_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountNonNegativeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 0
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void PetCountNonNegativeRule_WhenPetCountIsPositive_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountNonNegativeRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 5
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void PetCountMaximumRule_WhenPetCountIs99_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountMaximumRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 99
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void PetCountMaximumRule_WhenPetCountIs100_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountMaximumRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 100
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void PetCountMaximumRule_WhenPetCountIs101_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<PetCountMaximumRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 101
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("Pet count seems unrealistic (maximum 100).");
        }

        #endregion

        #region City Tests

        [Fact]
        public void CityMaxLengthRule_WhenCityIs100Characters_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<CityMaxLengthRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                City = new string('C', 100)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void CityMaxLengthRule_WhenCityIs101Characters_ShouldInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<CityMaxLengthRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                City = new string('C', 101)
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(1);
            errors[0].Message.Should().Be("City must be 100 characters or less.");
        }

        [Fact]
        public void CityMaxLengthRule_WhenCityIsNull_ShouldNotInsertValidationError()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            testTarget.Setup.Rule<CityMaxLengthRule>();
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                City = null!
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        #endregion

        #region Multiple Rules Integration Tests

        [Fact]
        public void MultipleRules_WhenAllFieldsValid_ShouldNotInsertAnyValidationErrors()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            // Load all validation rules
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            testTarget.Setup.Rule<FirstNameMaxLengthRule>();
            testTarget.Setup.Rule<LastNameRequiredRule>();
            testTarget.Setup.Rule<LastNameMaxLengthRule>();
            testTarget.Setup.Rule<EmailAddressRequiredRule>();
            testTarget.Setup.Rule<EmailAddressFormatRule>();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            testTarget.Setup.Rule<EmailAddressMaxLengthRule>();
            testTarget.Setup.Rule<BirthDateRequiredRule>();
            testTarget.Setup.Rule<MinimumAgeRule>();
            testTarget.Setup.Rule<MaximumAgeRule>();
            testTarget.Setup.Rule<PetCountNonNegativeRule>();
            testTarget.Setup.Rule<PetCountMaximumRule>();
            testTarget.Setup.Rule<CityMaxLengthRule>();

            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                PetCount = 2,
                City = "New York"
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MultipleRules_WhenMultipleFieldsInvalid_ShouldInsertMultipleValidationErrors()
        {
            // Arrange
            var testTarget = new RulesTestFixture();
            // Load all validation rules
            testTarget.Setup.Rule<FirstNameRequiredRule>();
            testTarget.Setup.Rule<FirstNameMaxLengthRule>();
            testTarget.Setup.Rule<LastNameRequiredRule>();
            testTarget.Setup.Rule<LastNameMaxLengthRule>();
            testTarget.Setup.Rule<EmailAddressRequiredRule>();
            testTarget.Setup.Rule<EmailAddressFormatRule>();
            testTarget.Setup.Rule<EmailAddressMaxLengthRule>();
            testTarget.Setup.Rule<EmailAddressSyntaxRule>();
            testTarget.Setup.Rule<BirthDateRequiredRule>();
            testTarget.Setup.Rule<MinimumAgeRule>();
            testTarget.Setup.Rule<MaximumAgeRule>();
            testTarget.Setup.Rule<PetCountNonNegativeRule>();
            testTarget.Setup.Rule<PetCountMaximumRule>();
            testTarget.Setup.Rule<CityMaxLengthRule>();

            var account = new Account
            {
                FirstName = null!, // Invalid - FirstNameRequiredRule
                LastName = "", // Invalid - LastNameRequiredRule
                EmailAddress = "invalid-email", // Invalid - EmailAddressFormatRule (no dot)
                BirthDate = DateTime.UtcNow.AddYears(-17), // Invalid - MinimumAgeRule
                PetCount = -1, // Invalid - PetCountNonNegativeRule
                City = new string('C', 101) // Invalid - CityMaxLengthRule
            };

            // Act
            testTarget.Session.Insert(account);
            testTarget.Session.Fire();

            // Assert
            var errors = testTarget.Session.Query<ValidationError>().ToList();
            errors.Should().HaveCount(7);
            errors.Should().Contain(e => e.Message == "FirstName is required.");
            errors.Should().Contain(e => e.Message == "LastName is required.");
            errors.Should().Contain(e => e.Message == "Email address must contain @ and . characters.");
            errors.Should().Contain(e => e.Message == "Email address does not match valid email syntax.");
            errors.Should().Contain(e => e.Message == "Account holder must be at least 18 years old.");
            errors.Should().Contain(e => e.Message == "Pet count must be a non-negative number.");
            errors.Should().Contain(e => e.Message == "City must be 100 characters or less.");
        }

        #endregion
    }
}
