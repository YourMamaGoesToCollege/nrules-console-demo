# NRules Console Demo Solution

## Overview

This is a comprehensive .NET 8.0 solution demonstrating a layered architecture with Entity Framework Core, SQLite database integration, and NRules business rules engine. The solution is designed to showcase separation of concerns through multiple project layers.

## Solution Architecture

### Project Structure

```
nrules-console-demo/
├── nrules-console-demo.sln          # Main solution file
├── account-entities/                # Shared entity models
│   ├── Account.cs                   # Account entity model
│   └── account-entities.csproj
├── account-repository/              # Data access layer
│   ├── AccountDbContext.cs          # EF Core DbContext
│   ├── AccountDbContextFactory.cs   # Design-time factory for migrations
│   ├── Migrations/                  # EF Core migrations folder
│   └── account-repository.csproj
├── account-business/                # Business logic layer
│   ├── account-business.csproj
│   └── (NRules integration here)
├── account-service/                 # Service layer
│   └── account-service.csproj
└── nrules-console/                  # Console application (entry point)
    ├── Program.cs
    └── nrules-console.csproj
```

### Project Dependencies

```
nrules-console → account-service → account-business → {account-repository, account-entities}
                                   ↓
                            account-business uses NRules for business rules
```

## Prerequisites

- **.NET SDK 8.0** or higher
- **Entity Framework Core Tools** (installed globally during setup)
- **MacBook/Unix-like environment** (SQLite works on all platforms)

## Installation & Setup

### 1. Clone or Navigate to the Project

```bash
cd /path/to/nrules-console-demo
```

### 2. Add dotnet-ef Tools (if not already installed)

```bash
# Install Entity Framework Core command-line tools globally
dotnet tool install --global dotnet-ef --version 8.0.11

# Update PATH (for macOS/Linux with zsh)
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"

# To make it permanent, add to ~/.zprofile:
echo 'export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"' >> ~/.zprofile
source ~/.zprofile
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Database Setup (Already Completed)

The SQLite database has been initialized and migrations applied. The database file is created at:

```
~/.nrules-console-demo/accounts.db
```

**Manual steps (if needed):**

```bash
# Create a new migration (if you modify Account entity)
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
dotnet ef migrations add YourMigrationName --project account-repository

# Apply migrations to create/update database
dotnet ef database update --project account-repository
```

## Configuration

### Database Connection

The database connection is configured in `AccountDbContextFactory.cs`:

```csharp
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".nrules-console-demo",
    "accounts.db"
);
var connectionString = $"Data Source={dbPath}";
```

**To change the database location:**
- Edit `AccountDbContextFactory.cs` and modify the `dbPath` variable
- Run `dotnet ef database update --project account-repository` to recreate the database

### Adding Connection String to appsettings.json (Optional)

If you want to use `appsettings.json` for configuration:

1. Create `appsettings.json` in the console project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=~/nrules-console-demo/accounts.db"
  }
}
```

2. Update `AccountDbContextFactory.cs` to read from configuration.

## Entity Model: Account

The `Account` entity represents a user account with the following properties:

| Property | Type | Required | Notes |
|----------|------|----------|-------|
| AccountId | int | Yes | Primary key, auto-generated |
| FirstName | string | Yes | Max 100 characters |
| LastName | string | Yes | Max 100 characters |
| BirthDate | DateTime | Yes | Birth date of the account holder |
| IsActive | bool | Yes | Account status (default: true) |
| City | string | No | Max 100 characters |
| EmailAddress | string | Yes | Max 255 characters, unique, validated |
| PetCount | int | Yes | Number of pets (default: 0) |
| CreatedAt | DateTime | Generated | Automatically set on creation |
| UpdatedAt | DateTime | Generated | Automatically updated on modification |

### Entity Relationships

- **Unique Constraint**: `EmailAddress` (prevents duplicate emails)
- **Indexed**: `EmailAddress` (for faster lookups)

## Usage Examples

### Using the Repository Layer

```csharp
using AccountRepository;
using Microsoft.EntityFrameworkCore;
using AccountEntities;

// Create DbContext
var options = new DbContextOptionsBuilder<AccountDbContext>()
    .UseSqlite("Data Source=~/.nrules-console-demo/accounts.db")
    .Build();

var dbContext = new AccountDbContext(options);

// Create a new account
var account = new Account
{
    FirstName = "John",
    LastName = "Doe",
    BirthDate = new DateTime(1990, 1, 15),
    IsActive = true,
    City = "New York",
    EmailAddress = "john.doe@example.com",
    PetCount = 2
};

dbContext.Accounts.Add(account);
await dbContext.SaveChangesAsync();

// Query accounts
var activeAccounts = await dbContext.Accounts
    .Where(a => a.IsActive)
    .ToListAsync();

// Update an account
account.PetCount = 3;
await dbContext.SaveChangesAsync();

// Delete an account
dbContext.Accounts.Remove(account);
await dbContext.SaveChangesAsync();
```

## Running the Console Application

```bash
# Build and run
dotnet run --project nrules-console

# Or build first, then run
dotnet build
dotnet run --project nrules-console
```

## Working with Migrations

### Create a New Migration (after modifying Account model)

```bash
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
dotnet ef migrations add YourDescriptiveName --project account-repository
```

### Apply Migrations

```bash
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
dotnet ef database update --project account-repository
```

### Rollback Last Migration

```bash
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
dotnet ef database update PreviousMigrationName --project account-repository
```

### View Current Database State

```bash
# Connect to SQLite database
sqlite3 ~/.nrules-console-demo/accounts.db

# Inside sqlite3 prompt:
.tables                 # List all tables
.schema Accounts        # View Account table schema
SELECT * FROM Accounts; # Query all accounts
.quit                   # Exit sqlite3
```

## Integrating NRules

The `account-business` project includes NRules (v1.0.3) for business rule evaluation. 

### Example: Business Rule Implementation

```csharp
using NRules;
using NRules.Fluent.Dsl;
using AccountEntities;

namespace AccountBusiness
{
    // Define a rule
    public class ActiveAccountRule : Rule
    {
        public override void Define()
        {
            Account account = null;

            When()
                .Match<Account>(() => account, a => a.IsActive);

            Then()
                .Do(ctx => Console.WriteLine($"Processing active account: {account.FirstName}"));
        }
    }

    // Use the rule
    public class RuleEngine
    {
        private readonly ISessionFactory _sessionFactory;

        public RuleEngine()
        {
            _sessionFactory = RuleRepositoryFactory.Current
                .GetRuleRepository(typeof(ActiveAccountRule).Assembly)
                .CreateFactory();
        }

        public void EvaluateAccounts(IEnumerable<Account> accounts)
        {
            var session = _sessionFactory.CreateSession();
            foreach (var account in accounts)
            {
                session.Insert(account);
            }
            session.Fire();
        }
    }
}
```

## Project References

- **account-entities**: No external dependencies (shared models)
- **account-repository**: 
  - Depends on: `account-entities`
  - NuGet: `Microsoft.EntityFrameworkCore.Sqlite (8.0.11)`, `Microsoft.EntityFrameworkCore.Design (8.0.11)`
- **account-business**: 
  - Depends on: `account-entities`
  - NuGet: `NRules (1.0.3)`
- **account-service**: 
  - Depends on: `account-business`
- **nrules-console**: 
  - Depends on: `account-service`

## Troubleshooting

### Issue: "dotnet-ef: command not found"

**Solution:**
```bash
# Install/reinstall globally
dotnet tool install --global dotnet-ef --version 8.0.11

# Add to PATH
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
```

### Issue: Database file not found

**Solution:**
- Ensure the directory `~/.nrules-console-demo/` exists
- Run `dotnet ef database update --project account-repository` to recreate it

### Issue: "Unable to create a 'DbContext' of type" during migrations

**Solution:**
- Ensure `AccountDbContextFactory.cs` exists in `account-repository` project
- Verify the factory implements `IDesignTimeDbContextFactory<AccountDbContext>`

### Issue: "Cannot write to SQLite database" (Permission error)

**Solution:**
```bash
# Check permissions
ls -la ~/.nrules-console-demo/

# Fix permissions if needed
chmod 755 ~/.nrules-console-demo/
chmod 644 ~/.nrules-console-demo/accounts.db
```

## Development Workflow

1. **Modify Entity Models**: Edit `account-entities/Account.cs`
2. **Create Migration**: `dotnet ef migrations add MigrationName --project account-repository`
3. **Apply Migration**: `dotnet ef database update --project account-repository`
4. **Implement Business Logic**: Add rules to `account-business` using NRules
5. **Test**: Run console application with `dotnet run --project nrules-console`

## Performance Considerations

- **Database Indexes**: EmailAddress is indexed for faster lookups
- **Unique Constraints**: EmailAddress is unique to prevent duplicates
- **Lazy Loading**: Disabled by default in EF Core 8.0 (use `.Include()` for eager loading)

## Best Practices

1. **Always use async/await** for database operations: `SaveChangesAsync()`, `ToListAsync()`
2. **Validate entities** before saving (use Data Annotations and Fluent API)
3. **Use migrations** for schema changes (never manually edit the database)
4. **Implement repository pattern** for data access abstraction
5. **Keep business logic separate** from data access logic
6. **Use dependency injection** for DbContext in production applications

## Next Steps

1. Implement repository pattern in `account-repository`
2. Create business logic services in `account-business`
3. Define and implement NRules business rules
4. Add unit tests for business logic
5. Create console UI for managing accounts
6. Implement logging and error handling

## Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [NRules Documentation](https://github.com/NRules/NRules)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)

## License

This project is for educational purposes.

## Support

For issues or questions:
1. Check the Troubleshooting section
2. Verify all prerequisites are installed
3. Ensure database migrations are up-to-date
4. Check the EF Core and NRules documentation

---

**Last Updated**: December 9, 2025
**Solution Version**: 1.0
**.NET Version**: 8.0
**Database**: SQLite
