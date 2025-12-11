# Business Action Architecture Guide

## Table of Contents

1. [Overview](#overview)
2. [Why Template Method Pattern?](#why-template-method-pattern)
3. [Architecture Layers](#architecture-layers)
4. [Class Hierarchy](#class-hierarchy)
5. [Complete Code Examples](#complete-code-examples)
6. [Use Cases](#use-cases)
7. [Best Practices](#best-practices)
8. [Testing Strategies](#testing-strategies)

---

## Overview

The Business Action architecture provides a robust, extensible framework for implementing business operations with consistent structure, validation, and audit logging. It leverages the **Template Method Pattern** to define a reusable algorithm while allowing customization at specific steps.

### Key Components

```
BusinessAction<T>              (Abstract Template)
    ↓
BusinessActionBase<T>          (Base Implementation)
    ↓
CreateAccountAction            (Concrete Action)
UpdateAccountAction            (Concrete Action)
DeleteAccountAction            (Concrete Action)
```

### Merits of This Architecture

✅ **Consistency** - All business actions follow the same execution flow  
✅ **Reusability** - Common functionality (validation, audit) implemented once  
✅ **Testability** - Each layer can be tested independently  
✅ **Maintainability** - Clear separation of concerns  
✅ **Extensibility** - Easy to add new actions or modify behavior  
✅ **Type Safety** - Generic `<T>` ensures compile-time type checking  
✅ **SOLID Principles** - Open/Closed, Single Responsibility, Dependency Inversion

---

## Why Template Method Pattern?

The Template Method Pattern defines the skeleton of an algorithm in a base class, letting subclasses override specific steps without changing the algorithm's structure.

### Problem It Solves

**Without Template Method:**

```csharp
// Each action implements validation differently
public class CreateAccountAction
{
    public async Task<Account> Execute()
    {
        // Validation logic here (duplicated)
        if (string.IsNullOrEmpty(Username)) throw...
        if (string.IsNullOrEmpty(Email)) throw...
        
        // Business logic
        var account = await CreateAccount();
        
        // Audit logging here (duplicated)
        Console.WriteLine($"Created: {account.Id}");
        
        return account;
    }
}

public class UpdateAccountAction
{
    public async Task<Account> Execute()
    {
        // Same validation logic duplicated!
        if (string.IsNullOrEmpty(Username)) throw...
        if (string.IsNullOrEmpty(Email)) throw...
        
        // Business logic
        var account = await UpdateAccount();
        
        // Same audit logging duplicated!
        Console.WriteLine($"Updated: {account.Id}");
        
        return account;
    }
}
```

**Problems:**

- ❌ Code duplication across all actions
- ❌ Inconsistent validation approaches
- ❌ Hard to add cross-cutting concerns (logging, metrics)
- ❌ Difficult to enforce company-wide standards
- ❌ Testing requires duplicating setup/teardown

**With Template Method:**

```csharp
// Base defines the algorithm structure
public abstract class BusinessAction<T>
{
    // Template method - defines the algorithm
    public async Task<T> ExecuteAsync()
    {
        await PreExecuteAsync();      // Step 1: Prepare
        var result = await RunAsync(); // Step 2: Execute
        await PostExecuteAsync(result);// Step 3: Finalize
        return result;
    }
    
    // Subclasses implement only what varies
    protected abstract Task<T> RunAsync();
}
```

**Benefits:**

- ✅ Single place to define execution flow
- ✅ Consistent behavior across all actions
- ✅ Easy to add features (metrics, caching, transactions)
- ✅ Subclasses focus only on unique logic

---

## Architecture Layers

### Layer 1: BusinessAction<T> (Abstract Template)

**Purpose:** Define the execution algorithm and extension points.

```csharp
public abstract class BusinessAction<T>
{
    // Template method - DO NOT OVERRIDE
    public async Task<T> ExecuteAsync()
    {
        await PreExecuteAsync();
        var result = await RunAsync();
        await PostExecuteAsync(result);
        return result;
    }

    // Hook: Called before execution
    protected virtual async Task PreExecuteAsync()
    {
        await Validate();
    }

    // Hook: Validation logic
    protected virtual Task Validate()
    {
        return Task.CompletedTask;
    }

    // Core logic - MUST IMPLEMENT
    protected abstract Task<T> RunAsync();

    // Hook: Called after execution
    protected virtual async Task PostExecuteAsync(T result)
    {
        await AuditLog(result);
    }

    // Hook: Audit logging
    protected virtual Task AuditLog(T result)
    {
        return Task.CompletedTask;
    }
}
```

**Key Points:**

- `ExecuteAsync()` is **sealed by convention** - defines the algorithm
- Virtual methods (`PreExecuteAsync`, `PostExecuteAsync`) provide extension points
- Abstract method (`RunAsync()`) forces implementation
- Default implementations use `Task.CompletedTask` (singleton, efficient)

### Layer 2: BusinessActionBase<T> (Base Implementation)

**Purpose:** Provide common implementations for domain actions.

```csharp
public abstract class BusinessActionBase<T> : BusinessAction<T>
{
    protected List<string> ValidationErrors { get; } = new();

    // Implement validation with error collection
    protected override async Task Validate()
    {
        ValidationErrors.Clear();
        await ValidateInputs();

        if (ValidationErrors.Count > 0)
        {
            var exceptions = ValidationErrors
                .Select(e => new ArgumentException(e))
                .ToList();
            throw new AggregateException("Validation failed", exceptions);
        }
    }

    // Subclasses implement specific validation
    protected virtual Task ValidateInputs()
    {
        return Task.CompletedTask;
    }

    // Implement audit logging with context
    protected override async Task AuditLog(T result)
    {
        var context = await GetAuditContext(result);
        var actionName = GetType().Name;

        Console.WriteLine($"[AUDIT] Action: {actionName}");
        Console.WriteLine($"[AUDIT] Result Type: {result?.GetType().Name}");
        Console.WriteLine($"[AUDIT] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        
        foreach (var kvp in context)
        {
            Console.WriteLine($"[AUDIT]   {kvp.Key}: {kvp.Value}");
        }
    }

    // Subclasses provide audit context
    protected virtual Task<Dictionary<string, object>> GetAuditContext(T result)
    {
        return Task.FromResult(new Dictionary<string, object>());
    }

    // Helper methods for common validations
    protected void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            AddValidationError($"{fieldName} is required.");
    }

    protected void ValidateEmail(string email, string fieldName = "Email")
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddValidationError($"{fieldName} is required.");
            return;
        }
        if (!email.Contains("@") || !email.Contains("."))
            AddValidationError($"{fieldName} must be a valid email address.");
    }

    protected void ValidateMinLength(string value, int minLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            AddValidationError($"{fieldName} is required.");
            return;
        }
        if (value.Length < minLength)
            AddValidationError($"{fieldName} must be at least {minLength} characters long.");
    }

    protected void AddValidationError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
            ValidationErrors.Add(error);
    }
}
```

**Key Points:**

- Implements validation with **error collection** (allows multiple errors)
- Provides **helper methods** for common validation patterns
- Implements structured audit logging
- Throws `AggregateException` for multiple validation errors
- Subclasses override `ValidateInputs()` and `GetAuditContext()`

### Layer 3: Concrete Actions

**Purpose:** Implement specific business operations.

```csharp
public class CreateAccountAction : BusinessActionBase<CreatedAccount>
{
    public string Username { get; }
    public string Email { get; }
    private readonly Account _account;

    public CreateAccountAction(Account account)
    {
        if (account == null) 
            throw new ArgumentNullException(nameof(account));

        _account = account;
        
        var first = account.FirstName?.Trim();
        var last = account.LastName?.Trim();

        Username = string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last)
            ? throw new ArgumentException("FirstName or LastName is required")
            : $"{first} {last}".Trim();

        Email = account.EmailAddress?.Trim() 
            ?? throw new ArgumentNullException(nameof(account.EmailAddress));
    }

    // Implement validation
    protected override Task ValidateInputs()
    {
        ValidateRequired(Username, nameof(Username));
        ValidateEmail(Email, nameof(Email));
        return Task.CompletedTask;
    }

    // Implement core business logic
    protected override async Task<CreatedAccount> RunAsync()
    {
        // Simulate database operation
        await Task.Delay(10);

        return new CreatedAccount(
            Id: Guid.NewGuid(),
            Username: Username,
            Email: Email,
            CreatedAt: DateTime.UtcNow
        );
    }

    // Implement audit context
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
}
```

---

## Complete Code Examples

### Example 1: Create Account Action

```csharp
// Define the result record
public record CreatedAccount(Guid Id, string Username, string Email, DateTime CreatedAt);

// Implement the action
public class CreateAccountAction : BusinessActionBase<CreatedAccount>
{
    public string Username { get; }
    public string Email { get; }

    public CreateAccountAction(string username, string email)
    {
        Username = username;
        Email = email;
    }

    protected override Task ValidateInputs()
    {
        ValidateRequired(Username, nameof(Username));
        ValidateEmail(Email, nameof(Email));
        ValidateMinLength(Username, 3, nameof(Username));
        return Task.CompletedTask;
    }

    protected override async Task<CreatedAccount> RunAsync()
    {
        // Simulate database save
        await Task.Delay(50);
        
        return new CreatedAccount(
            Id: Guid.NewGuid(),
            Username: Username,
            Email: Email,
            CreatedAt: DateTime.UtcNow
        );
    }

    protected override Task<Dictionary<string, object>> GetAuditContext(CreatedAccount result)
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            { "AccountId", result.Id },
            { "Username", result.Username },
            { "Email", result.Email }
        });
    }
}

// Usage
var action = new CreateAccountAction("john.doe", "john@example.com");
var result = await action.ExecuteAsync();
Console.WriteLine($"Account created: {result.Id}");
```

**Output:**

```
[AUDIT] Action: CreateAccountAction
[AUDIT] Result Type: CreatedAccount
[AUDIT] Timestamp: 2025-12-10 21:22:32.996 UTC
[AUDIT] Context:
[AUDIT]   AccountId: 79bcc0a5-116a-4ae7-9acd-be9925224562
[AUDIT]   Username: john.doe
[AUDIT]   Email: john@example.com
Account created: 79bcc0a5-116a-4ae7-9acd-be9925224562
```

### Example 2: Update Account Action

```csharp
public record UpdatedAccount(Guid Id, string Username, string Email, DateTime UpdatedAt);

public class UpdateAccountAction : BusinessActionBase<UpdatedAccount>
{
    private readonly Guid _accountId;
    public string NewEmail { get; }

    public UpdateAccountAction(Guid accountId, string newEmail)
    {
        _accountId = accountId;
        NewEmail = newEmail;
    }

    protected override Task ValidateInputs()
    {
        if (_accountId == Guid.Empty)
            AddValidationError("AccountId cannot be empty.");
        
        ValidateEmail(NewEmail, nameof(NewEmail));
        return Task.CompletedTask;
    }

    protected override async Task<UpdatedAccount> RunAsync()
    {
        // Simulate fetching and updating account
        await Task.Delay(50);
        
        return new UpdatedAccount(
            Id: _accountId,
            Username: "john.doe", // Would fetch from DB
            Email: NewEmail,
            UpdatedAt: DateTime.UtcNow
        );
    }

    protected override Task<Dictionary<string, object>> GetAuditContext(UpdatedAccount result)
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            { "AccountId", result.Id },
            { "OldEmail", "old@example.com" }, // Would fetch from DB
            { "NewEmail", result.Email },
            { "UpdatedAt", result.UpdatedAt }
        });
    }
}

// Usage
var updateAction = new UpdateAccountAction(
    accountId: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    newEmail: "newemail@example.com"
);
var result = await updateAction.ExecuteAsync();
```

### Example 3: Complex Validation with Multiple Errors

```csharp
public record RegisteredUser(
    Guid Id, 
    string Username, 
    string Email, 
    string PhoneNumber, 
    DateTime RegisteredAt
);

public class RegisterUserAction : BusinessActionBase<RegisteredUser>
{
    public string Username { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public string Password { get; }
    public int Age { get; }

    public RegisterUserAction(
        string username, 
        string email, 
        string phoneNumber, 
        string password, 
        int age)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
        Password = password;
        Age = age;
    }

    protected override Task ValidateInputs()
    {
        // Username validation
        ValidateRequired(Username, nameof(Username));
        ValidateMinLength(Username, 3, nameof(Username));
        if (Username?.Length > 50)
            AddValidationError("Username must be 50 characters or less.");

        // Email validation
        ValidateEmail(Email, nameof(Email));

        // Phone validation
        ValidateRequired(PhoneNumber, nameof(PhoneNumber));
        if (!PhoneNumber?.StartsWith("+") ?? true)
            AddValidationError("Phone number must start with country code (+).");

        // Password validation
        ValidateRequired(Password, nameof(Password));
        ValidateMinLength(Password, 8, nameof(Password));
        if (!HasUpperCase(Password))
            AddValidationError("Password must contain at least one uppercase letter.");
        if (!HasNumber(Password))
            AddValidationError("Password must contain at least one number.");

        // Age validation
        if (Age < 18)
            AddValidationError("User must be at least 18 years old.");
        if (Age > 120)
            AddValidationError("Age must be realistic.");

        return Task.CompletedTask;
    }

    protected override async Task<RegisteredUser> RunAsync()
    {
        // Hash password, save to database, etc.
        await Task.Delay(100);
        
        return new RegisteredUser(
            Id: Guid.NewGuid(),
            Username: Username,
            Email: Email,
            PhoneNumber: PhoneNumber,
            RegisteredAt: DateTime.UtcNow
        );
    }

    protected override Task<Dictionary<string, object>> GetAuditContext(RegisteredUser result)
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            { "UserId", result.Id },
            { "Username", result.Username },
            { "Email", result.Email },
            { "PhoneNumber", result.PhoneNumber },
            { "Age", Age },
            { "RegistrationMethod", "Web" }
        });
    }

    private bool HasUpperCase(string value) => 
        value?.Any(char.IsUpper) ?? false;
    
    private bool HasNumber(string value) => 
        value?.Any(char.IsDigit) ?? false;
}

// Usage with error handling
try
{
    var action = new RegisterUserAction(
        username: "ab",           // Too short
        email: "invalid-email",   // Invalid format
        phoneNumber: "1234567",   // Missing country code
        password: "weak",         // Too short, no uppercase, no number
        age: 15                   // Too young
    );
    
    var result = await action.ExecuteAsync();
}
catch (AggregateException ex)
{
    Console.WriteLine("Validation failed with errors:");
    foreach (var inner in ex.InnerExceptions)
    {
        Console.WriteLine($"  - {inner.Message}");
    }
}
```

**Output:**

```
Validation failed with errors:
  - Username must be at least 3 characters long.
  - Email must be a valid email address.
  - Phone number must start with country code (+).
  - Password must be at least 8 characters long.
  - Password must contain at least one uppercase letter.
  - Password must contain at least one number.
  - User must be at least 18 years old.
```

### Example 4: Action with Database Transaction

```csharp
public class TransferMoneyAction : BusinessActionBase<TransferResult>
{
    private readonly IAccountRepository _repository;
    private readonly Guid _fromAccountId;
    private readonly Guid _toAccountId;
    private readonly decimal _amount;

    public TransferMoneyAction(
        IAccountRepository repository,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount)
    {
        _repository = repository;
        _fromAccountId = fromAccountId;
        _toAccountId = toAccountId;
        _amount = amount;
    }

    protected override Task ValidateInputs()
    {
        if (_fromAccountId == Guid.Empty)
            AddValidationError("From account is required.");
        if (_toAccountId == Guid.Empty)
            AddValidationError("To account is required.");
        if (_fromAccountId == _toAccountId)
            AddValidationError("Cannot transfer to the same account.");
        if (_amount <= 0)
            AddValidationError("Amount must be greater than zero.");
        if (_amount > 1000000)
            AddValidationError("Amount exceeds maximum transfer limit.");

        return Task.CompletedTask;
    }

    protected override async Task<TransferResult> RunAsync()
    {
        using var transaction = await _repository.BeginTransactionAsync();
        
        try
        {
            // Debit from account
            var fromAccount = await _repository.GetByIdAsync(_fromAccountId);
            if (fromAccount.Balance < _amount)
                throw new InvalidOperationException("Insufficient funds.");
            
            fromAccount.Balance -= _amount;
            await _repository.UpdateAsync(fromAccount);

            // Credit to account
            var toAccount = await _repository.GetByIdAsync(_toAccountId);
            toAccount.Balance += _amount;
            await _repository.UpdateAsync(toAccount);

            await transaction.CommitAsync();

            return new TransferResult(
                TransferId: Guid.NewGuid(),
                FromAccountId: _fromAccountId,
                ToAccountId: _toAccountId,
                Amount: _amount,
                TransferredAt: DateTime.UtcNow
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    protected override Task<Dictionary<string, object>> GetAuditContext(TransferResult result)
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            { "TransferId", result.TransferId },
            { "FromAccountId", result.FromAccountId },
            { "ToAccountId", result.ToAccountId },
            { "Amount", result.Amount },
            { "Currency", "USD" }
        });
    }
}
```

---

## Use Cases

### 1. CRUD Operations

- `CreateAccountAction`, `ReadAccountAction`, `UpdateAccountAction`, `DeleteAccountAction`
- Consistent validation and audit across all operations

### 2. Financial Transactions

- `TransferMoneyAction`, `WithdrawFundsAction`, `DepositFundsAction`
- Built-in validation, transaction support, audit trail

### 3. User Authentication

- `LoginAction`, `RegisterAction`, `ChangePasswordAction`, `ResetPasswordAction`
- Security validation, audit logging for compliance

### 4. Business Workflows

- `ApproveOrderAction`, `ProcessPaymentAction`, `ShipOrderAction`
- Step-by-step validation, state transitions, audit trail

### 5. Integration Operations

- `SyncCustomerDataAction`, `ImportOrdersAction`, `ExportReportsAction`
- Error handling, retry logic, audit logging

### 6. Batch Processing

- `ProcessBulkPaymentsAction`, `GenerateMonthlyReportsAction`
- Progress tracking, error collection, comprehensive audit

---

## Best Practices

### ✅ DO

1. **Keep RunAsync() focused on business logic**

   ```csharp
   protected override async Task<Result> RunAsync()
   {
       // Only business logic here
       var entity = await _repository.CreateAsync(data);
       await _notificationService.NotifyAsync(entity);
       return entity;
   }
   ```

2. **Use descriptive validation messages**

   ```csharp
   ValidateRequired(Email, "Customer email address");
   AddValidationError("Order total must be between $1 and $10,000");
   ```

3. **Provide rich audit context**

   ```csharp
   protected override Task<Dictionary<string, object>> GetAuditContext(Result result)
   {
       return Task.FromResult(new Dictionary<string, object>
       {
           { "UserId", _currentUser.Id },
           { "IpAddress", _httpContext.Connection.RemoteIpAddress },
           { "UserAgent", _httpContext.Request.Headers["User-Agent"] },
           { "ResultId", result.Id }
       });
   }
   ```

4. **Use records for result types**

   ```csharp
   public record CreatedAccount(Guid Id, string Username, string Email, DateTime CreatedAt);
   ```

5. **Inject dependencies via constructor**

   ```csharp
   public CreateAccountAction(IAccountRepository repository, IEmailService emailService)
   {
       _repository = repository;
       _emailService = emailService;
   }
   ```

### ❌ DON'T

1. **Don't put business logic in ValidateInputs()**

   ```csharp
   // ❌ BAD
   protected override Task ValidateInputs()
   {
       var account = await _repository.GetByIdAsync(id); // Business logic!
       return Task.CompletedTask;
   }
   ```

2. **Don't throw exceptions in GetAuditContext()**

   ```csharp
   // ❌ BAD
   protected override Task<Dictionary<string, object>> GetAuditContext(Result result)
   {
       if (result == null) throw new ArgumentNullException(); // Will crash audit!
       // Handle gracefully instead
   }
   ```

3. **Don't override ExecuteAsync()**

   ```csharp
   // ❌ BAD - breaks template method pattern
   public override async Task<Result> ExecuteAsync()
   {
       // Custom logic that bypasses validation/audit
   }
   ```

4. **Don't add state that changes during execution**

   ```csharp
   // ❌ BAD
   private int _retryCount; // Mutable state
   
   // ✅ GOOD
   private readonly int _maxRetries; // Immutable configuration
   ```

---

## Testing Strategies

### Unit Testing Concrete Actions

```csharp
public class CreateAccountActionTests
{
    [Fact]
    public async Task ExecuteAsync_ValidInput_CreatesAccount()
    {
        // Arrange
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john@example.com"
        };
        var action = new CreateAccountAction(account);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("John Doe", result.Username);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidEmail_ThrowsAggregateException()
    {
        // Arrange
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "invalid-email" // Invalid format
        };
        var action = new CreateAccountAction(account);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => action.ExecuteAsync()
        );
        
        Assert.Contains("valid email", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MissingUsername_ThrowsAggregateException()
    {
        // Arrange
        var account = new Account
        {
            FirstName = "",
            LastName = "",
            EmailAddress = "john@example.com"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new CreateAccountAction(account)
        );
        
        Assert.Contains("FirstName or LastName is required", exception.Message);
    }
}
```

### Integration Testing with Dependencies

```csharp
public class TransferMoneyActionIntegrationTests : IAsyncLifetime
{
    private readonly IAccountRepository _repository;
    private Guid _fromAccountId;
    private Guid _toAccountId;

    public TransferMoneyActionIntegrationTests()
    {
        // Use in-memory database for testing
        _repository = new InMemoryAccountRepository();
    }

    public async Task InitializeAsync()
    {
        // Setup test accounts
        _fromAccountId = await _repository.CreateAsync(new Account
        {
            Balance = 1000m
        });
        
        _toAccountId = await _repository.CreateAsync(new Account
        {
            Balance = 500m
        });
    }

    [Fact]
    public async Task TransferMoney_ValidTransfer_UpdatesBalances()
    {
        // Arrange
        var action = new TransferMoneyAction(
            _repository,
            _fromAccountId,
            _toAccountId,
            100m
        );

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        var fromAccount = await _repository.GetByIdAsync(_fromAccountId);
        var toAccount = await _repository.GetByIdAsync(_toAccountId);
        
        Assert.Equal(900m, fromAccount.Balance);
        Assert.Equal(600m, toAccount.Balance);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

### Testing Custom Validation

```csharp
public class ValidationTestAction : BusinessActionBase<string>
{
    public string TestValue { get; set; }

    protected override Task ValidateInputs()
    {
        ValidateRequired(TestValue, nameof(TestValue));
        return Task.CompletedTask;
    }

    protected override Task<string> RunAsync()
    {
        return Task.FromResult($"Processed: {TestValue}");
    }
}

public class ValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateInputs_EmptyValue_ThrowsException(string value)
    {
        // Arrange
        var action = new ValidationTestAction { TestValue = value };

        // Act & Assert
        await Assert.ThrowsAsync<AggregateException>(() => action.ExecuteAsync());
    }

    [Fact]
    public async Task ValidateInputs_ValidValue_Succeeds()
    {
        // Arrange
        var action = new ValidationTestAction { TestValue = "test" };

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        Assert.Equal("Processed: test", result);
    }
}
```

---

## Conclusion

The Business Action architecture provides a powerful, maintainable framework for implementing business operations. By leveraging the Template Method Pattern and layered abstractions, it delivers:

- **Consistency** across all business operations
- **Reusability** of common functionality
- **Testability** at every layer
- **Extensibility** for future requirements
- **Type Safety** through generics
- **Best Practices** enforcement through structure

This architecture scales from simple CRUD operations to complex business workflows while maintaining clean, testable code that follows SOLID principles.
