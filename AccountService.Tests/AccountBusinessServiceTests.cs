using AccountEntities;
using AccountBusiness;
using FluentAssertions;
using Xunit;

namespace AccountService.Tests
{
    public class AccountBusinessServiceTests
    {
        [Fact]
        public void PrepareForSave_ValidAccount_NormalizesAndReturns()
        {
            var svc = new AccountBusinessService();
            var acc = new Account
            {
                FirstName = "  John  ",
                LastName = "  Doe  ",
                BirthDate = new DateTime(1990, 1, 1),
                EmailAddress = "  JOHN.DOE@EXAMPLE.COM  ",
                City = "  New York  ",
                PetCount = 2,
                IsActive = true
            };

            var prepared = svc.PrepareForSave(acc);

            prepared.FirstName.Should().Be("John");
            prepared.LastName.Should().Be("Doe");
            prepared.EmailAddress.Should().Be("john.doe@example.com");
            prepared.City.Should().Be("New York");
            prepared.PetCount.Should().Be(2);
        }

        [Fact]
        public void ValidateAccountId_Invalid_Throws()
        {
            var svc = new AccountBusinessService();
            Action act = () => svc.ValidateAccountId(0);
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("Account ID must be greater than 0");
        }
    }
}
