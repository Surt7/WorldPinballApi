using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatorTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operator_tokens",
                schema: "public",
                columns: table => new
                {
                    token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    prefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    issued_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operator_tokens", x => x.token_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operator_tokens_is_active",
                schema: "public",
                table: "operator_tokens",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_operator_tokens_token_hash",
                schema: "public",
                table: "operator_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operator_tokens",
                schema: "public");
        }
    }
}
