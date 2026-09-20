using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatescore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_score_entries_session_id",
                schema: "public",
                table: "score_entries");

            migrationBuilder.DropIndex(
                name: "ix_game_sessions_user_id_started_at_utc",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.AlterColumn<long>(
                name: "score",
                schema: "public",
                table: "score_entries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "slot_number",
                schema: "public",
                table: "score_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "user_profiles",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    country_code = table.Column<string>(type: "text", nullable: false),
                    is_profile_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "game_session_players",
                schema: "public",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_number = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_profile_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    guest_initials = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_session_players", x => new { x.session_id, x.slot_number });
                    table.ForeignKey(
                        name: "fk_game_session_players_game_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "public",
                        principalTable: "game_sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_game_session_players_user_profiles_user_profile_user_id",
                        column: x => x.user_profile_user_id,
                        principalSchema: "public",
                        principalTable: "user_profiles",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_session_id",
                schema: "public",
                table: "score_entries",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_session_id_slot_number",
                schema: "public",
                table: "score_entries",
                columns: new[] { "session_id", "slot_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_session_players_user_id_joined_at_utc",
                schema: "public",
                table: "game_session_players",
                columns: new[] { "user_id", "joined_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_game_session_players_user_profile_user_id",
                schema: "public",
                table: "game_session_players",
                column: "user_profile_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_session_players",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_profiles",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_score_entries_session_id",
                schema: "public",
                table: "score_entries");

            migrationBuilder.DropIndex(
                name: "ix_score_entries_session_id_slot_number",
                schema: "public",
                table: "score_entries");

            migrationBuilder.DropColumn(
                name: "slot_number",
                schema: "public",
                table: "score_entries");

            migrationBuilder.AlterColumn<int>(
                name: "score",
                schema: "public",
                table: "score_entries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                schema: "public",
                table: "game_sessions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_session_id",
                schema: "public",
                table: "score_entries",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_user_id_started_at_utc",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "user_id", "started_at_utc" });
        }
    }
}
