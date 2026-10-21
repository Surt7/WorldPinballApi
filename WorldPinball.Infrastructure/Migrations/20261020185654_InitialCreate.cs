using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldPinball.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "ca_certificates",
                schema: "public",
                columns: table => new
                {
                    ca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ca_type = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    subject_dn = table.Column<string>(type: "text", nullable: false),
                    issuer_dn = table.Column<string>(type: "text", nullable: false),
                    serial_number = table.Column<string>(type: "text", nullable: false),
                    thumbprint_sha256 = table.Column<string>(type: "text", nullable: false),
                    cert_pem = table.Column<string>(type: "text", nullable: false),
                    private_key_ref = table.Column<string>(type: "text", nullable: true),
                    not_before_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    not_after_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_trusted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ca_certificates", x => x.ca_id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "public",
                columns: table => new
                {
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial_physical = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: true),
                    public_key_fingerprint = table.Column<string>(type: "text", nullable: true),
                    key_type = table.Column<string>(type: "text", nullable: true),
                    key_algorithm = table.Column<string>(type: "text", nullable: true),
                    last_counter = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    provisioned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    client_version = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.device_id);
                });

            migrationBuilder.CreateTable(
                name: "game_modes",
                schema: "public",
                columns: table => new
                {
                    mode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_modes", x => x.mode_id);
                });

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

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "publishers",
                schema: "public",
                columns: table => new
                {
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publishers", x => x.publisher_id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                schema: "public",
                columns: table => new
                {
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.region_id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "public",
                columns: table => new
                {
                    role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    permission = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role, x.permission });
                });

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

            migrationBuilder.CreateTable(
                name: "streaming_access_requests",
                schema: "public",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streaming_access_requests", x => x.request_id);
                });

            migrationBuilder.CreateTable(
                name: "streaming_partners",
                schema: "public",
                columns: table => new
                {
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streaming_partners", x => x.partner_id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    initials = table.Column<string>(type: "text", nullable: false),
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
                name: "device_certificates",
                schema: "public",
                columns: table => new
                {
                    device_cert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cert_serial_number = table.Column<string>(type: "text", nullable: false),
                    cert_thumbprint_sha256 = table.Column<string>(type: "text", nullable: false),
                    subject_dn = table.Column<string>(type: "text", nullable: false),
                    issuer_dn = table.Column<string>(type: "text", nullable: false),
                    cert_pem = table.Column<string>(type: "text", nullable: false),
                    not_before_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    not_after_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    issued_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoke_reason = table.Column<string>(type: "text", nullable: true),
                    superseded_by_device_cert_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_certificates", x => x.device_cert_id);
                    table.ForeignKey(
                        name: "fk_device_certificates_ca_certificates_ca_id",
                        column: x => x.ca_id,
                        principalSchema: "public",
                        principalTable: "ca_certificates",
                        principalColumn: "ca_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_device_certificates_device_certificates_superseded_by_devic",
                        column: x => x.superseded_by_device_cert_id,
                        principalSchema: "public",
                        principalTable: "device_certificates",
                        principalColumn: "device_cert_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_device_certificates_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ip_licenses",
                schema: "public",
                columns: table => new
                {
                    ip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_owner = table.Column<string>(type: "text", nullable: true),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ip_licenses", x => x.ip_id);
                    table.ForeignKey(
                        name: "fk_ip_licenses_publishers_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "public",
                        principalTable: "publishers",
                        principalColumn: "publisher_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "certificate_revocations",
                schema: "public",
                columns: table => new
                {
                    revocation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_cert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    revoked_by_user_id = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_certificate_revocations", x => x.revocation_id);
                    table.ForeignKey(
                        name: "fk_certificate_revocations_device_certificates_device_cert_id",
                        column: x => x.device_cert_id,
                        principalSchema: "public",
                        principalTable: "device_certificates",
                        principalColumn: "device_cert_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ip_license_regions",
                schema: "public",
                columns: table => new
                {
                    ip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ip_license_regions", x => new { x.ip_id, x.region_id });
                    table.ForeignKey(
                        name: "fk_ip_license_regions_ip_licenses_ip_id",
                        column: x => x.ip_id,
                        principalSchema: "public",
                        principalTable: "ip_licenses",
                        principalColumn: "ip_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ip_license_regions_regions_region_id",
                        column: x => x.region_id,
                        principalSchema: "public",
                        principalTable: "regions",
                        principalColumn: "region_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "machines",
                schema: "public",
                columns: table => new
                {
                    machine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<Guid>(type: "uuid", nullable: false),
                    nonce = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    world_pinball_serial_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    manufacturer_model_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    manufacturer_model_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    manufacturer_model_year = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    manufacturer_serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    manufacturer_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    machine_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    voltage = table.Column<int>(type: "integer", nullable: true),
                    frequency_hz = table.Column<int>(type: "integer", nullable: true),
                    amperage = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    manufactured_date = table.Column<DateOnly>(type: "date", nullable: true),
                    display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    registered_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_machines", x => x.machine_id);
                    table.ForeignKey(
                        name: "fk_machines_ip_licenses_ip_id",
                        column: x => x.ip_id,
                        principalSchema: "public",
                        principalTable: "ip_licenses",
                        principalColumn: "ip_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "missions",
                schema: "public",
                columns: table => new
                {
                    mission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mode_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    mission_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_missions", x => x.mission_id);
                    table.ForeignKey(
                        name: "fk_missions_game_modes_mode_id",
                        column: x => x.mode_id,
                        principalSchema: "public",
                        principalTable: "game_modes",
                        principalColumn: "mode_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_missions_ip_licenses_ip_id",
                        column: x => x.ip_id,
                        principalSchema: "public",
                        principalTable: "ip_licenses",
                        principalColumn: "ip_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_tokens",
                schema: "public",
                columns: table => new
                {
                    token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    token_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    machine_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issued_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    consumed_by_device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_uses = table.Column<int>(type: "integer", nullable: false),
                    uses_count = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    validity_hours = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollment_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "fk_enrollment_tokens_devices_consumed_by_device_id",
                        column: x => x.consumed_by_device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_enrollment_tokens_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_enrollment_tokens_machines_machine_id",
                        column: x => x.machine_id,
                        principalSchema: "public",
                        principalTable: "machines",
                        principalColumn: "machine_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_sessions",
                schema: "public",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    machine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nonce = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    client_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    telemetry_mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "fk_game_sessions_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_game_sessions_game_modes_mode_id",
                        column: x => x.mode_id,
                        principalSchema: "public",
                        principalTable: "game_modes",
                        principalColumn: "mode_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_game_sessions_machines_machine_id",
                        column: x => x.machine_id,
                        principalSchema: "public",
                        principalTable: "machines",
                        principalColumn: "machine_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "machine_devices",
                schema: "public",
                columns: table => new
                {
                    machine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attached_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    detached_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_machine_devices", x => new { x.machine_id, x.device_id });
                    table.ForeignKey(
                        name: "fk_machine_devices_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_machine_devices_machines_machine_id",
                        column: x => x.machine_id,
                        principalSchema: "public",
                        principalTable: "machines",
                        principalColumn: "machine_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mission_progress",
                schema: "public",
                columns: table => new
                {
                    mission_progress_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    current_value = table.Column<int>(type: "integer", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mission_progress", x => x.mission_progress_id);
                    table.ForeignKey(
                        name: "fk_mission_progress_missions_mission_id",
                        column: x => x.mission_id,
                        principalSchema: "public",
                        principalTable: "missions",
                        principalColumn: "mission_id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "score_entries",
                schema: "public",
                columns: table => new
                {
                    score_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_number = table.Column<int>(type: "integer", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    machine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<long>(type: "bigint", nullable: false),
                    achieved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_validated = table.Column<bool>(type: "boolean", nullable: false),
                    validation_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_score_entries", x => x.score_entry_id);
                    table.ForeignKey(
                        name: "fk_score_entries_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "public",
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_score_entries_game_modes_mode_id",
                        column: x => x.mode_id,
                        principalSchema: "public",
                        principalTable: "game_modes",
                        principalColumn: "mode_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_score_entries_game_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "public",
                        principalTable: "game_sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_score_entries_ip_licenses_ip_id",
                        column: x => x.ip_id,
                        principalSchema: "public",
                        principalTable: "ip_licenses",
                        principalColumn: "ip_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_score_entries_machines_machine_id",
                        column: x => x.machine_id,
                        principalSchema: "public",
                        principalTable: "machines",
                        principalColumn: "machine_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ca_certificates_ca_type_is_active",
                schema: "public",
                table: "ca_certificates",
                columns: new[] { "ca_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_ca_certificates_not_after_utc",
                schema: "public",
                table: "ca_certificates",
                column: "not_after_utc");

            migrationBuilder.CreateIndex(
                name: "ix_ca_certificates_thumbprint_sha256",
                schema: "public",
                table: "ca_certificates",
                column: "thumbprint_sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_certificate_revocations_device_cert_id",
                schema: "public",
                table: "certificate_revocations",
                column: "device_cert_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_certificate_revocations_revoked_at_utc",
                schema: "public",
                table: "certificate_revocations",
                column: "revoked_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_certificate_revocations_revoked_by_user_id",
                schema: "public",
                table: "certificate_revocations",
                column: "revoked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_certificates_ca_id",
                schema: "public",
                table: "device_certificates",
                column: "ca_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_certificates_cert_thumbprint_sha256",
                schema: "public",
                table: "device_certificates",
                column: "cert_thumbprint_sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_certificates_device_id_not_after_utc",
                schema: "public",
                table: "device_certificates",
                columns: new[] { "device_id", "not_after_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_device_certificates_device_id_status",
                schema: "public",
                table: "device_certificates",
                columns: new[] { "device_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_device_certificates_superseded_by_device_cert_id",
                schema: "public",
                table: "device_certificates",
                column: "superseded_by_device_cert_id");

            migrationBuilder.CreateIndex(
                name: "ix_devices_last_seen_at_utc",
                schema: "public",
                table: "devices",
                column: "last_seen_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_devices_public_key_fingerprint",
                schema: "public",
                table: "devices",
                column: "public_key_fingerprint");

            migrationBuilder.CreateIndex(
                name: "ix_devices_serial_physical",
                schema: "public",
                table: "devices",
                column: "serial_physical",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devices_status",
                schema: "public",
                table: "devices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_consumed_by_device_id",
                schema: "public",
                table: "enrollment_tokens",
                column: "consumed_by_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_device_id",
                schema: "public",
                table: "enrollment_tokens",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_issued_by_user_id",
                schema: "public",
                table: "enrollment_tokens",
                column: "issued_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_machine_id",
                schema: "public",
                table: "enrollment_tokens",
                column: "machine_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_token_hash",
                schema: "public",
                table: "enrollment_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_tokens_token_type_expires_at",
                schema: "public",
                table: "enrollment_tokens",
                columns: new[] { "token_type", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_game_modes_code",
                schema: "public",
                table: "game_modes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_modes_is_active",
                schema: "public",
                table: "game_modes",
                column: "is_active");

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

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_device_id_started_at_utc",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "device_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_machine_id_started_at_utc",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "machine_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_mode_id_started_at_utc",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "mode_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_nonce",
                schema: "public",
                table: "game_sessions",
                column: "nonce",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ip_license_regions_region_id",
                schema: "public",
                table: "ip_license_regions",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_ip_licenses_is_active",
                schema: "public",
                table: "ip_licenses",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ip_licenses_name",
                schema: "public",
                table: "ip_licenses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ip_licenses_publisher_id",
                schema: "public",
                table: "ip_licenses",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_ip_licenses_slug",
                schema: "public",
                table: "ip_licenses",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machine_devices_device_id",
                schema: "public",
                table: "machine_devices",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_machine_devices_machine_id_is_active",
                schema: "public",
                table: "machine_devices",
                columns: new[] { "machine_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_machines_idempotency_key",
                schema: "public",
                table: "machines",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machines_ip_id",
                schema: "public",
                table: "machines",
                column: "ip_id");

            migrationBuilder.CreateIndex(
                name: "ix_machines_manufacturer_serial_number",
                schema: "public",
                table: "machines",
                column: "manufacturer_serial_number");

            migrationBuilder.CreateIndex(
                name: "ix_machines_nonce",
                schema: "public",
                table: "machines",
                column: "nonce",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machines_status",
                schema: "public",
                table: "machines",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_machines_world_pinball_serial_number",
                schema: "public",
                table: "machines",
                column: "world_pinball_serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mission_progress_mission_id",
                schema: "public",
                table: "mission_progress",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_mission_progress_status",
                schema: "public",
                table: "mission_progress",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_mission_progress_user_id_mission_id",
                schema: "public",
                table: "mission_progress",
                columns: new[] { "user_id", "mission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mission_progress_user_id_updated_at_utc",
                schema: "public",
                table: "mission_progress",
                columns: new[] { "user_id", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_missions_code",
                schema: "public",
                table: "missions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_missions_ip_id",
                schema: "public",
                table: "missions",
                column: "ip_id");

            migrationBuilder.CreateIndex(
                name: "ix_missions_is_active",
                schema: "public",
                table: "missions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_missions_mode_id",
                schema: "public",
                table: "missions",
                column: "mode_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at_utc",
                schema: "public",
                table: "outbox_messages",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_publishers_name",
                schema: "public",
                table: "publishers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_publishers_slug",
                schema: "public",
                table: "publishers",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regions_code",
                schema: "public",
                table: "regions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_device_id_achieved_at_utc",
                schema: "public",
                table: "score_entries",
                columns: new[] { "device_id", "achieved_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_ip_id_mode_id_score_achieved_at_utc",
                schema: "public",
                table: "score_entries",
                columns: new[] { "ip_id", "mode_id", "score", "achieved_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_machine_id_achieved_at_utc",
                schema: "public",
                table: "score_entries",
                columns: new[] { "machine_id", "achieved_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_score_entries_mode_id",
                schema: "public",
                table: "score_entries",
                column: "mode_id");

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
                name: "ix_score_entries_user_id_achieved_at_utc",
                schema: "public",
                table: "score_entries",
                columns: new[] { "user_id", "achieved_at_utc" });

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

            migrationBuilder.CreateIndex(
                name: "ix_streaming_access_requests_session_id",
                schema: "public",
                table: "streaming_access_requests",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_streaming_partners_user_id",
                schema: "public",
                table: "streaming_partners",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificate_revocations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "enrollment_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "game_session_players",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ip_license_regions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "machine_devices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mission_progress",
                schema: "public");

            migrationBuilder.DropTable(
                name: "operator_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "score_entries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stream_access_grants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stream_subscriptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "streaming_access_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "streaming_partners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "device_certificates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "regions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "missions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "game_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ca_certificates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "game_modes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "machines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ip_licenses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "publishers",
                schema: "public");
        }
    }
}
