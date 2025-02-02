using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
using GMB.Domain;
using Gaming_multiplayer_backend.Middleware;
using Gaming_multiplayer_backend.Extensions;
using Gaming_multiplayer_backend.Hubs;
using Gaming_multiplayer_backend;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.ConfigureSerilog();
builder.Services.AddControllers();


builder.Services.AddDbContext<GMB_DbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("GMB.Domain")));

builder.Services.AddApplicationServices();


var app = builder.Build();

// Apply migrations using DbInitializer
try
{
    DbInitializer.Initialize(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger>();
    logger.Fatal(ex, "Application failed to start due to database migration error.");
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<NotificationHub>("/notifications");
app.UseCustomExceptionHandler();

app.UseCors(builder =>
    builder.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
           .WithOrigins("http://localhost:4200"));

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

