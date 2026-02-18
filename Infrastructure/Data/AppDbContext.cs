using Microsoft.EntityFrameworkCore;
using TodoApi.Core;

namespace TodoApi.Infrastructure.Data;

/// <summary>
/// EF Core DbContext. Uses InMemory database so no real DB setup is needed.
/// In a real app you'd use SQL Server, PostgreSQL, etc.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional: seed a few todos for demo
        modelBuilder.Entity<Todo>().HasData(
            new Todo { Id = 1, Title = "Learn CQRS", Description = "Understand commands and queries", IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new Todo { Id = 2, Title = "Try Vertical Slices", Description = "Organize by feature", IsCompleted = false, CreatedAt = DateTime.UtcNow }
        );
    }
}
