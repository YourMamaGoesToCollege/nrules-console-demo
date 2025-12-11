# Template Method Actions in .NET Business Layer

## Overview

This documentation introduces the template-method action pattern for business logic in .NET applications, focusing on the `CreateAccountAction` in this project. It explains the motivation, design, and practical benefits of using actions to encapsulate business operations, and demonstrates how this pattern improves separation of concerns, extensibility, and testability. The guide is intended for developers building layered .NET solutions who want to keep business logic clean, maintainable, and ready for future growth.

## Introduction

Modern .NET applications often require business logic that goes beyond simple CRUD operations. As systems grow, business processes may involve validation, normalization, side effects (such as sending emails or logging), and integration with external systems. The template-method action pattern provides a way to encapsulate these operations in dedicated classes, making your codebase more modular and easier to test.

This guide uses the example of account creation, showing how the `CreateAccountAction` fits into a layered architecture. It covers:

- What the action is and how it works
- Why actions are useful
- How they fit into the business/service flow
- The actual implementation in this project
- Practical design decisions and alternatives

## What is `CreateAccountAction`?

`CreateAccountAction` is a concrete implementation of the template-method pattern, extending `BusinessActionBase<Account>`:

```csharp
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
        // Uses NRules for validation (13 rules)
        // See implementation below
    }

    protected override async Task<Account> RunAsync()
    {
        // Normalize email, set timestamps, persist to repository
        // Returns the saved Account entity
    }

    protected override Task<Dictionary<string, object>> GetAuditContext(Account result)
    {
        // Provides audit context for logging
    }
}
```

**Key characteristics:**

- Extends `BusinessActionBase<Account>` (3-layer hierarchy: `BusinessAction<T>` → `BusinessActionBase<T>` → `CreateAccountAction`)
- Returns `Account` entity directly (no intermediate result type)
- Uses NRules for validation with 13 validation rules
- Trims fields in constructor before validation
- Normalizes email to lowercase in `RunAsync()`
- Persists to repository via `IAccountRepository`
- Implements audit logging via `GetAuditContext()`

## Why Use Actions and the Template Method Pattern?

### Separation of Concerns

- Keeps business orchestration (validation, normalization) separate from the steps needed to perform an operation (side effects, external calls).
- `AccountService` and `AccountBusinessService` remain thin and focused.
- The action encapsulates all account creation logic in one place.

### Single Responsibility & Extensibility

- Each action encapsulates one business operation with clear boundaries.
- Future changes (e.g., sending a welcome email, audit logging) are isolated to the action.
- The template method pattern provides hooks: `Validate()`, `RunAsync()`, `GetAuditContext()`.

### Testability

- Actions have clear input/output contracts (`Account` → `Account`).
- You can unit-test actions in isolation and mock them in business/service tests.
- NRules validation is testable using `NRules.Testing.RulesTestFixture`.

### Pluggable Creation Strategy

- The factory (`IActionFactory`) resolves actions from DI or Activator.
- You can swap implementations or decorate actions at runtime.
- Dependencies are injected automatically via constructor.

### Validation with NRules

- This project uses NRules for validation (single source of truth).
- All validation rules are in `account-business/rules/AccountValidationRule.cs`.
- 13 rules cover: FirstName, LastName, Email (Required/Format/Syntax/MaxLength), BirthDate, Age (Min/Max), PetCount, City.
- Email validation includes OWASP-compliant regex for syntax validation.
- See `docs/Email_Validation_Strategy.md` for comprehensive email validation documentation.

## How Does It Fit in the Flow?

1. UI/console/tests call `AccountService.CreateAccountAsync(Account account)`.
2. `AccountService` delegates to `AccountBusinessService.CreateAccountAsync(account)`.
3. `AccountBusinessService`:
   - Uses `IActionFactory.Create<CreateAccountAction>(account, repository, logger)`
   - Calls `action.ExecuteAsync()` → returns `Account`
   - The action handles all validation, normalization, and persistence internally
4. `AccountService` returns the saved `Account` to the caller

**Simplified flow:**

```
User/Test → AccountService → AccountBusinessService → CreateAccountAction
                                                            ↓
                                                      [Validate with NRules]
                                                            ↓
                                                      [Normalize & Persist]
                                                            ↓
                                                      [Audit Log]
                                                            ↓
                                                      Return Account
```

**Key difference from earlier design:**

- The action now returns `Account` directly (not `CreatedAccount`)
- All validation is done via NRules (no manual validation in business service)
- Field trimming happens in the action constructor
- Email normalization (lowercase) happens in `RunAsync()`

## The Three-Layer Hierarchy

This project uses a three-layer hierarchy for business actions:

### 1. `BusinessAction<T>` (Base Template)

Location: `account-business/actions/BusinessAction.cs`

```csharp
public abstract class BusinessAction<T>
{
    public async Task<T> ExecuteAsync()
    {
        await PreExecuteAsync();
        var result = await RunAsync();
        await PostExecuteAsync(result);
        return result;
    }

    protected virtual async Task PreExecuteAsync()
    {
        await Validate();
    }

    protected virtual Task Validate()
    {
        return Task.CompletedTask;
    }

    protected abstract Task<T> RunAsync();

    protected virtual async Task PostExecuteAsync(T result)
    {
        await PostValidate();
        await AuditLog(result);
    }

    protected abstract Task PostValidate();

    protected virtual Task AuditLog(T result)
    {
        return Task.CompletedTask;
    }
}
```

**Responsibilities:**

- Defines the template method pattern with `ExecuteAsync()`
- Provides virtual hooks: `Validate()`, `AuditLog()`
- Requires derived classes to implement: `RunAsync()`, `PostValidate()`

### 2. `BusinessActionBase<T>` (Common Implementation)

Location: `account-business/actions/BusinessActionBase.cs`

```csharp
public abstract class BusinessActionBase<T> : BusinessAction<T>
{
    protected List<string> ValidationErrors { get; } = new();

    protected override Task Validate()
    {
        ValidationErrors.Clear();
        return Task.CompletedTask;
    }

    protected override Task PostValidate()
    {
        if (ValidationErrors.Count > 0)
        {
            var exceptions = new List<Exception>();
            foreach (var error in ValidationErrors)
            {
                exceptions.Add(new ArgumentException(error));
            }
            throw new AggregateException("Validation failed", exceptions);
        }
        return Task.CompletedTask;
    }

    protected override async Task AuditLog(T result)
    {
        var context = await GetAuditContext(result);
        var actionName = GetType().Name;
        var resultType = result?.GetType().Name ?? "null";

        Console.WriteLine($"[AUDIT] Action: {actionName}");
        Console.WriteLine($"[AUDIT] Result Type: {resultType}");
        Console.WriteLine($"[AUDIT] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");

        if (context != null && context.Count > 0)
        {
            Console.WriteLine("[AUDIT] Context:");
            foreach (var kvp in context)
            {
                Console.WriteLine($"[AUDIT]   {kvp.Key}: {kvp.Value}");
            }
        }
    }

    protected virtual Task<Dictionary<string, object>> GetAuditContext(T result)
    {
        return Task.FromResult(new Dictionary<string, object>());
    }

    protected void AddValidationError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            ValidationErrors.Add(error);
        }
    }
}
```

**Responsibilities:**

- Implements validation error collection
- Implements `PostValidate()` to throw `AggregateException` if validation fails
- Implements audit logging with context
- Provides `AddValidationError()` helper for derived classes
- Provides virtual `GetAuditContext()` for custom audit data

### 3. `CreateAccountAction` (Concrete Implementation)

Location: `account-business/actions/CreateAccountAction.cs`

**Responsibilities:**

- Overrides `Validate()` to use NRules validation (13 rules)
- Implements `RunAsync()` with account creation logic
- Implements `GetAuditContext()` with account-specific audit data
- Trims fields in constructor
- Normalizes email in `RunAsync()`
- Persists to repository

## When You Might Not Need This Pattern

- If creation is a simple DB insert without validation or business logic, the action pattern may be overkill.
- You can remove the action and let the business service validate and call repository.AddAsync directly.
- For very simple CRUD operations, the action pattern adds complexity.

**However, this project benefits from the pattern because:**

- Account creation requires 13 validation rules (NRules)
- Email validation includes OWASP-compliant regex
- Field normalization (trimming, lowercase email)
- Audit logging requirements
- Repository persistence
- Future extensibility (welcome emails, events, etc.)

## Implementation Details

### Field Trimming

Fields are trimmed in the constructor before validation:

```csharp
public CreateAccountAction(
    Account account,
    IAccountRepository repository,
    ILogger<CreateAccountAction>? logger = null)
{
    // ... null checks ...

    // Trim fields immediately before validation
    _account.FirstName = _account.FirstName?.Trim();
    _account.LastName = _account.LastName?.Trim();
    _account.EmailAddress = _account.EmailAddress?.Trim();
    _account.City = _account.City?.Trim();
}
```

**Why in constructor?**

- Ensures fields are clean before validation runs
- Eliminates whitespace issues with regex validation
- Single point of responsibility for field preparation

### NRules Validation

The `Validate()` method uses NRules with 13 validation rules:

```csharp
protected override Task Validate()
{
    _logger?.LogInformation("Validating account creation for {Email}", _account.EmailAddress);

    // Create NRules repository and load validation rules
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

    return Task.CompletedTask;
}
```

**Validation Rules (13 total):**

1. `FirstNameRequiredRule` - FirstName cannot be null or empty
2. `LastNameRequiredRule` - LastName cannot be null or empty
3. `EmailAddressRequiredRule` - EmailAddress cannot be null or empty
4. `EmailAddressFormatRule` - EmailAddress must contain @ and .
5. `EmailAddressSyntaxRule` - EmailAddress must match OWASP regex
6. `EmailAddressMaxLengthRule` - EmailAddress max 100 characters
7. `BirthDateRequiredRule` - BirthDate must be set
8. `AgeMinimumRule` - Age must be at least 18
9. `AgeMaximumRule` - Age must be 120 or less
10. `PetCountMinimumRule` - PetCount must be 0 or more
11. `CityRequiredRule` - City cannot be null or empty
12. `CityMinLengthRule` - City must be at least 2 characters
13. `CityMaxLengthRule` - City must be 100 characters or less

See `account-business/rules/AccountValidationRule.cs` for implementations.

### Email Normalization

Email is normalized to lowercase in `RunAsync()`:

```csharp
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
```

### Audit Context

Audit context provides metadata for audit logging:

```csharp
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
```

## TL;DR

- `CreateAccountAction` extends `BusinessActionBase<Account>` which extends `BusinessAction<Account>`
- Returns `Account` entity directly (no intermediate result type)
- Uses NRules for validation with 13 validation rules
- Trims fields in constructor, normalizes email in `RunAsync()`
- Persists to repository and provides audit context
- This pattern improves separation of concerns, testability, and extensibility
- See `docs/Email_Validation_Strategy.md` for email validation details
- See `docs/NRules_Testing_Strategy.md` for testing approach

## What Next?

- Review `account-business/rules/AccountValidationRule.cs` for validation rules
- Review `AccountService.Tests/AccountValidationRulesTests.cs` for unit tests (45 tests)
- Review `docs/Email_Validation_Strategy.md` for OWASP email validation
- Add more actions for other business operations as needed
- Extend validation rules as business requirements evolve

---

### Full Example: `CreateAccountAction.cs`

```csharp
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
```
