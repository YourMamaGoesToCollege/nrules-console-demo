using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountEntities;

namespace AccountBusiness.Actions
{
    public record CreatedAccount(Guid Id, string Username, string Email, DateTime CreatedAt);

    public class CreateAccountAction : BusinessActionBase<CreatedAccount>
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

        protected override Task ValidateInputs()
        {
            ValidateRequired(Username, nameof(Username));
            ValidateEmail(Email, nameof(Email));

            return Task.CompletedTask;
        }

        protected override Task<Dictionary<string, object>> GetAuditContext(CreatedAccount result)
        {
            var context = new Dictionary<string, object>
            {
                { "AccountId", result.Id },
                { "Username", result.Username },
                { "Email", result.Email },
                { "CreatedAt", result.CreatedAt }
            };

            return Task.FromResult(context);
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