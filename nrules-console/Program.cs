using System;
using Microsoft.Extensions.DependencyInjection;
using AccountBusiness;
using AccountBusiness.Actions;
using AccountRepository;
using AccountEntities;
using AccountService;
using Microsoft.EntityFrameworkCore;

// Minimal DI demo for developers: register business/action factory and service objects
var services = new ServiceCollection();

// Action factory (will attempt to resolve action types from the ServiceProvider first)
services.AddSingleton<IActionFactory, DefaultActionFactory>(sp => new DefaultActionFactory(sp));
// Register concrete action types so DefaultActionFactory can resolve them from IServiceProvider
services.AddTransient<CreateAccountAction>();

// Register business service and its interface
services.AddTransient<IAccountBusinessService, AccountBusinessService>();

// Register repository & DbContext for demo (in-memory DB here for simplicity)
services.AddDbContext<AccountDbContext>(opt => opt.UseInMemoryDatabase("nrules-demo"));
services.AddTransient<IAccountRepository, AccountRepository.AccountRepository>();

// Register AccountService
services.AddTransient<AccountService.AccountService>();

var provider = services.BuildServiceProvider();

Console.WriteLine("nrules-console demo DI container configured.");

// Resolve a sample AccountService and show a create flow (demo only)
using (var scope = provider.CreateScope())
{
	var svc = scope.ServiceProvider.GetRequiredService<AccountService.AccountService>();

	var account = new Account
	{
		FirstName = "Demo",
		LastName = "User",
		EmailAddress = "demo.user@example.com",
		BirthDate = DateTime.UtcNow.AddYears(-30),
		IsActive = true
	};

	var created = await svc.CreateAccountAsync(account);
	Console.WriteLine($"Created account Id: {created.AccountId}, Email: {created.EmailAddress}");
}
