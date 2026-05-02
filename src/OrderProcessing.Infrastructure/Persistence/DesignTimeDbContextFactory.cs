using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderProcessing.Infrastructure.Persistence;

/// <summary>
/// Used only by EF Core design-time tools (dotnet ef migrations add).
/// Not used at runtime.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=order_processing_db;Username=postgres;Password=yourpassword")
            .Options;

        return new AppDbContext(options);
    }
}
