# Action Factory Pattern in .NET Business Layer

## Overview

This brief explains the role and usage of the action factory (`DefaultActionFactory`) in the business layer, with practical examples and use cases. It covers how the factory enables dependency injection for business actions, how actions can access services like repositories or other business services, and how to extend the pattern for advanced scenarios.

## What is the Action Factory?

The action factory is an abstraction (`IActionFactory`) for creating business action instances. The default implementation (`DefaultActionFactory`) can:

- Resolve actions from the DI container (IServiceProvider), so dependencies are injected automatically.
- Fall back to using `Activator.CreateInstance` if DI is not available.

This enables actions to have constructor dependencies (e.g., repositories, business services, loggers) and be managed by DI, making them more powerful and testable.

## How Does It Work?

### Factory Implementation

```csharp
public class DefaultActionFactory : IActionFactory
{
    private readonly IServiceProvider? _provider;

    public DefaultActionFactory(IServiceProvider? provider = null)
    {
        _provider = provider;
    }

    public T Create<T>(params object[] args) where T : class
    {
        if (_provider != null)
        {
            // Try to resolve from DI if available
            var service = _provider.GetService(typeof(T)) as T;
            if (service != null) return service;
        }
        // Fallback to Activator
        return Activator.CreateInstance(typeof(T), args) as T ?? throw new InvalidOperationException($"Unable to create action of type {typeof(T)}");
    }
}
```

- If DI is configured, actions can be resolved with all their dependencies.
- If not, actions are created with the provided constructor arguments.

### Typical Usage in Business Service

```csharp
var action = _actionFactory.Create<CreateAccountAction>(account);
```

- The business service requests an action instance, passing any required arguments.
- The factory resolves the action, injecting dependencies if DI is available.

## Use Cases

### 1. Actions with Repository Access

The `CreateAccountAction` in this project demonstrates how actions persist data directly:

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
```

**Key points:**

- The action extends `BusinessActionBase<Account>` (not `BusinessAction<T>` directly)
- Returns `Account` entity directly (no intermediate result type)
- Uses NRules for validation (see `Validate()` override)
- Implements `GetAuditContext()` for audit logging

- Register `CreateAccountAction` as transient in DI:

```csharp
services.AddTransient<CreateAccountAction>();
```

- The factory will inject the repository when resolving the action.

### 2. Actions Using Business Services

Actions can depend on business services for complex logic coordination:

```csharp
public class CustomAccountAction : BusinessActionBase<Account>
{
    private readonly IAccountBusinessService _businessService;
    private readonly Account _account;

    public CustomAccountAction(IAccountBusinessService businessService, Account account)
    {
        _businessService = businessService;
        _account = account;
    }

    protected override async Task<Account> RunAsync()
    {
        // Call other business logic
        var result = await _businessService.CreateAccountAsync(_account);
        // ... more logic ...
        return result;
    }
}
```

- Register `CustomAccountAction` in DI:

```csharp
services.AddTransient<CustomAccountAction>();
```

**Note:** This project uses `BusinessActionBase<T>` as the base class for all actions, not `BusinessAction<T>` directly. The base class provides validation error collection, audit logging, and common functionality.

### 3. Actions with Multiple Dependencies

Actions can take any number of services as constructor parameters:

```csharp
public class ComplexAction : BusinessActionBase<Account>
{
    private readonly IAccountRepository _repo;
    private readonly ILogger<ComplexAction> _logger;
    private readonly IAccountBusinessService _businessService;
    private readonly Account _account;

    public ComplexAction(
        IAccountRepository repo,
        ILogger<ComplexAction> logger,
        IAccountBusinessService businessService,
        Account account)
    {
        _repo = repo;
        _logger = logger;
        _businessService = businessService;
        _account = account;
    }

    protected override async Task<Account> RunAsync()
    {
        // Use all injected services
        _logger.LogInformation("Processing account");
        var result = await _businessService.CreateAccountAsync(_account);
        return result;
    }
}
```

- Register all dependencies in DI.
- The factory will resolve and inject them.

**Note:** The actual `CreateAccountAction` in this project uses NRules for validation (13 rules covering name, email, age, city, etc.). See `account-business/rules/AccountValidationRule.cs` for rule implementations.

## Injecting Multiple DI Services into an Action

You can inject as many services as you need into your action by declaring them as constructor parameters. The DI container will automatically resolve and inject all registered dependencies when the factory creates the action.

### Example: Action with Multiple Services (from actual project)

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
}
```

### DI Registration

Register all dependencies and the action in your DI setup:

```csharp
services.AddTransient<IAccountRepository, AccountRepository>();
services.AddTransient<IAccountBusinessService, AccountBusinessService>();
services.AddTransient<CreateAccountAction>();
```

### Factory Usage

When you call the factory, pass the Account entity:

```csharp
var action = _actionFactory.Create<CreateAccountAction>(account);
var result = await action.ExecuteAsync();
```

The factory will resolve all registered services and inject them, plus any arguments you provide.

**Summary:**

- Declare all needed services in your action's constructor.
- Register those services and the action in DI.
- The factory handles the rest: all dependencies are injected automatically.
- Actions extend `BusinessActionBase<T>` which provides validation error collection and audit logging.
- This project uses NRules for validation (see `AccountValidationRule.cs` for 13 validation rules).

## Where to Register Actions in DI

You should register your actions (like `CreateAccountAction`) in the DI container where you configure your application's services. For a .NET console app, this is typically in `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using AccountBusiness.Actions;

var services = new ServiceCollection();

// Register your repository, business service, and other dependencies
services.AddTransient<IAccountRepository, AccountRepository>();
services.AddTransient<AccountBusinessService>();

// Register the action so DI can inject dependencies
services.AddTransient<CreateAccountAction>();

// Register the action factory
services.AddSingleton<IActionFactory, DefaultActionFactory>(sp => new DefaultActionFactory(sp));

var provider = services.BuildServiceProvider();
```

**Key points:**

- Register all dependencies (repositories, business services, loggers, etc.) before registering the action.
- Register the action itself as transient (or scoped if needed).
- Register the action factory so it can resolve actions from DI.
- This setup ensures that when you call `_actionFactory.Create<CreateAccountAction>(account)`, DI will inject all required services into the action's constructor.

You can use this pattern in any .NET app—console, web, or service—by adding registrations to your DI setup (e.g., `Startup.cs`, `Program.cs`, or wherever you configure services`).
