using System.ComponentModel.DataAnnotations; // For handling ValidationException
using System.Net;                           // For HTTP status codes
using System.Text.Json;                     // For serializing JSON responses
using Microsoft.AspNetCore.Http;            // For HttpContext and RequestDelegate
using Microsoft.Extensions.Logging;         // For ILogger

namespace Gaming_multiplayer_backend.Middleware
{
    /// <summary>
    /// Middleware that catches exceptions thrown during HTTP request processing,
    /// logs the error, and returns a JSON response with an appropriate HTTP status code.
    /// </summary>
    public class CustomExceptionHandlerMiddleware
    {
        // Delegate for the next middleware in the pipeline.
        private readonly RequestDelegate _next;

        // Logger for logging errors and information.
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware with the given HttpContext.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline.
                await _next(context);
            }
            catch (Exception exception)
            {
                // Handle any exceptions thrown by downstream middleware.
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Handles exceptions by setting an appropriate HTTP status code,
        /// logging the error, and writing a JSON error response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Default HTTP status code for unhandled exceptions is 500 (Internal Server Error).
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            // Check the type of the exception to set a more specific status code and error message.
            switch (exception)
            {
                // For validation errors, return 400 (Bad Request) along with error details.
                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new
                    {
                        error = validationException.Message,
                        details = validationException.Data
                    });
                    break;

                // For all other unhandled exceptions, log the error and return a generic message.
                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    result = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
                    break;
            }

            // Set the response content type to JSON.
            context.Response.ContentType = "application/json";
            // Set the HTTP status code based on the exception type.
            context.Response.StatusCode = (int)code;

            // Write the JSON response to the HTTP response stream.
            return context.Response.WriteAsync(result);
        }
    }
}
