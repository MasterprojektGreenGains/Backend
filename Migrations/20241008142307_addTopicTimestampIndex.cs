using GreenGainsBackend.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGainsBackend.Migrations
{
    /// <inheritdoc />
    public partial class addTopicTimestampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "sensorreadings",
                type: "text",
                nullable: false,
                oldClrType: typeof(OBISCode),
                oldType: "obis_code");

            migrationBuilder.CreateIndex(
                name: "IX_sensorreadings_Topic_Timestamp",
                table: "sensorreadings",
                columns: new[] { "Topic", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sensorreadings_Topic_Timestamp",
                table: "sensorreadings");

            migrationBuilder.AlterColumn<OBISCode>(
                name: "Code",
                table: "sensorreadings",
                type: "obis_code",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
