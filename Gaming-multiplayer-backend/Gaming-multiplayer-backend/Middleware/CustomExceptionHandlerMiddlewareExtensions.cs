using Microsoft.AspNetCore.Builder;

namespace Gaming_multiplayer_backend.Middleware
{
    /// <summary>
    /// Extension methods to easily add the custom exception handling middleware to the HTTP request pipeline.
    /// </summary>
    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        /// <summary>
        /// Adds the CustomExceptionHandlerMiddleware to the IApplicationBuilder.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The updated application builder.</returns>
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            // Adds the custom exception handling middleware to the pipeline.
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
