using GreenGainsBackend.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class MeterDataDbContext : DbContext
{
    public MeterDataDbContext(DbContextOptions<MeterDataDbContext> options) : base(options)
    {
    }

    static MeterDataDbContext() => NpgsqlConnection.GlobalTypeMapper.MapEnum<OBISCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
