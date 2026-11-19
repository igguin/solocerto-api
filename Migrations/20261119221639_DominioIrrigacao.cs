using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloCerto.Api.Migrations
{
    /// <inheritdoc />
    public partial class DominioIrrigacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Agricultores",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Plantacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgricultorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cultura = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    AreaHectares = table.Column<double>(type: "REAL", nullable: false),
                    UmidadeCritica = table.Column<double>(type: "REAL", nullable: false),
                    UmidadeIdeal = table.Column<double>(type: "REAL", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plantacoes_Agricultores_AgricultorId",
                        column: x => x.AgricultorId,
                        principalTable: "Agricultores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicoesSolo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UmidadePercentual = table.Column<double>(type: "REAL", nullable: false),
                    MedidoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoesSolo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicoesSolo_Plantacoes_PlantacaoId",
                        column: x => x.PlantacaoId,
                        principalTable: "Plantacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrevisoesChuva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Probabilidade = table.Column<double>(type: "REAL", nullable: false),
                    ValidaAPartirDe = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fonte = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisoesChuva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisoesChuva_Plantacoes_PlantacaoId",
                        column: x => x.PlantacaoId,
                        principalTable: "Plantacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecomendacoesIrrigacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicaoSoloId = table.Column<int>(type: "INTEGER", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    RegraAplicada = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Justificativa = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    UmidadeConsiderada = table.Column<double>(type: "REAL", nullable: true),
                    ProbabilidadeChuvaConsiderada = table.Column<double>(type: "REAL", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecomendacoesIrrigacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecomendacoesIrrigacao_MedicoesSolo_MedicaoSoloId",
                        column: x => x.MedicaoSoloId,
                        principalTable: "MedicoesSolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecomendacoesIrrigacao_Plantacoes_PlantacaoId",
                        column: x => x.PlantacaoId,
                        principalTable: "Plantacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agricultores_Cidade",
                table: "Agricultores",
                column: "Cidade");

            migrationBuilder.CreateIndex(
                name: "IX_MedicoesSolo_PlantacaoId_MedidoEm",
                table: "MedicoesSolo",
                columns: new[] { "PlantacaoId", "MedidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Plantacoes_AgricultorId_Cultura",
                table: "Plantacoes",
                columns: new[] { "AgricultorId", "Cultura" });

            migrationBuilder.CreateIndex(
                name: "IX_PrevisoesChuva_PlantacaoId_ValidaAPartirDe",
                table: "PrevisoesChuva",
                columns: new[] { "PlantacaoId", "ValidaAPartirDe" });

            migrationBuilder.CreateIndex(
                name: "IX_RecomendacoesIrrigacao_MedicaoSoloId",
                table: "RecomendacoesIrrigacao",
                column: "MedicaoSoloId");

            migrationBuilder.CreateIndex(
                name: "IX_RecomendacoesIrrigacao_PlantacaoId_CriadoEm",
                table: "RecomendacoesIrrigacao",
                columns: new[] { "PlantacaoId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_RecomendacoesIrrigacao_Tipo",
                table: "RecomendacoesIrrigacao",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrevisoesChuva");

            migrationBuilder.DropTable(
                name: "RecomendacoesIrrigacao");

            migrationBuilder.DropTable(
                name: "MedicoesSolo");

            migrationBuilder.DropTable(
                name: "Plantacoes");

            migrationBuilder.DropIndex(
                name: "IX_Agricultores_Cidade",
                table: "Agricultores");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Agricultores");
        }
    }
}
