using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Queue> Queues { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<PasswordResetFlow> PasswordResetFlows { get; set; }
    public DbSet<SubscriptionManagement> SubscriptionManagements { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
