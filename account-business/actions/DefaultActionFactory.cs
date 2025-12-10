using System;

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
                // Try to resolve from DI if available
                var service = _provider.GetService(typeof(T)) as T;
                if (service != null) return service;
            }

            // Fallback to Activator
            return Activator.CreateInstance(typeof(T), args) as T ?? throw new InvalidOperationException($"Unable to create action of type {typeof(T)}");
        }
    }
}
