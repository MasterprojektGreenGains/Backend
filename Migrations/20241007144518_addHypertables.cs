﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGainsBackend.Migrations
{
    /// <inheritdoc />
    public partial class addHypertables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SELECT create_hypertable('meterreadings', by_range('Timestamp'));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}