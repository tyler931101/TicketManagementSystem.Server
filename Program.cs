using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseCors("AllowAll");
app.MapControllers();

Console.WriteLine("🚀 Ticket Management Server Starting...");
Console.WriteLine("📍 Server will run on: http://localhost:5000");
Console.WriteLine("🗄️  Database: SQLite (ticket_system_server.db)");
Console.WriteLine();

try
{
    await app.RunAsync("http://localhost:5000");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Server failed to start: {ex.Message}");
}
