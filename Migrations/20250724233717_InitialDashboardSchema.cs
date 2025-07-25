using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dashboard.Migrations
{
    /// <inheritdoc />
    public partial class InitialDashboardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ambientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Criticidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Cor = table.Column<string>(type: "text", nullable: false),
                    Peso = table.Column<int>(type: "integer", nullable: false),
                    Downtime = table.Column<bool>(type: "boolean", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criticidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposIncidente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposIncidente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Segmentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    AmbienteId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segmentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Segmentos_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataHoraInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataHoraFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipoIncidenteId = table.Column<int>(type: "integer", nullable: false),
                    AmbienteId = table.Column<int>(type: "integer", nullable: false),
                    SegmentoId = table.Column<int>(type: "integer", nullable: false),
                    CriticidadeId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    AcoesTomadas = table.Column<string>(type: "text", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidentes_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidentes_Criticidades_CriticidadeId",
                        column: x => x.CriticidadeId,
                        principalTable: "Criticidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidentes_Segmentos_SegmentoId",
                        column: x => x.SegmentoId,
                        principalTable: "Segmentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidentes_TiposIncidente_TipoIncidenteId",
                        column: x => x.TipoIncidenteId,
                        principalTable: "TiposIncidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AmbienteId = table.Column<int>(type: "integer", nullable: false),
                    SegmentoId = table.Column<int>(type: "integer", nullable: false),
                    Peso = table.Column<int>(type: "integer", nullable: false),
                    MTTRMetaHoras = table.Column<double>(type: "double precision", nullable: false),
                    SuperacaoMTTR = table.Column<bool>(type: "boolean", nullable: false),
                    MTBFMetaHoras = table.Column<double>(type: "double precision", nullable: false),
                    SuperacaoMTBF = table.Column<bool>(type: "boolean", nullable: false),
                    MTBFMetaDias = table.Column<double>(type: "double precision", nullable: false),
                    DisponibilidadeMeta = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metas_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Metas_Segmentos_SegmentoId",
                        column: x => x.SegmentoId,
                        principalTable: "Segmentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_AmbienteId",
                table: "Incidentes",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_CriticidadeId",
                table: "Incidentes",
                column: "CriticidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_SegmentoId",
                table: "Incidentes",
                column: "SegmentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_TipoIncidenteId",
                table: "Incidentes",
                column: "TipoIncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_AmbienteId",
                table: "Metas",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_SegmentoId",
                table: "Metas",
                column: "SegmentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Segmentos_AmbienteId",
                table: "Segmentos",
                column: "AmbienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidentes");

            migrationBuilder.DropTable(
                name: "Metas");

            migrationBuilder.DropTable(
                name: "Criticidades");

            migrationBuilder.DropTable(
                name: "TiposIncidente");

            migrationBuilder.DropTable(
                name: "Segmentos");

            migrationBuilder.DropTable(
                name: "Ambientes");
        }
    }
}
