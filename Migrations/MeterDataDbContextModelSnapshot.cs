﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenGainsBackend.Migrations
{
    [DbContext(typeof(MeterDataDbContext))]
    partial class MeterDataDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "obis_code", new[] { "code_1_7_0", "code_1_8_0", "code_2_7_0", "code_2_8_0", "code_3_8_0", "code_4_8_0", "code_16_7_0", "code_31_7_0", "code_32_7_0", "code_51_7_0", "code_52_7_0", "code_71_7_0", "code_72_7_0" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GreenGainsBackend.Domain.SensorReading", b =>
                {
                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Topic")
                        .HasColumnType("text");

                    b.Property<string>("Uptime")
                        .HasColumnType("text");

                    b.Property<double>("Value")
                        .HasColumnType("double precision");

                    b.HasIndex("Topic", "Timestamp");

                    b.ToTable("sensorreadings");
                });
#pragma warning restore 612, 618
        }
    }
}
