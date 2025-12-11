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
        /// Override to run logic before the main action. Calls Validate by default.
        /// </summary>
        protected virtual async Task PreExecuteAsync()
        {
            await Validate();
        }

        /// <summary>
        /// Validates the action inputs before execution.
        /// Override to implement specific validation logic.
        /// </summary>
        protected virtual Task Validate()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Concrete actions implement this with their core logic and return the result.
        /// </summary>
        protected abstract Task<T> RunAsync();

        /// <summary>
        /// Override to run logic after the main action. Calls AuditLog by default.
        /// </summary>
        protected virtual async Task PostExecuteAsync(T result)
        {
            await PostValidate();
            await AuditLog(result);
        }

        protected abstract Task PostValidate();

        /// <summary>
        /// Provides contextual audit logging for the action.
        /// Override to implement specific audit logging logic.
        /// </summary>
        protected virtual Task AuditLog(T result)
        {
            return Task.CompletedTask;
        }
    }
}
