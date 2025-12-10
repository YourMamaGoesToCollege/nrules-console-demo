# Template Method Actions in .NET Business Layer

## Overview

This documentation introduces the template-method action pattern for business logic in .NET applications, focusing on the `CreateAccountAction` and its result type `CreatedAccount`. It explains the motivation, design, and practical benefits of using actions to encapsulate business operations, and demonstrates how this pattern improves separation of concerns, extensibility, and testability. The guide is intended for developers building layered .NET solutions who want to keep business logic clean, maintainable, and ready for future growth.

## Introduction

Modern .NET applications often require business logic that goes beyond simple CRUD operations. As systems grow, business processes may involve validation, normalization, side effects (such as sending emails or logging), and integration with external systems. The template-method action pattern provides a way to encapsulate these operations in dedicated classes, making your codebase more modular and easier to test.

This guide uses the example of account creation, showing how the `CreateAccountAction` and its result type `CreatedAccount` fit into a layered architecture. It covers:

- What the action/result types are
- Why they are useful
- How they fit into the business/service flow
- Practical design decisions and alternatives

## What is `CreatedAccount`?

```csharp
public record CreatedAccount(Guid Id, string Username, string Email, DateTime CreatedAt);
```

- `CreatedAccount` is a small immutable data type (C# record) used as the result type returned by the `CreateAccountAction`.
- It represents the outcome of an account-creation action: a generated Id, a username, the email used, and the time the action created the account.

## What is `CreateAccountAction`?

- `CreateAccountAction` is an implementation of the template-method pattern (inherits `BusinessAction<CreatedAccount>`).
- It:
  - Accepts an `Account` model in its constructor
  - Validates and normalizes input
  - Runs creation logic in `RunAsync()` (currently a placeholder)
  - Returns a `CreatedAccount` that the business layer then maps back to your domain `Account` for persistence

## Why Use Actions and Result Types?

### Separation of Concerns

- Keeps business orchestration (validation, normalization) separate from the steps needed to perform an operation (side effects, external calls).
- `AccountService` and `AccountBusinessService` remain thin and focused.

### Extensibility & Single-Responsibility

- Each action can encapsulate multiple responsibilities (DB insert, audit, events, external calls).
- Future changes (e.g., sending a welcome email) are isolated to the action.

### Testability

- Actions have clear input/output contracts (`Account` → `CreatedAccount`).
- You can unit-test actions in isolation and mock them in business/service tests.

### Pluggable Creation Strategy

- The factory (`IActionFactory`) resolves actions from DI or Activator.
- You can swap implementations or decorate actions at runtime.

### Clear Result Contract

- `CreatedAccount` is intentionally different from `Account` to keep boundaries clear.
- The business layer maps the result into the domain model for persistence.

## How Does It Fit in the Flow?

1. UI/console/tests call `AccountService.CreateAccountAsync(Account account)`.
2. `AccountService` delegates to `AccountBusinessService.CreateAccountAsync(account)`.
3. `AccountBusinessService`:
   - Validates & normalizes
   - Uses `IActionFactory.Create<CreateAccountAction>(account)`
   - Calls `action.ExecuteAsync()` → returns `CreatedAccount`
   - Maps `CreatedAccount` fields back onto the `Account` model
   - Calls `PrepareForSave(account)` and returns the prepared model
4. `AccountService` persists the prepared `Account` via repository

## Why Not Just Use `Account`?

- `CreatedAccount.Id` is a GUID (action-level id), while your DB `Account` has an integer `AccountId` identity.
- The action may compute creation metadata not yet persisted in your database model.
- Returning a small result object is cleaner than mutating the domain model inside the action.

## When You Might Not Need This Pattern

- If creation is a simple DB insert, the action/result abstraction may be overkill.
- You can remove the action and let the business service do validation + call repository.AddAsync directly.
- If you prefer the domain `Account` to be the only contract, modify the action to return an `Account` or skip the action pattern.

## Implementation Notes & Suggestions

- The current action `RunAsync` is a placeholder. Replace it with real logic when ready.
- Register actions in DI so dependencies can be injected.
- If the GUID in `CreatedAccount.Id` is your canonical id, add a corresponding field to `Account`.
- For richer name parsing, move username → first/last mapping into a helper/service.

## TL;DR

- `CreatedAccount` is the result of an account-creation action.
- `CreateAccountAction` encapsulates creation steps and returns that result.
- This pattern improves separation of concerns, testability, and extensibility.
- Not strictly required for simple flows, but valuable as business logic grows.

## What Next?

- Replace the placeholder `RunAsync` with real repository calls.
- Collapse the action pattern if you prefer a simpler flow.
- Add more actions for other business operations as needed.

---

### Full Example: `CreateAccountAction.cs`

```csharp
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
```
