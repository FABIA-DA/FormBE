using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FormBE.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "FormBE");

            migrationBuilder.CreateTable(
                name: "field_group",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_type",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    regex = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_group_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "FormBE",
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "single_choice_field",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_single_choice_field", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    field_type_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_optional = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_field_type_field_type_id",
                        column: x => x.field_type_id,
                        principalSchema: "FormBE",
                        principalTable: "field_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_group_group_id",
                        column: x => x.group_id,
                        principalSchema: "FormBE",
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "field_group_single_choice_field",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    field_group_id = table.Column<int>(type: "integer", nullable: false),
                    single_choice_field_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_group_single_choice_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_group_single_choice_field_field_group_field_group_id",
                        column: x => x.field_group_id,
                        principalSchema: "FormBE",
                        principalTable: "field_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_field_group_single_choice_field_single_choice_field_single_",
                        column: x => x.single_choice_field_id,
                        principalSchema: "FormBE",
                        principalTable: "single_choice_field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    single_choice_field_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_single_choice_field_single_choice_field_id",
                        column: x => x.single_choice_field_id,
                        principalSchema: "FormBE",
                        principalTable: "single_choice_field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field_group_field",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    field_group_id = table.Column<int>(type: "integer", nullable: false),
                    field_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_group_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_group_field_field_field_id",
                        column: x => x.field_id,
                        principalSchema: "FormBE",
                        principalTable: "field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_field_group_field_field_group_field_group_id",
                        column: x => x.field_group_id,
                        principalSchema: "FormBE",
                        principalTable: "field_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field_response",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    field_id = table.Column<int>(type: "integer", nullable: false),
                    telephone_number = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_response", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_response_field_field_id",
                        column: x => x.field_id,
                        principalSchema: "FormBE",
                        principalTable: "field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "form_field_group",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    form_id = table.Column<int>(type: "integer", nullable: false),
                    field_group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_field_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_field_group_field_group_field_group_id",
                        column: x => x.field_group_id,
                        principalSchema: "FormBE",
                        principalTable: "field_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_field_group_form_form_id",
                        column: x => x.form_id,
                        principalSchema: "FormBE",
                        principalTable: "form",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option_field",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    option_id = table.Column<int>(type: "integer", nullable: false),
                    field_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_field_field_field_id",
                        column: x => x.field_id,
                        principalSchema: "FormBE",
                        principalTable: "field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_option_field_option_option_id",
                        column: x => x.option_id,
                        principalSchema: "FormBE",
                        principalTable: "option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option_response",
                schema: "FormBE",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    option_id = table.Column<int>(type: "integer", nullable: false),
                    telephone_number = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_response", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_response_option_option_id",
                        column: x => x.option_id,
                        principalSchema: "FormBE",
                        principalTable: "option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_field_field_type_id",
                schema: "FormBE",
                table: "field",
                column: "field_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_group_field_field_group_id",
                schema: "FormBE",
                table: "field_group_field",
                column: "field_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_group_field_field_id",
                schema: "FormBE",
                table: "field_group_field",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_group_single_choice_field_field_group_id",
                schema: "FormBE",
                table: "field_group_single_choice_field",
                column: "field_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_group_single_choice_field_single_choice_field_id",
                schema: "FormBE",
                table: "field_group_single_choice_field",
                column: "single_choice_field_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_response_field_id",
                schema: "FormBE",
                table: "field_response",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_group_id",
                schema: "FormBE",
                table: "form",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_field_group_field_group_id",
                schema: "FormBE",
                table: "form_field_group",
                column: "field_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_field_group_form_id",
                schema: "FormBE",
                table: "form_field_group",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_parent_id",
                schema: "FormBE",
                table: "group",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_single_choice_field_id",
                schema: "FormBE",
                table: "option",
                column: "single_choice_field_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_field_field_id",
                schema: "FormBE",
                table: "option_field",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_field_option_id",
                schema: "FormBE",
                table: "option_field",
                column: "option_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_response_option_id",
                schema: "FormBE",
                table: "option_response",
                column: "option_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_group_field",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "field_group_single_choice_field",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "field_response",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "form_field_group",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "option_field",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "option_response",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "field_group",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "form",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "field",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "option",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "group",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "field_type",
                schema: "FormBE");

            migrationBuilder.DropTable(
                name: "single_choice_field",
                schema: "FormBE");
        }
    }
}
