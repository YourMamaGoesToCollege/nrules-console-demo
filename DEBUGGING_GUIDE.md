# VS Code C# Debugging Guide

## Launch Configurations Available

Your workspace now has 4 debug configurations in `.vscode/launch.json`:

### 1. **Debug Tests (AccountService.Tests)**
- Builds and debugs the test project directly
- Debugger stops at breakpoints in test code
- Console: Internal (shows debug output in VS Code)
- **Use when:** You want to step through test logic and inspect variables

### 2. **Debug Console App (nrules-console)**
- Builds and debugs the console application
- Console: Integrated Terminal (shows app output in VS Code terminal)
- **Use when:** You want to debug the main application entry point

### 3. **Run Tests (No Debug)**
- Builds and runs tests without debugger attached
- Console: External Terminal (shows in system terminal)
- **Use when:** You want to run tests and see output, but don't need line-by-line debugging

### 4. **Attach to Process**
- Attaches debugger to a running process
- **Use when:** You have a process already running and want to debug it

---

## How to Debug

### Method 1: From Debug View (Easiest)
1. Press `Shift+Cmd+D` (Mac) or `Ctrl+Shift+D` (Windows/Linux)
2. Click the dropdown at the top of the Run & Debug panel
3. Select a configuration (e.g., "Debug Tests (AccountService.Tests)")
4. Click the green play button or press `F5`

### Method 2: From Command Palette
1. Press `Cmd+Shift+P` (Mac) or `Ctrl+Shift+P` (Windows/Linux)
2. Type "Debug:" 
3. Select "Debug: Start Debugging"
4. Choose a configuration from the dropdown

### Method 3: One-Click Test Debug
1. Open any test file (e.g., `AccountService.Tests/AccountServiceIntegrationTests.cs`)
2. You should see "Debug" links above `[Fact]` methods
3. Click "Debug" and it runs that specific test in debug mode

---

## Setting Breakpoints

### Line Breakpoint
- Click in the left margin (line number area) → red dot appears
- Debugger stops on that line before executing

### Conditional Breakpoint
- Right-click line number → "Add Conditional Breakpoint"
- Enter condition (e.g., `result.AccountId > 5`)
- Only breaks when condition is true

### Logpoint (No breakpoint)
- Right-click line number → "Add Logpoint"
- Enter log message (e.g., `FirstName: {account.FirstName}`)
- Logs without pausing execution

---

## Debugging Controls

| Action | Mac | Windows/Linux | Button |
|--------|-----|---------------|--------|
| **Continue** | F5 | F5 | ▶️ |
| **Step Over** | F10 | F10 | ⤵️ |
| **Step Into** | F11 | F11 | ⬇️ |
| **Step Out** | Shift+F11 | Shift+F11 | ⬆️ |
| **Restart** | Cmd+Shift+F5 | Ctrl+Shift+F5 | 🔄 |
| **Stop** | Shift+F5 | Shift+F5 | ⏹️ |

---

## Inspecting Variables

### Hover Over Variables
- Hover your mouse over any variable name while debugging
- A popup shows the current value

### Watch Variables
1. Open **Run & Debug** view (Shift+Cmd+D)
2. Scroll to **WATCH** section
3. Click **+** to add a variable
4. Type variable name (e.g., `result.FirstName`)
5. Value updates as you step through code

### Debug Console
1. Open **View → Debug Console** or press `Shift+Cmd+Y`
2. Type expressions and press Enter:
   ```
   result.AccountId
   account.EmailAddress
   result == null
   ```

---

## Example: Debug a Test

```csharp
[Fact]
public async Task CreateAccountAsync_WithValidData_ReturnsAccountWithId()
{
    var dbContext = CreateInMemoryDbContext();
    var repository = new AccountRepository.AccountRepository(dbContext);
    var service = new AccountSvc(repository);

    var firstName = "John";          // ← Set breakpoint here
    var lastName = "Doe";
    var birthDate = new DateTime(1990, 1, 15);
    var email = "john.doe@example.com";

    var result = await service.CreateAccountAsync(
        firstName, lastName, birthDate, email);
    
    // ← Set another breakpoint here
    
    result.Should().NotBeNull();
    result.AccountId.Should().BeGreaterThan(0);
}
```

Steps:
1. Click left margin on line with `var firstName = "John";`
2. Click the green play button to start "Debug Tests (AccountService.Tests)"
3. Debugger stops at the breakpoint
4. In **WATCH**, add: `firstName`, `service`, `repository`
5. Press F10 to step to the next line
6. Watch values update in real-time
7. Press F5 to continue to the next breakpoint

---

## Debugging with Tasks

Your `tasks.json` defines several build/test tasks:

- **build-tests**: Build only the test project
- **build-console**: Build only the console app
- **build-all**: Build the entire solution (default)
- **test-all**: Run all tests
- **clean**: Clean build artifacts

To run a task:
1. Press `Cmd+Shift+B` (Mac) or `Ctrl+Shift+B` (Windows/Linux)
2. Select a task from the dropdown

---

## Troubleshooting

**Breakpoints not working?**
- Ensure you're running in Debug configuration (not Release)
- Rebuild the project: press `Cmd+Shift+B` → select "build-all"
- Restart VS Code

**Debugger not stopping?**
- Check that the launch configuration is correct
- Verify the program path in `launch.json` matches your project structure
- Check OmniSharp Log for errors: View → Output → OmniSharp Log

**"Process exited before debugger attached"?**
- The program ran too fast. Add a breakpoint at the start of your code
- Or use a `System.Diagnostics.Debugger.Break()` to force a pause

**Can't find a variable?**
- Variable might be out of scope (stepped past its declaration)
- Use Watch to add the variable while it's still in scope

---

## Advanced: Debug Single Test

To debug a **specific test** without running all tests:

1. Open Test Explorer (left sidebar, flask icon)
2. Find your test (e.g., `CreateAccountAsync_WithValidData_ReturnsAccountWithId`)
3. Right-click → **Debug Test**

---

## Pro Tips

🎯 **Break on Exception**
- View → Run & Debug → Breakpoints
- Check "Break on All Exceptions" (will pause on any thrown exception)

🎯 **Debug Console Expressions**
- Debug Console can evaluate any C# expression:
  ```
  DateTime.Now.AddDays(5)
  Math.Sqrt(16)
  String.Join(",", new[] { "a", "b", "c" })
  ```

🎯 **Conditional Debug Output**
- Use Debug Console to log complex values without stopping:
  ```csharp
  System.Diagnostics.Debug.WriteLine($"Account: {result.FirstName}");
  ```

🎯 **Time Travel with IntelliTrace** (Premium)
- VS Code doesn't have IntelliTrace, but you can use:
  - Breakpoint history
  - Console logging
  - Watch expressions to review past states

---

## Quick Reference

```bash
# From terminal, start debugging console app
dotnet run --project nrules-console

# From terminal, run tests with output
dotnet test AccountService.Tests --verbosity normal

# Build for debugging (Debug config includes symbols)
dotnet build -c Debug

# Build for release (no debug symbols, optimized)
dotnet build -c Release
```

---

**All set!** Open VS Code, press `Shift+Cmd+D`, and start debugging! 🚀
