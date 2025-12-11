using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountBusiness.Actions
{
    /// <summary>
    /// Base implementation of BusinessAction providing common functionality
    /// for domain-specific business actions.
    /// 
    /// Concrete actions should extend this class and implement:
    /// - RunAsync() - Core business logic
    /// - ValidateInputs() - Specific validation rules
    /// - GetAuditContext() - Audit metadata
    /// </summary>
    public abstract class BusinessActionBase<T> : BusinessAction<T>
    {
        protected List<string> ValidationErrors { get; } = new();

        /// <summary>
        /// Validates the action.
        /// Throws AggregateException if any validation errors are found.
        /// </summary>
        protected override Task Validate()
        {
            ValidationErrors.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to implement post-execution validation logic.
        /// This is called after RunAsync() completes but before AuditLog().
        /// </summary>
        protected override Task PostValidate()
        {
            if (ValidationErrors.Count > 0)
            {
                var exceptions = new List<Exception>();
                foreach (var error in ValidationErrors)
                {
                    exceptions.Add(new ArgumentException(error));
                }
                throw new AggregateException("Validation failed", exceptions);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Provides audit logging with context information.
        /// Logs action name, result type, and custom context.
        /// </summary>
        protected override async Task AuditLog(T result)
        {
            var context = await GetAuditContext(result);
            var actionName = GetType().Name;
            var resultType = result?.GetType().Name ?? "null";

            // In production, replace Console.WriteLine with proper logging framework
            Console.WriteLine($"[AUDIT] Action: {actionName}");
            Console.WriteLine($"[AUDIT] Result Type: {resultType}");
            Console.WriteLine($"[AUDIT] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");

            if (context != null && context.Count > 0)
            {
                Console.WriteLine("[AUDIT] Context:");
                foreach (var kvp in context)
                {
                    Console.WriteLine($"[AUDIT]   {kvp.Key}: {kvp.Value}");
                }
            }
        }

        /// <summary>
        /// Override to provide custom audit context for the action.
        /// Return a dictionary of key-value pairs for audit logging.
        /// </summary>
        protected virtual Task<Dictionary<string, object>> GetAuditContext(T result)
        {
            return Task.FromResult(new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to add validation error.
        /// </summary>
        protected void AddValidationError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                ValidationErrors.Add(error);
            }
        }


    }
}
