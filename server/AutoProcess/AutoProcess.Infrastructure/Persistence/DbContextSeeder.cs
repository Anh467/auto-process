// AutoProcess.Infrastructure/Persistence/DbContextSeeder.cs
using AutoProcess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class DbContextSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbContextSeeder> _logger;

    public DbContextSeeder(AppDbContext context, ILogger<DbContextSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("⏳ Checking pending migrations...");

            // Tự động migrate khi khởi động
            await _context.Database.MigrateAsync();

            _logger.LogInformation("✅ Migration completed!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Migration failed!");
            throw;
        }
    }
}