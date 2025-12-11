# NRules Validation Architecture

## Overview

This document explains how NRules validation rules are located, loaded, compiled, evaluated, and how results are retrieved in the `CreateAccountAction`.

---

## Architecture Flow

```
1. Rule Definition (Design Time)
   ↓
2. Rule Location & Loading (Runtime)
   ↓
3. Rule Compilation
   ↓
4. Session Creation
   ↓
5. Fact Insertion
   ↓
6. Rule Evaluation (Fire)
   ↓
7. Result Retrieval
   ↓
8. Error Collection & Response
```

---

## Step-by-Step Process

### 1. Rule Definition (Design Time)

Rules are defined as classes inheriting from `NRules.Fluent.Dsl.Rule` in the `AccountBusiness.Rules` namespace.

```csharp
public class FirstNameRequiredRule : Rule
{
    public override void Define()
    {
        Account account = null!;

        When()
            .Match<Account>(() => account, a => string.IsNullOrWhiteSpace(a.FirstName));

        Then()
            .Do(ctx => ctx.Insert(new ValidationError("FirstName is required.")));
    }
}
```

**Key Components:**

- **When()** - Defines the condition to match (left-hand side)
- **Match<T>()** - Declares the fact type and matching predicate
- **Then()** - Defines the action when condition is true (right-hand side)
- **Do()** - Executes the action (inserts ValidationError)

### 2. Rule Location & Loading (Runtime)

When `Validate()` is called, NRules needs to find the rule classes.

```csharp
var repository = new RuleRepository();
repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
```

**How It Works:**

- `RuleRepository` - Container for rule definitions
- `Load()` - Scans for types that inherit from `Rule`
- `From(Assembly)` - Specifies which assembly to scan
- `typeof(FirstNameRequiredRule).Assembly` - Gets the assembly where rules are compiled

**What Gets Loaded:**
NRules scans the assembly and discovers all classes that:

1. Inherit from `NRules.Fluent.Dsl.Rule`
2. Have a `Define()` method
3. Are concrete (non-abstract) classes

In our case, it finds:

- `FirstNameRequiredRule`
- `FirstNameMaxLengthRule`
- `LastNameRequiredRule`
- `EmailAddressRequiredRule`
- `BirthDateRequiredRule`
- `MinimumAgeRule`
- `MaximumAgeRule`
- `PetCountNonNegativeRule`
- `PetCountMaximumRule`
- `CityMaxLengthRule`
- ... and more

**Alternative Loading Methods:**

```csharp
// Option 1: Load from current assembly
repository.Load(x => x.From(typeof(CreateAccountAction).Assembly));

// Option 2: Load specific rules
repository.Load(x => x
    .From(typeof(FirstNameRequiredRule))
    .From(typeof(EmailAddressRequiredRule)));

// Option 3: Load with namespace filter
repository.Load(x => x
    .From(Assembly.GetExecutingAssembly())
    .Where(t => t.Namespace == "AccountBusiness.Rules"));
```

### 3. Rule Compilation

After loading, rules must be compiled into an executable form.

```csharp
var factory = repository.Compile();
```

**What Happens:**

- NRules analyzes all loaded rules
- Converts `When()` conditions into a **RETE network** (efficient pattern matching algorithm)
- Optimizes rule execution order based on dependencies
- Creates a `ISessionFactory` that can produce sessions

**RETE Network:**
The RETE algorithm is a pattern matching algorithm for implementing rule-based systems. It builds a directed acyclic graph (DAG) of nodes where:

- **Alpha nodes** - Filter individual facts
- **Beta nodes** - Join multiple facts
- **Terminal nodes** - Execute actions

This makes evaluation very efficient, especially when facts don't change much between evaluations.

**Compilation Result:**

```
ISessionFactory - Reusable factory for creating sessions
```

### 4. Session Creation

A session represents a single execution context for evaluating rules.

```csharp
var session = factory.CreateSession();
```

**What Is a Session?**

- An instance of the rules engine
- Contains **working memory** (facts)
- Tracks which rules have fired
- Isolated from other sessions (thread-safe when used correctly)

**Session Lifecycle:**

```
Create Session → Insert Facts → Fire Rules → Query Results → Dispose
```

### 5. Fact Insertion

Facts are objects that rules operate on. We insert the `Account` object to be validated.

```csharp
session.Insert(_account);
```

**What Happens:**

1. The `Account` object is added to working memory
2. NRules propagates it through the RETE network
3. All alpha nodes (filters) evaluate the account
4. Matching conditions are activated
5. Rules that match are added to the **agenda** (ready to fire)

**Example Flow:**

```
Account { FirstName = "" } 
   ↓
FirstNameRequiredRule.When() evaluates: string.IsNullOrWhiteSpace("")
   ↓
Condition matches = TRUE
   ↓
Rule added to agenda
```

### 6. Rule Evaluation (Fire)

Firing executes all activated rules.

```csharp
session.Fire();
```

**What Happens:**

1. Rules in the agenda are executed in order
2. Each rule's `Then()` action is invoked
3. Actions can insert new facts (like `ValidationError`)
4. Newly inserted facts can activate more rules
5. Process continues until no more rules can fire

**Execution Order:**

- Rules with higher **salience** (priority) fire first
- Otherwise, order depends on RETE network structure
- All matching rules will eventually fire

**Example:**

```
Agenda:
  - FirstNameRequiredRule
  - EmailAddressRequiredRule
  - MinimumAgeRule

Fire:
  1. FirstNameRequiredRule executes
     → ctx.Insert(new ValidationError("FirstName is required."))
  
  2. EmailAddressRequiredRule executes
     → ctx.Insert(new ValidationError("Email address is required."))
  
  3. MinimumAgeRule executes
     → ctx.Insert(new ValidationError("Account holder must be at least 18 years old."))

Working Memory Now Contains:
  - Account { ... }
  - ValidationError { Message = "FirstName is required." }
  - ValidationError { Message = "Email address is required." }
  - ValidationError { Message = "Account holder must be at least 18 years old." }
```

### 7. Result Retrieval

After rules fire, we query the session for results.

```csharp
var errors = session.Query<ValidationError>().ToList();
```

**What Happens:**

- `Query<T>()` - Retrieves all facts of type `T` from working memory
- Returns `IEnumerable<ValidationError>`
- `.ToList()` - Materializes the query into a list

**Query Capabilities:**

```csharp
// Get all validation errors
var allErrors = session.Query<ValidationError>().ToList();

// Query with LINQ
var criticalErrors = session.Query<ValidationError>()
    .Where(e => e.Message.Contains("required"))
    .ToList();

// Count errors
var errorCount = session.Query<ValidationError>().Count();
```

### 8. Error Collection & Response

Finally, errors are processed and returned to the caller.

```csharp
foreach (var error in errors)
{
    AddValidationError(error.Message);
}

// If validation passes, set computed properties
if (ValidationErrors.Count == 0)
{
    Username = $"{_account.FirstName.Trim()} {_account.LastName.Trim()}".Trim();
    Email = _account.EmailAddress.Trim();
}

return Task.CompletedTask;
```

**Error Handling:**

- Each `ValidationError` is added to the `ValidationErrors` collection
- `BusinessActionBase.Validate()` checks `ValidationErrors.Count`
- If errors exist, throws `AggregateException` with all errors
- If no errors, action proceeds to `RunAsync()`

---

## Complete Code Example

```csharp
protected override Task Validate()
{
    // 1. Create repository
    var repository = new RuleRepository();
    
    // 2. Load rules from assembly
    repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
    
    // 3. Compile rules into session factory
    var factory = repository.Compile();
    
    // 4. Create session
    var session = factory.CreateSession();
    
    // 5. Insert fact (Account)
    session.Insert(_account);
    
    // 6. Fire all matching rules
    session.Fire();
    
    // 7. Retrieve results
    var errors = session.Query<ValidationError>().ToList();
    
    // 8. Collect errors
    foreach (var error in errors)
    {
        AddValidationError(error.Message);
    }
    
    // Set properties if validation passed
    if (ValidationErrors.Count == 0)
    {
        Username = $"{_account.FirstName.Trim()} {_account.LastName.Trim()}".Trim();
        Email = _account.EmailAddress.Trim();
    }
    
    return Task.CompletedTask;
}
```

---

## Performance Considerations

### Compilation Overhead

- **Problem:** Compiling rules on every validation is expensive
- **Solution:** Cache the `ISessionFactory` and reuse it

```csharp
// BAD - Compiles every time
protected override Task Validate()
{
    var repository = new RuleRepository();
    repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
    var factory = repository.Compile(); // EXPENSIVE!
    // ...
}

// GOOD - Compile once, reuse
private static readonly Lazy<ISessionFactory> _sessionFactory = new(() =>
{
    var repository = new RuleRepository();
    repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
    return repository.Compile();
});

protected override Task Validate()
{
    var session = _sessionFactory.Value.CreateSession(); // FAST!
    // ...
}
```

### Session Reuse

- **Don't reuse sessions** - They maintain state and aren't thread-safe
- **Do reuse the factory** - It's thread-safe and expensive to create

### Memory Management

- Sessions are disposable but don't implement `IDisposable`
- Let garbage collection handle cleanup
- For long-running processes, consider explicitly clearing session data

---

## Advanced Scenarios

### Conditional Rule Loading

```csharp
// Load only required rules
repository.Load(x => x
    .From(typeof(FirstNameRequiredRule))
    .From(typeof(EmailAddressRequiredRule)));

// Load rules by convention
repository.Load(x => x
    .From(Assembly.GetExecutingAssembly())
    .Where(t => t.Name.EndsWith("ValidationRule")));
```

### Rule Priorities (Salience)

```csharp
public class CriticalValidationRule : Rule
{
    public override void Define()
    {
        Priority(100); // Higher priority fires first
        
        Account account = null!;
        When()
            .Match<Account>(() => account, a => a.EmailAddress == null);
        Then()
            .Do(ctx => ctx.Insert(new ValidationError("CRITICAL: Email is null")));
    }
}
```

### Rule Groups

```csharp
// Group related rules
repository.Load(x => x
    .From(typeof(FirstNameRequiredRule))
    .From(typeof(LastNameRequiredRule))
    .WithTag("BasicValidation"));

// Fire only specific groups
session.Fire(x => x.WithTag("BasicValidation"));
```

### Multiple Fact Types

```csharp
// Insert multiple facts
session.Insert(_account);
session.Insert(new ValidationContext { IsStrictMode = true });

// Rules can match multiple facts
When()
    .Match<Account>(() => account)
    .Match<ValidationContext>(() => context, c => c.IsStrictMode)
Then()
    .Do(ctx => /* strict validation */);
```

---

## Troubleshooting

### Rules Not Firing

**Problem:** Rules don't execute even though conditions should match.

**Checklist:**

1. ✅ Rule class inherits from `Rule`
2. ✅ Rule has `Define()` method
3. ✅ Rule is in the scanned assembly
4. ✅ Conditions in `When()` are correct
5. ✅ `session.Fire()` is called
6. ✅ Fact types match (exact type, not derived)

### No Results Returned

**Problem:** `session.Query<T>()` returns empty.

**Possible Causes:**

- Rules didn't fire (check agenda with `session.Agenda`)
- Wrong type in query (check exact type name)
- Rules didn't insert expected facts
- Session was cleared or disposed

### Performance Issues

**Problem:** Validation is slow.

**Solutions:**

- Cache the `ISessionFactory`
- Reduce rule complexity
- Use fewer rules
- Profile with NRules diagnostics

---

## Summary

**NRules Validation Flow:**

1. **Define** - Write rules as classes with `When()` and `Then()`
2. **Load** - Scan assembly for rule classes
3. **Compile** - Build RETE network from rules
4. **Session** - Create execution context
5. **Insert** - Add facts to working memory
6. **Fire** - Execute matching rules
7. **Query** - Retrieve results from working memory
8. **Respond** - Process errors and continue or fail

**Key Benefits:**

- ✅ Declarative rule definitions
- ✅ Separation of validation logic from business logic
- ✅ Efficient pattern matching (RETE algorithm)
- ✅ Easy to add/remove rules
- ✅ Testable in isolation
- ✅ Reusable across actions

**Best Practices:**

- Cache the compiled `ISessionFactory`
- Don't reuse sessions
- Keep rules simple and focused
- Use descriptive rule names
- Document complex rule logic
- Test rules independently
