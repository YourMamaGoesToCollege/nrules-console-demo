using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountEntities;
using AccountRepository;
using Microsoft.Extensions.Logging;
using NRules;
using NRules.Fluent;
using AccountBusiness.Rules.Validation;

namespace AccountBusiness.Actions
{
    public class CreateAccountAction : BusinessActionBase<Account>
    {
        private readonly Account _account;
        private readonly IAccountRepository _repository;
        private readonly ILogger<CreateAccountAction>? _logger;

        public CreateAccountAction(
            Account account,
            IAccountRepository repository,
            ILogger<CreateAccountAction>? logger = null)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            _account = account;
            _repository = repository;
            _logger = logger;

            // Trim fields immediately before validation
            _account.FirstName = _account.FirstName?.Trim();
            _account.LastName = _account.LastName?.Trim();
            _account.EmailAddress = _account.EmailAddress?.Trim();
            _account.City = _account.City?.Trim();
        }

        protected override Task Validate()
        {
            _logger?.LogInformation("Validating account creation for {Email}", _account.EmailAddress);

            // Create NRules repository and load validation rules
            // Rules are organized in AccountBusiness.Rules.Validation namespace
            // Loading from assembly - namespace provides logical organization
            var repository = new RuleRepository();
            repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));

            // Compile rules into a session factory
            var factory = repository.Compile();

            // Create a session and insert the account for validation
            var session = factory.CreateSession();
            session.Insert(_account);

            // Fire all rules
            session.Fire();

            // Query for validation errors
            var errors = session.Query<ValidationError>().ToList();

            // Add errors to ValidationErrors collection
            foreach (var error in errors)
            {
                _logger?.LogWarning("Validation error: {ErrorMessage}", error.Message);
                AddValidationError(error.Message);
            }

            // Log validation result
            if (ValidationErrors.Count == 0)
            {
                _logger?.LogInformation("Validation passed for {FirstName} {LastName} ({Email})",
                    _account.FirstName, _account.LastName, _account.EmailAddress);
            }
            else
            {
                _logger?.LogError("Validation failed with {ErrorCount} error(s)", ValidationErrors.Count);
            }

            return Task.CompletedTask;
        }

        protected override Task<Dictionary<string, object>> GetAuditContext(Account result)
        {
            var context = new Dictionary<string, object>
            {
                { "AccountId", result.AccountId },
                { "Username", $"{result.FirstName} {result.LastName}" },
                { "Email", result.EmailAddress ?? string.Empty },
                { "CreatedAt", result.CreatedAt }
            };

            return Task.FromResult(context);
        }

        protected override async Task<Account> RunAsync()
        {
            _logger?.LogInformation("Preparing account for {FirstName} {LastName} ({Email})",
                _account.FirstName, _account.LastName, _account.EmailAddress);

            // Normalize email to lowercase (trimming already done in constructor)
            _account.EmailAddress = _account.EmailAddress?.ToLower();

            // Set timestamps
            _account.CreatedAt = DateTime.UtcNow;
            _account.UpdatedAt = DateTime.UtcNow;

            _logger?.LogInformation("Persisting account for {Email}", _account.EmailAddress);

            // Persist the account to the repository
            var savedAccount = await _repository.AddAsync(_account);

            _logger?.LogInformation("Account created successfully with ID {AccountId} for {Email}",
                savedAccount.AccountId, savedAccount.EmailAddress);

            return savedAccount;
        }
    }
}