using System;
using System.Threading.Tasks;
using AccountBusiness.Actions;
using AccountEntities;
using Xunit;

namespace AccountBusiness.Tests
{
    public class CreateAccountActionTests
    {
        [Fact]
        public async Task CreateAccountAction_PreparesAndReturnsCreatedAccount()
        {
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true,
                PetCount = 0
            };

            var action = new CreateAccountAction(account);
            var created = await action.ExecuteAsync();

            Assert.NotNull(created);
            Assert.Equal("john@example.com", created.Email);
            Assert.Contains("John", created.Username);
        }

        [Fact]
        public void CreateAccountAction_Throws_WhenMissingNames()
        {
            var account = new Account
            {
                FirstName = "",
                LastName = "",
                EmailAddress = "john@example.com",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true,
                PetCount = 0
            };

            Assert.Throws<ArgumentException>(() => new CreateAccountAction(account));
        }
    }
}
