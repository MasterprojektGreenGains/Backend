using GreenGainsBackend.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

public class MeterDataDbContext : DbContext
{
    public MeterDataDbContext(DbContextOptions<MeterDataDbContext> options) : base(options)
    {
    }

    static MeterDataDbContext() => NpgsqlConnection.GlobalTypeMapper.MapEnum<OBISCode>();

    public DbSet<SensorReading> SensorReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<OBISCode>();

        modelBuilder.Entity<SensorReading>().HasNoKey();
        modelBuilder.Entity<SensorReading>().Property(s => s.Code).HasConversion(new EnumToStringConverter<OBISCode>());
        modelBuilder.Entity<SensorReading>().HasIndex(s => new { s.Topic, s.Timestamp });
    }
}
