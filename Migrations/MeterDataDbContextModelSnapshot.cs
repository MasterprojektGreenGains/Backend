﻿// <auto-generated />
using System;
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

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GreenGainsBackend.Domain.MeterReading", b =>
                {
                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Topic")
                        .HasColumnType("text");

                    b.Property<string>("Uptime")
                        .HasColumnType("text");

                    b.ToTable("meterreadings");
                });
#pragma warning restore 612, 618
        }
    }
}
