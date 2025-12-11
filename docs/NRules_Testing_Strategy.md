# NRules Testing Strategy

## Overview

This document describes the testing strategy for NRules validation rules in the nrules-console-demo project. The strategy focuses on isolated unit testing of individual rules using the official NRules.Testing framework.

## Why Test NRules Rules in Isolation?

Testing NRules rules in isolation provides several key benefits:

1. **Precision**: Verify that each rule behaves correctly for its specific validation concern
2. **Clarity**: Understand exactly which rule is responsible for which validation error
3. **Maintainability**: Changes to one rule don't affect tests for other rules
4. **Debugging**: When a test fails, you immediately know which rule has the issue
5. **Documentation**: Tests serve as executable documentation of rule behavior
6. **Confidence**: Comprehensive coverage ensures rules work correctly before integration

## NRules.Testing Framework

The NRules.Testing framework provides the `RulesTestFixture` class, which enables isolated testing of rules without requiring the full application context.

### Key Components

- **RulesTestFixture**: Main test fixture that provides a repository and session
- **Repository**: Used to load rules for testing
- **Session**: In-memory session for inserting facts and firing rules
- **Query**: Mechanism to retrieve facts inserted by rules during execution

### Basic Pattern

```csharp
// 1. Create test fixture
var testTarget = new RulesTestFixture();

// 2. Load the specific rule(s) to test
testTarget.Repository.Load(x => x.From(typeof(RuleToTest)));

// 3. Create test data
var account = new Account { /* setup test data */ };

// 4. Insert facts into the session
testTarget.Session.Insert(account);

// 5. Fire all rules
testTarget.Session.Fire();

// 6. Query for results and assert
var errors = testTarget.Session.Query<ValidationError>().ToList();
errors.Should().HaveCount(1);
errors[0].Message.Should().Be("Expected error message");
```

## Test Structure and Organization

### Test Class Structure

Tests are organized in the `AccountValidationRulesTests` class with the following structure:

```csharp
public class AccountValidationRulesTests
{
    #region FirstName Tests
    // Tests for FirstNameRequiredRule
    // Tests for FirstNameMaxLengthRule
    #endregion

    #region LastName Tests
    // Tests for LastNameRequiredRule
    // Tests for LastNameMaxLengthRule
    #endregion

    // ... other rule groups ...

    #region Multiple Rules Integration Tests
    // Tests that load all rules together
    #endregion
}
```

### Test Naming Convention

Test names follow the pattern: `RuleName_WhenCondition_ShouldExpectedBehavior`

Examples:

- `FirstNameRequiredRule_WhenFirstNameIsNull_ShouldInsertValidationError`
- `FirstNameMaxLengthRule_WhenFirstNameIs100Characters_ShouldNotInsertValidationError`
- `EmailAddressFormatRule_WhenEmailHasNoAtSymbol_ShouldInsertValidationError`

This naming convention makes it immediately clear:

- Which rule is being tested
- What scenario/condition is being tested
- What the expected outcome should be

## Test Coverage Strategy

### 1. Required Field Rules

For required field rules (FirstName, LastName, EmailAddress, BirthDate):

- **Test null**: Field is null
- **Test empty**: Field is empty string
- **Test whitespace**: Field contains only whitespace
- **Test valid**: Field has valid value

Example:

```csharp
[Fact]
public void FirstNameRequiredRule_WhenFirstNameIsNull_ShouldInsertValidationError()
{
    var testTarget = new RulesTestFixture();
    testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule)));
    var account = new Account { FirstName = null, /* other required fields */ };
    testTarget.Session.Insert(account);
    testTarget.Session.Fire();
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    errors.Should().HaveCount(1);
    errors[0].Message.Should().Be("FirstName is required.");
}
```

### 2. Length Validation Rules

For maximum length rules (FirstName, LastName, EmailAddress, City):

- **Test at boundary**: Field length equals maximum (should pass)
- **Test over boundary**: Field length exceeds maximum by 1 (should fail)
- **Test null/empty**: Field is null or empty (should pass - not this rule's concern)

Example:

```csharp
[Fact]
public void FirstNameMaxLengthRule_WhenFirstNameIs100Characters_ShouldNotInsertValidationError()
{
    var testTarget = new RulesTestFixture();
    testTarget.Repository.Load(x => x.From(typeof(FirstNameMaxLengthRule)));
    var account = new Account { FirstName = new string('A', 100), /* other fields */ };
    testTarget.Session.Insert(account);
    testTarget.Session.Fire();
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    errors.Should().BeEmpty();
}
```

### 3. Format Validation Rules

For format rules (EmailAddress):

- **Test missing required components**: No @ symbol, no dot
- **Test valid format**: Proper email format
- **Test edge cases**: Special characters, multiple dots, etc.

Example:

```csharp
[Fact]
public void EmailAddressFormatRule_WhenEmailHasNoAtSymbol_ShouldInsertValidationError()
{
    var testTarget = new RulesTestFixture();
    testTarget.Repository.Load(x => x.From(typeof(EmailAddressFormatRule)));
    var account = new Account { EmailAddress = "johnexample.com", /* other fields */ };
    testTarget.Session.Insert(account);
    testTarget.Session.Fire();
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    errors.Should().HaveCount(1);
    errors[0].Message.Should().Be("Email address must be a valid email format.");
}
```

### 4. Range Validation Rules

For range rules (MinimumAge, MaximumAge, PetCount):

- **Test below minimum**: Value just below the minimum (should fail)
- **Test at minimum**: Value at the minimum boundary (should pass/fail based on inclusive/exclusive)
- **Test above minimum**: Value just above the minimum (should pass)
- **Test at maximum**: Value at the maximum boundary
- **Test above maximum**: Value just above the maximum

Example:

```csharp
[Fact]
public void MinimumAgeRule_WhenAge17_ShouldInsertValidationError()
{
    var testTarget = new RulesTestFixture();
    testTarget.Repository.Load(x => x.From(typeof(MinimumAgeRule)));
    var account = new Account 
    { 
        BirthDate = DateTime.UtcNow.AddYears(-17).AddDays(-1),
        /* other fields */ 
    };
    testTarget.Session.Insert(account);
    testTarget.Session.Fire();
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    errors.Should().HaveCount(1);
    errors[0].Message.Should().Be("Account holder must be at least 18 years old.");
}
```

### 5. Integration Tests

Test multiple rules together to ensure they work correctly in combination:

- **All valid**: All fields valid, no errors expected
- **Multiple invalid**: Multiple fields invalid, multiple errors expected

Example:

```csharp
[Fact]
public void MultipleRules_WhenAllFieldsValid_ShouldNotInsertAnyValidationErrors()
{
    var testTarget = new RulesTestFixture();
    testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
    var account = new Account 
    { 
        FirstName = "John",
        LastName = "Doe",
        EmailAddress = "john@example.com",
        BirthDate = DateTime.UtcNow.AddYears(-30),
        PetCount = 2,
        City = "New York"
    };
    testTarget.Session.Insert(account);
    testTarget.Session.Fire();
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    errors.Should().BeEmpty();
}
```

## Setup and Teardown

### Per-Test Setup

Each test creates its own `RulesTestFixture` instance. This ensures:

- Complete isolation between tests
- No shared state
- Tests can run in any order
- Parallel test execution is safe

No teardown is required as the fixture is garbage collected after each test.

### Loading Rules

**Single Rule Testing:**

```csharp
testTarget.Repository.Load(x => x.From(typeof(SpecificRule)));
```

**Multiple Rules Testing:**

```csharp
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
```

Loading from the assembly loads all rules in the `AccountBusiness.Rules.Validation` namespace.

## Assertion Strategies

### 1. Error Count Assertions

Always verify the exact number of errors expected:

```csharp
var errors = testTarget.Session.Query<ValidationError>().ToList();
errors.Should().HaveCount(1); // Exactly one error
errors.Should().BeEmpty(); // No errors
errors.Should().HaveCount(6); // Multiple specific errors
```

### 2. Error Message Assertions

Verify the exact error message to ensure the correct rule fired:

```csharp
errors[0].Message.Should().Be("FirstName is required.");
```

### 3. Multiple Error Assertions

For integration tests with multiple errors, use `Contain`:

```csharp
errors.Should().Contain(e => e.Message == "FirstName is required.");
errors.Should().Contain(e => e.Message == "LastName is required.");
errors.Should().Contain(e => e.Message == "Email address must be a valid email format.");
```

## Testing Multiple Rules Together vs. Isolation

### Isolated Rule Testing (Recommended for Unit Tests)

**When to use:**

- Testing a single rule's behavior
- Verifying boundary conditions
- Debugging a specific rule
- Initial development of a rule

**Advantages:**

- Fast execution
- Clear failure diagnosis
- Easy to maintain
- Precise control over what's being tested

**Example:**

```csharp
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule)));
```

### Multiple Rules Testing (Recommended for Integration Tests)

**When to use:**

- Verifying rules work together correctly
- Testing real-world scenarios
- Ensuring no rule conflicts
- Comprehensive validation testing

**Advantages:**

- Tests realistic scenarios
- Catches rule interaction issues
- Validates complete validation logic
- Mirrors production behavior

**Example:**

```csharp
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
```

## Best Practices

### 1. Test One Thing at a Time

Each isolated test should validate one specific rule behavior:

✅ Good:

```csharp
// Tests only FirstNameRequiredRule with null value
public void FirstNameRequiredRule_WhenFirstNameIsNull_ShouldInsertValidationError()
```

❌ Bad:

```csharp
// Tests multiple rules and multiple conditions
public void ValidationRules_WhenMultipleFieldsInvalid_ShouldFail()
```

### 2. Use Descriptive Test Names

Test names should clearly communicate:

- The rule being tested
- The test condition
- The expected outcome

### 3. Test Boundary Conditions

Always test at the boundaries of validation rules:

- Length: test at max length and max + 1
- Age: test at minimum age, minimum - 1, maximum age, maximum + 1
- Range: test at min, min - 1, max, max + 1

### 4. Keep Tests Independent

Each test should:

- Create its own fixture
- Not depend on other tests
- Be able to run in any order
- Be able to run in parallel

### 5. Use Meaningful Test Data

Use realistic test data that makes sense in context:

✅ Good:

```csharp
var account = new Account
{
    FirstName = "John",
    LastName = "Doe",
    EmailAddress = "john@example.com"
};
```

❌ Bad:

```csharp
var account = new Account
{
    FirstName = "x",
    LastName = "y",
    EmailAddress = "z@a.b"
};
```

### 6. Document Complex Test Logic

For tests with non-obvious setup (like age calculations), add comments:

```csharp
// Create email: 242 'a' chars + '@' + 13 chars 'example.com' = 256 total
var longEmail = new string('a', 242) + "@example.com";
```

## Common Pitfalls and Solutions

### Pitfall 1: Not Loading Rules

**Problem:**

```csharp
var testTarget = new RulesTestFixture();
// Forgot to load rules!
testTarget.Session.Insert(account);
testTarget.Session.Fire();
```

**Solution:**
Always load rules before firing:

```csharp
var testTarget = new RulesTestFixture();
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule)));
testTarget.Session.Insert(account);
testTarget.Session.Fire();
```

### Pitfall 2: Incorrect Boundary Testing

**Problem:**

```csharp
// Testing 100 chars when rule allows <= 100 (should pass, not fail)
public void FirstNameMaxLengthRule_WhenFirstNameIs100Characters_ShouldInsertValidationError()
```

**Solution:**
Test both sides of the boundary:

- At boundary: should pass
- Just over boundary: should fail

### Pitfall 3: Not Querying for Results

**Problem:**

```csharp
testTarget.Session.Fire();
// Forgot to query for ValidationError results!
// No assertions made
```

**Solution:**
Always query and assert:

```csharp
testTarget.Session.Fire();
var errors = testTarget.Session.Query<ValidationError>().ToList();
errors.Should().HaveCount(1);
```

### Pitfall 4: Incomplete Test Coverage

**Problem:**
Only testing the negative case (when rule fails), not the positive case (when rule passes).

**Solution:**
Always test both scenarios:

```csharp
// Negative: should insert error
FirstNameRequiredRule_WhenFirstNameIsNull_ShouldInsertValidationError()

// Positive: should NOT insert error
FirstNameRequiredRule_WhenFirstNameIsValid_ShouldNotInsertValidationError()
```

### Pitfall 5: Loading All Rules When Testing One

**Problem:**

```csharp
// Loading all rules when only testing one
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule).Assembly));
// But only asserting on FirstNameRequiredRule behavior
```

**Solution:**
For isolated tests, load only the specific rule:

```csharp
testTarget.Repository.Load(x => x.From(typeof(FirstNameRequiredRule)));
```

## Example Test Walkthrough

Let's walk through a complete test example:

```csharp
[Fact]
public void EmailAddressFormatRule_WhenEmailHasNoAtSymbol_ShouldInsertValidationError()
{
    // ARRANGE
    // 1. Create the test fixture - provides isolated NRules environment
    var testTarget = new RulesTestFixture();
    
    // 2. Load only the EmailAddressFormatRule for isolation
    testTarget.Repository.Load(x => x.From(typeof(EmailAddressFormatRule)));
    
    // 3. Create an Account with an invalid email (no @ symbol)
    var account = new Account
    {
        FirstName = "John",           // Valid - not tested by this rule
        LastName = "Doe",             // Valid - not tested by this rule
        EmailAddress = "johnexample.com", // INVALID - missing @ symbol
        BirthDate = DateTime.UtcNow.AddYears(-30) // Valid - not tested by this rule
    };

    // ACT
    // 4. Insert the account into the session (working memory)
    testTarget.Session.Insert(account);
    
    // 5. Fire all loaded rules (in this case, just EmailAddressFormatRule)
    testTarget.Session.Fire();

    // ASSERT
    // 6. Query the session for ValidationError facts inserted by rules
    var errors = testTarget.Session.Query<ValidationError>().ToList();
    
    // 7. Verify exactly one error was inserted
    errors.Should().HaveCount(1);
    
    // 8. Verify the error message is correct
    errors[0].Message.Should().Be("Email address must be a valid email format.");
}
```

**What happens during this test:**

1. **Fixture Creation**: Creates an isolated NRules environment with its own repository and session
2. **Rule Loading**: Loads only the `EmailAddressFormatRule` from the assembly
3. **Fact Creation**: Creates an Account with an invalid email address (missing @)
4. **Fact Insertion**: Inserts the account into the working memory (session)
5. **Rule Execution**: Fires all loaded rules - the EmailAddressFormatRule evaluates the condition
6. **Rule Match**: The rule condition matches (email has no @), so the "Then" action executes
7. **Error Insertion**: The rule inserts a ValidationError fact into the session
8. **Result Query**: Queries the session for all ValidationError facts
9. **Assertions**: Verifies exactly one error exists with the expected message

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Only NRules Tests

```bash
dotnet test --filter "FullyQualifiedName~AccountValidationRulesTests"
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~FirstNameRequiredRule_WhenFirstNameIsNull"
```

### Run Tests with Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Continuous Integration

These tests should run:

- On every commit
- Before every pull request merge
- As part of the CI/CD pipeline

The tests are fast (isolated, in-memory) and reliable (no external dependencies), making them ideal for CI environments.

## References

- [NRules Getting Started Guide](https://nrules.net/articles/getting-started.html)
- [NRules.Testing API Documentation](https://nrules.net/api/NRules.Testing.RulesTestFixture.html)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)

## Conclusion

This testing strategy ensures that NRules validation rules are thoroughly tested in isolation, providing confidence that each rule behaves correctly. By following the patterns and best practices outlined in this document, you can maintain high-quality validation logic that is easy to test, understand, and maintain.

The combination of:

- Isolated unit tests for individual rules
- Integration tests for rule combinations
- Comprehensive boundary testing
- Clear naming conventions
- Detailed assertions

provides a robust testing foundation for the NRules validation framework in this project.
