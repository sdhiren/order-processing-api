using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Middleware;
using OrderProcessing.Application;
using OrderProcessing.Infrastructure;
using OrderProcessing.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ----- Services -----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database", tags: ["db", "postgres"]);

// ----- App pipeline -----
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrderProcessing API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-apply EF migrations on startup, seed dev data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
        await DataSeeder.SeedAsync(db);
}

await app.RunAsync();

// Make Program accessible to integration tests
public partial class Program { }
