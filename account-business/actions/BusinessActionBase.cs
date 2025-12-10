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
        /// Validates inputs by calling ValidateInputs().
        /// Throws AggregateException if any validation errors are found.
        /// </summary>
        protected override async Task Validate()
        {
            ValidationErrors.Clear();
            await ValidateInputs();

            if (ValidationErrors.Count > 0)
            {
                var exceptions = new List<Exception>();
                foreach (var error in ValidationErrors)
                {
                    exceptions.Add(new ArgumentException(error));
                }
                throw new AggregateException("Validation failed", exceptions);
            }
        }

        /// <summary>
        /// Override to implement specific validation logic.
        /// Add validation errors to ValidationErrors collection.
        /// </summary>
        protected virtual Task ValidateInputs()
        {
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

        /// <summary>
        /// Helper method to validate required string field.
        /// </summary>
        protected void ValidateRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddValidationError($"{fieldName} is required.");
            }
        }

        /// <summary>
        /// Helper method to validate email format.
        /// </summary>
        protected void ValidateEmail(string email, string fieldName = "Email")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                AddValidationError($"{fieldName} is required.");
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                AddValidationError($"{fieldName} must be a valid email address.");
            }
        }

        /// <summary>
        /// Helper method to validate minimum length.
        /// </summary>
        protected void ValidateMinLength(string value, int minLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddValidationError($"{fieldName} is required.");
                return;
            }

            if (value.Length < minLength)
            {
                AddValidationError($"{fieldName} must be at least {minLength} characters long.");
            }
        }
    }
}
