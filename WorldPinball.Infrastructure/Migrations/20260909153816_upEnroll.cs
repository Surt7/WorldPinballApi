using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class upEnroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_machines_idempotency_key",
                schema: "public",
                table: "machines",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machines_nonce",
                schema: "public",
                table: "machines",
                column: "nonce",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_machines_idempotency_key",
                schema: "public",
                table: "machines");

            migrationBuilder.DropIndex(
                name: "ix_machines_nonce",
                schema: "public",
                table: "machines");
        }
    }
}
