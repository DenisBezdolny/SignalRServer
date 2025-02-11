using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
using GMB.Domain;
using Gaming_multiplayer_backend.Middleware;
using Gaming_multiplayer_backend.Extensions;
using Gaming_multiplayer_backend.Hubs;
using Gaming_multiplayer_backend;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Configure Logging using Serilog
// ---------------------------------------------------------------------
builder.ConfigureSerilog();

// ---------------------------------------------------------------------
// Add services to the dependency injection container
// ---------------------------------------------------------------------
// Register controllers for API endpoints.
builder.Services.AddControllers();

// Configure and register the Entity Framework Core DbContext.
// The connection string is retrieved from the configuration, and the migrations are located in the "GMB.Domain" assembly.
builder.Services.AddDbContext<GMB_DbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("GMB.Domain"))
);

// Register other application-specific services (repositories, business logic, etc.).
builder.Services.AddApplicationServices();

var app = builder.Build();

// ---------------------------------------------------------------------
// Database Cleanup on Startup
// ---------------------------------------------------------------------
// Create a scope to get scoped services (like DbContext and ILogger).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

    // Delete all existing clients and rooms from the database.
    dbContext.Clients.ExecuteDelete();
    dbContext.Rooms.ExecuteDelete();

    // Persist the deletions.
    dbContext.SaveChanges();
    logger.Information("[Startup Cleanup] All clients and rooms have been deleted.");
}

// ---------------------------------------------------------------------
// Apply Migrations using DbInitializer
// ---------------------------------------------------------------------
try
{
    // This method applies any pending migrations and seeds the database if needed.
    DbInitializer.Initialize(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger>();
    // Log a fatal error if migration fails, then rethrow the exception.
    logger.Fatal(ex, "Application failed to start due to database migration error.");
    throw;
}

// ---------------------------------------------------------------------
// Configure the HTTP Request Pipeline
// ---------------------------------------------------------------------

// In Development environment, enable Swagger UI for API documentation.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// ---------------------------------------------------------------------
// Custom Middleware to Log the Origin Header for CORS debugging
// ---------------------------------------------------------------------
// This middleware logs the value of the "Origin" header for each request.
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Origin"))
    {
        var origin = context.Request.Headers["Origin"].ToString();
        // Log the Origin using the built-in logger (configured with Serilog)
        app.Logger.LogInformation("[CORS Log] Request from Origin: {Origin}", origin);
    }
    await next.Invoke();
});

// ---------------------------------------------------------------------
// Configure CORS Policy
// ---------------------------------------------------------------------
// Allow any header and method, allow credentials, and restrict allowed origins to a list (e.g., localhost and a tunnel URL).
app.UseCors(builder =>
    builder.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
           .WithOrigins("http://localhost:4200", "https://bumpy-papayas-tan.loca.lt")
);

// ---------------------------------------------------------------------
// Map SignalR Hubs and Other Middleware
// ---------------------------------------------------------------------

// Map the SignalR hub endpoint for notifications.
app.MapHub<NotificationHub>("/notifications");

// Use custom exception handling middleware.
app.UseCustomExceptionHandler();

// Use authorization middleware (if your app requires it).
app.UseAuthorization();

// Map controller routes.
app.MapControllers();

// Run the application.
app.Run();
