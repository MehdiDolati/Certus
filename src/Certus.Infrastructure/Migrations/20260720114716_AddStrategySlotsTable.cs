using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategySlotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StrategySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeightValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StrategyDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategySlots_Strategies_StrategyDefinitionId",
                        column: x => x.StrategyDefinitionId,
                        principalTable: "Strategies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrategySlots_PortfolioId",
                table: "StrategySlots",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySlots_StrategyDefinitionId",
                table: "StrategySlots",
                column: "StrategyDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySlots_StrategyId",
                table: "StrategySlots",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySlots_StrategyId_PortfolioId",
                table: "StrategySlots",
                columns: new[] { "StrategyId", "PortfolioId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategySlots");
        }
    }
}
