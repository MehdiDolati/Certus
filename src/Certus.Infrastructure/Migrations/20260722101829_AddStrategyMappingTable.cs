using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyMappingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StrategyMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StrategyExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StrategyDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyMappings_ConnectionId",
                table: "StrategyMappings",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyMappings_ConnectionId_StrategyExternalId",
                table: "StrategyMappings",
                columns: new[] { "ConnectionId", "StrategyExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StrategyMappings_StrategyDefinitionId",
                table: "StrategyMappings",
                column: "StrategyDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyMappings");
        }
    }
}
