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

Suppose you want an action to persist data directly:

```csharp
public class CreateAccountAction : BusinessAction<CreatedAccount>
{
    private readonly IAccountRepository _repository;
    private readonly Account _account;

    public CreateAccountAction(IAccountRepository repository, Account account)
    {
        _repository = repository;
        _account = account;
    }

    protected override async Task<CreatedAccount> RunAsync()
    {
        // Use repository to persist
        var saved = await _repository.AddAsync(_account);
        return new CreatedAccount(
            Id: Guid.NewGuid(),
            Username: saved.FirstName + " " + saved.LastName,
            Email: saved.EmailAddress,
            CreatedAt: saved.CreatedAt
        );
    }
}
```

- Register `CreateAccountAction` as transient in DI:

```csharp
services.AddTransient<CreateAccountAction>();
```

- The factory will inject the repository when resolving the action.

### 2. Actions Using Other Business Services

Actions can depend on other business services for advanced logic:

```csharp
public class CustomAccountAction : BusinessAction<CustomResult>
{
    private readonly AccountBusinessService _businessService;
    private readonly Account _account;

    public CustomAccountAction(AccountBusinessService businessService, Account account)
    {
        _businessService = businessService;
        _account = account;
    }

    protected override async Task<CustomResult> RunAsync()
    {
        // Call other business logic
        var prepared = _businessService.PrepareForSave(_account);
        // ... more logic ...
        return new CustomResult(...);
    }
}
```

- Register `CustomAccountAction` in DI:

```csharp
services.AddTransient<CustomAccountAction>();
```

### 3. Actions with Multiple Dependencies

Actions can take any number of services as constructor parameters:

```csharp
public class ComplexAction : BusinessAction<ComplexResult>
{
    public ComplexAction(IAccountRepository repo, ILogger logger, AccountBusinessService businessService, Account account) { ... }
    // ...
}
```

- Register all dependencies in DI.
- The factory will resolve and inject them.

## Injecting Multiple DI Services into an Action

You can inject as many services as you need into your action by declaring them as constructor parameters. The DI container will automatically resolve and inject all registered dependencies when the factory creates the action.

### Example: Action with Multiple Services

```csharp
public class AuditAccountAction : BusinessAction<AuditResult>
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<AuditAccountAction> _logger;
    private readonly AccountBusinessService _businessService;
    private readonly Account _account;

    public AuditAccountAction(
        IAccountRepository repository,
        ILogger<AuditAccountAction> logger,
        AccountBusinessService businessService,
        Account account)
    {
        _repository = repository;
        _logger = logger;
        _businessService = businessService;
        _account = account;
    }

    protected override async Task<AuditResult> RunAsync()
    {
        // Use all injected services
        var prepared = _businessService.PrepareForSave(_account);
        var saved = await _repository.AddAsync(prepared);
        _logger.LogInformation($"Account created: {saved.AccountId}");
        return new AuditResult(saved.AccountId, true);
    }
}
```

### DI Registration

Register all dependencies and the action in your DI setup:

```csharp
services.AddTransient<IAccountRepository, AccountRepository>();
services.AddTransient<AccountBusinessService>();
services.AddTransient<AuditAccountAction>();
```

### Factory Usage

When you call the factory, pass any non-service arguments (like the Account):

```csharp
var action = _actionFactory.Create<AuditAccountAction>(account);
```

The factory will resolve all registered services and inject them, plus any arguments you provide.

**Summary:**

- Just declare all needed services in your action's constructor.
- Register those services and the action in DI.
- The factory will handle the rest: all dependencies are injected automatically.

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
