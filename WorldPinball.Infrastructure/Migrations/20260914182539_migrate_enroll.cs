using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class migrate_enroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "validity_hours",
                schema: "public",
                table: "enrollment_tokens",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "validity_hours",
                schema: "public",
                table: "enrollment_tokens");
        }
    }
}
