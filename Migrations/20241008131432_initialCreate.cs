using GreenGainsBackend.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGainsBackend.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:obis_code", "code_1_7_0,code_1_8_0,code_2_7_0,code_2_8_0,code_3_8_0,code_4_8_0,code_16_7_0,code_31_7_0,code_32_7_0,code_51_7_0,code_52_7_0,code_71_7_0,code_72_7_0");

            migrationBuilder.CreateTable(
                name: "sensorreadings",
                columns: table => new
                {
                    Topic = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Uptime = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Code = table.Column<OBISCode>(type: "obis_code", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensorreadings");
        }
    }
}
