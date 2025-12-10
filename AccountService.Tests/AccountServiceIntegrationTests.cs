using AccountEntities;
using AccountRepository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AccountSvc = AccountService.AccountService;

namespace AccountService.Tests
{
    /// <summary>
    /// Integration tests for AccountService using in-memory database
    /// </summary>
    public class AccountServiceIntegrationTests
    {
        /// <summary>
        /// Create in-memory DbContext for testing
        /// </summary>
        private AccountDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AccountDbContext(options);
        }

        // Helper to create AccountService with business service for DI
        private AccountSvc CreateService(AccountRepository.AccountRepository repo)
        {
            return new AccountSvc(repo, new AccountBusiness.AccountBusinessService());
        }

        #region CreateAccount Tests

        [Fact]
        public async Task CreateAccountAsync_WithValidData_ReturnsAccountWithId()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            var firstName = "John";
            var lastName = "Doe";
            var birthDate = new DateTime(1990, 1, 15);
            var email = "john.doe@example.com";
            var city = "New York";
            var petCount = 2;

            // Act
            var result = await service.CreateAccountAsync(new Account
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                EmailAddress = email,
                City = city,
                PetCount = petCount,
                IsActive = true
            });

            // Assert
            result.Should().NotBeNull();
            result.AccountId.Should().BeGreaterThan(0);
            result.FirstName.Should().Be(firstName);
            result.LastName.Should().Be(lastName);
            result.BirthDate.Should().Be(birthDate);
            result.EmailAddress.Should().Be(email.ToLower());
            result.City.Should().Be(city);
            result.PetCount.Should().Be(petCount);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAccountAsync_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            var email = "test@example.com";

            // Act & Assert
            await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = email });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAccountAsync(new Account { FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1992,5,20), EmailAddress = email }));

            exception.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task CreateAccountAsync_WithoutCity_CreatesAccountSuccessfully()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            // Act
            var result = await service.CreateAccountAsync(new Account { FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1992,5,20), EmailAddress = "jane@example.com" });

            // Assert
            result.Should().NotBeNull();
            result.City.Should().BeEmpty();
            result.PetCount.Should().Be(0);
        }

        [Fact]
        public async Task CreateAccountAsync_WithEmptyFirstName_ThrowsArgumentException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAccountAsync(new Account { FirstName = "", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" }));

            exception.ParamName.Should().Be("firstName");
        }

        [Fact]
        public async Task CreateAccountAsync_WithEmptyLastName_ThrowsArgumentException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAccountAsync(new Account { FirstName = "John", LastName = "", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" }));

            exception.ParamName.Should().Be("lastName");
        }

        [Fact]
        public async Task CreateAccountAsync_WithFutureBirthDate_ThrowsArgumentException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            var futureDate = DateTime.Now.AddDays(1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = futureDate, EmailAddress = "john@example.com" }));

            exception.Message.Should().Contain("cannot be in the future");
        }

        [Fact]
        public async Task CreateAccountAsync_WithInvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "invalid-email" }));

            exception.Message.Should().Contain("Invalid email format");
        }

        [Fact]
        public async Task CreateAccountAsync_WithNegativePetCount_ThrowsArgumentException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com", PetCount = -1 }));

            exception.Message.Should().Contain("cannot be negative");
        }

        [Fact]
        public async Task CreateAccountAsync_NormalizesEmailToLowercase()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = CreateService(repository);

            var email = "JOHN.DOE@EXAMPLE.COM";

            // Act
            var result = await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = email });

            // Assert
            result.EmailAddress.Should().Be(email.ToLower());
        }

        [Fact]
        public async Task CreateAccountAsync_TrimsWhitespace()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            // Act
            var result = await service.CreateAccountAsync(new Account { FirstName = "  John  ", LastName = "  Doe  ", BirthDate = new DateTime(1990,1,15), EmailAddress = "  john@example.com  " });

            // Assert
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.EmailAddress.Should().Be("john@example.com");
        }

        #endregion

        #region GetAccount Tests

        [Fact]
        public async Task GetAccountAsync_WithValidId_ReturnsAccount()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            var created = await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" });

            // Act
            var result = await service.GetAccountAsync(created.AccountId);

            // Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(created.AccountId);
            result.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetAccountAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            // Act
            var result = await service.GetAccountAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAccountByEmailAsync_WithValidEmail_ReturnsAccount()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            var email = "john@example.com";
            var created = await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = email });

            // Act
            var result = await service.GetAccountByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(created.AccountId);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_WithInvalidEmail_ReturnsNull()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            // Act
            var result = await service.GetAccountByEmailAsync("nonexistent@example.com");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllAccounts Tests

        [Fact]
        public async Task GetAllAccountsAsync_WithMultipleAccounts_ReturnsAllAccounts()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" });
            await service.CreateAccountAsync(new Account { FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1992,5,20), EmailAddress = "jane@example.com" });
            await service.CreateAccountAsync(new Account { FirstName = "Bob", LastName = "Johnson", BirthDate = new DateTime(1985,3,10), EmailAddress = "bob@example.com" });

            // Act
            var result = await service.GetAllAccountsAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetActiveAccountsAsync_ReturnsOnlyActiveAccounts()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com", IsActive = true });
            await service.CreateAccountAsync(new Account { FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1992,5,20), EmailAddress = "jane@example.com", IsActive = false });
            await service.CreateAccountAsync(new Account { FirstName = "Bob", LastName = "Johnson", BirthDate = new DateTime(1985,3,10), EmailAddress = "bob@example.com", IsActive = true });

            // Act
            var result = await service.GetActiveAccountsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(a => a.IsActive).Should().BeTrue();
        }

        #endregion

        #region UpdateAccount Tests

        [Fact]
        public async Task UpdateAccountAsync_WithValidData_UpdatesAccount()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            var created = await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com", PetCount = 2 });

            // Act - update by mutating the returned model and sending it back
            created.FirstName = "Jane";
            created.LastName = "Smith";
            created.BirthDate = new DateTime(1992,5,20);
            created.EmailAddress = "jane@example.com";
            created.PetCount = 3;

            var updated = await service.UpdateAccountAsync(created);

            // Assert
            updated.FirstName.Should().Be("Jane");
            updated.LastName.Should().Be("Smith");
            updated.PetCount.Should().Be(3);
        }

        [Fact]
        public async Task UpdateAccountAsync_WithNonexistentId_ThrowsException()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAccountAsync(new Account { AccountId = 999, FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" }));

            exception.Message.Should().Contain("not found");
        }

        #endregion

        #region DeleteAccount Tests

        [Fact]
        public async Task DeleteAccountAsync_WithValidId_DeletesAccount()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            var created = await service.CreateAccountAsync(new Account { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990,1,15), EmailAddress = "john@example.com" });

            // Act
            var result = await service.DeleteAccountAsync(created.AccountId);
            var retrieved = await service.GetAccountAsync(created.AccountId);

            // Assert
            result.Should().BeTrue();
            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAccountAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var repository = new AccountRepository.AccountRepository(dbContext);
            var service = new AccountSvc(repository);

            // Act
            var result = await service.DeleteAccountAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
