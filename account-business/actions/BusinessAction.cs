using System;
using System.Threading.Tasks;

namespace AccountBusiness.Actions
{
    /// <summary>
    /// Template-method base for business actions.
    /// Provides a single ExecuteAsync() entry point with overridable pre/post hooks
    /// and a protected abstract RunAsync() which concrete actions must implement.
    /// </summary>
    public abstract class BusinessAction<T>
    {
        /// <summary>
        /// Template method: calls PreExecuteAsync, RunAsync and PostExecuteAsync in order.
        /// </summary>
        public async Task<T> ExecuteAsync()
        {
            await PreExecuteAsync();
            var result = await RunAsync();
            await PostExecuteAsync(result);
            return result;
        }

        /// <summary>
        /// Override to run logic before the main action. Default does nothing.
        /// </summary>
        protected virtual Task PreExecuteAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Concrete actions implement this with their core logic and return the result.
        /// </summary>
        protected abstract Task<T> RunAsync();

        /// <summary>
        /// Override to run logic after the main action. Default does nothing.
        /// </summary>
        protected virtual Task PostExecuteAsync(T result)
        {
            return Task.CompletedTask;
        }
    }
}
