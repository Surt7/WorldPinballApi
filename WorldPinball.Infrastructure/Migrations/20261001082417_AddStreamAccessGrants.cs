using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStreamAccessGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stream_access_grants",
                schema: "public",
                columns: table => new
                {
                    grant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    principal_type = table.Column<int>(type: "integer", nullable: false),
                    principal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    origin = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revocation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    granted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stream_access_grants", x => x.grant_id);
                });

            migrationBuilder.CreateTable(
                name: "stream_subscriptions",
                schema: "public",
                columns: table => new
                {
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscriber_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_principal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscribed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notifications_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stream_subscriptions", x => x.subscription_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stream_access_grants_principal_type_principal_id",
                schema: "public",
                table: "stream_access_grants",
                columns: new[] { "principal_type", "principal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_stream_access_grants_status",
                schema: "public",
                table: "stream_access_grants",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_stream_subscriptions_subscriber_user_id_target_type_target_",
                schema: "public",
                table: "stream_subscriptions",
                columns: new[] { "subscriber_user_id", "target_type", "target_principal_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stream_access_grants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stream_subscriptions",
                schema: "public");
        }
    }
}
