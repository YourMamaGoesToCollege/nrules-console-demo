using System;
using Microsoft.Extensions.DependencyInjection;

namespace AccountBusiness.Actions
{
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
                try
                {
                    // Use DI container with ActivatorUtilities to resolve services
                    // This will inject registered services (IAccountRepository, ILogger<T>)
                    // and use provided args for non-service parameters (like Account)
                    return ActivatorUtilities.CreateInstance<T>(_provider, args);
                }
                catch (Exception ex)
                {
                    // Log or wrap the exception with more context
                    throw new InvalidOperationException(
                        $"Failed to create instance of {typeof(T).Name} using DI. " +
                        $"Ensure all required services are registered. Error: {ex.Message}", ex);
                }
            }

            // Fallback to Activator when no service provider available
            return Activator.CreateInstance(typeof(T), args) as T
                ?? throw new InvalidOperationException($"Unable to create action of type {typeof(T)}");
        }
    }
}
