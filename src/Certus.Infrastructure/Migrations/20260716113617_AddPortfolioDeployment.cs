using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioDeployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioDeployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceFolderPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TargetMT4Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeployedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonitoringStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EACount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioDeployments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioDeployments_ConnectionId",
                table: "PortfolioDeployments",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioDeployments_PortfolioId",
                table: "PortfolioDeployments",
                column: "PortfolioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioDeployments");
        }
    }
}
