using System;

namespace AccountBusiness.Actions
{
    /// <summary>
    /// Simple factory abstraction for creating action instances. Implementations
    /// may resolve actions from an IServiceProvider or fall back to Activator.CreateInstance.
    /// </summary>
    public interface IActionFactory
    {
        T Create<T>(params object[] args) where T : class;
    }
}
