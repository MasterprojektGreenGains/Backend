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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
