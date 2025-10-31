using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DengueAlert.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DengueAlerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GeoCode = table.Column<int>(type: "int", nullable: false),
                    SemanaEpidemiologica = table.Column<int>(type: "int", nullable: false),
                    EndWeek = table.Column<int>(type: "int", nullable: false),
                    EndYear = table.Column<int>(type: "int", nullable: false),
                    DataIniSE = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CasosEst = table.Column<double>(type: "double", nullable: true),
                    CasosEstMin = table.Column<double>(type: "double", nullable: true),
                    CasosEstMax = table.Column<double>(type: "double", nullable: true),
                    Casos = table.Column<int>(type: "int", nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: true),
                    PRt1 = table.Column<double>(type: "double", nullable: true),
                    PInc100k = table.Column<double>(type: "double", nullable: true),
                    VersaoModelo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rt = table.Column<double>(type: "double", nullable: true),
                    Pop = table.Column<int>(type: "int", nullable: true),
                    TempMin = table.Column<double>(type: "double", nullable: true),
                    TempMed = table.Column<double>(type: "double", nullable: true),
                    TempMax = table.Column<double>(type: "double", nullable: true),
                    UmidMin = table.Column<double>(type: "double", nullable: true),
                    UmidMed = table.Column<double>(type: "double", nullable: true),
                    UmidMax = table.Column<double>(type: "double", nullable: true),
                    Receptivo = table.Column<int>(type: "int", nullable: true),
                    Transmissao = table.Column<int>(type: "int", nullable: true),
                    NivelInc = table.Column<int>(type: "int", nullable: true),
                    CasProv = table.Column<int>(type: "int", nullable: true),
                    NotifAccumYear = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DengueAlerts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DengueAlerts_EndWeek_EndYear_GeoCode",
                table: "DengueAlerts",
                columns: new[] { "EndWeek", "EndYear", "GeoCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DengueAlerts");
        }
    }
}
