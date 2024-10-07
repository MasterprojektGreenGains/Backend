using GreenGainsBackend.Domain;
using Microsoft.EntityFrameworkCore;

public class MeterDataDbContext : DbContext
{
    public MeterDataDbContext(DbContextOptions<MeterDataDbContext> options) : base(options)
    {
    }

    public DbSet<MeterReading> MeterReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MeterReading>().HasNoKey();

    }
}
