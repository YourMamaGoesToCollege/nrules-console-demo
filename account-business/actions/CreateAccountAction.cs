using System;
using System.Threading.Tasks;
using AccountEntities;

namespace AccountBusiness.Actions
{
    public record CreatedAccount(Guid Id, string Username, string Email, DateTime CreatedAt);

    public class CreateAccountAction : BusinessAction<CreatedAccount>
    {
        public string Username { get; }
        public string Email { get; }

        private readonly Account _account;

        public CreateAccountAction(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _account = account;

            // Map existing Account model fields to the action's expectations.
            var first = account.FirstName?.Trim();
            var last = account.LastName?.Trim();

            Username = string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last)
                ? throw new ArgumentException("FirstName or LastName is required on Account", nameof(account))
                : $"{first} {last}".Trim();

            Email = account.EmailAddress?.Trim() ?? throw new ArgumentNullException(nameof(account.EmailAddress));
        }

        protected override Task PreExecuteAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username is required.", nameof(Username));

            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required.", nameof(Email));

            return Task.CompletedTask;
        }

        protected override async Task<CreatedAccount> RunAsync()
        {
            // Placeholder for persistence/creation logic.
            // Replace with real repository calls / transaction handling.
            await Task.Delay(10);

            return new CreatedAccount(
                Id: Guid.NewGuid(),
                Username: Username,
                Email: Email,
                CreatedAt: DateTime.UtcNow
            );
        }

    }
}