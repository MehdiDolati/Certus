using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEANamesAndFixBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EANames",
                table: "PortfolioDeployments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            // Fix the backfill: clear incorrect slots and re-assign using time-based matching
            migrationBuilder.Sql(@"
                -- Step 1: Clear all existing StrategySlots (the old backfill was incorrect)
                DELETE FROM StrategySlots;

                -- Step 2: Re-assign strategies to portfolios using time-based matching
                -- Strategies created within 60 seconds of a deployment belong to that deployment
                DECLARE @PortfolioId UNIQUEIDENTIFIER;
                DECLARE @EACount INT;
                DECLARE @DeploymentTime DATETIME2;
                DECLARE @Weight DECIMAL(18,6);

                DECLARE deployment_cursor CURSOR FOR
                    SELECT PortfolioId, EACount, CreatedAt FROM PortfolioDeployments
                    WHERE Status NOT IN ('Error', 'Stopped');

                OPEN deployment_cursor;
                FETCH NEXT FROM deployment_cursor INTO @PortfolioId, @EACount, @DeploymentTime;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF @EACount > 0
                    BEGIN
                        SET @Weight = 1.0 / @EACount;

                        -- Find strategies created within 60 seconds of the deployment
                        -- that don't already have a slot for this portfolio
                        INSERT INTO StrategySlots (Id, StrategyId, PortfolioId, WeightValue, AssignedAt)
                        SELECT NEWID(), s.Id, @PortfolioId, @Weight, GETUTCDATE()
                        FROM (
                            SELECT TOP (@EACount) s.Id
                            FROM Strategies s
                            WHERE NOT EXISTS (
                                SELECT 1 FROM StrategySlots ss
                                WHERE ss.StrategyId = s.Id AND ss.PortfolioId = @PortfolioId
                            )
                            AND ABS(DATEDIFF(SECOND, s.CreatedAt, @DeploymentTime)) <= 60
                            ORDER BY s.CreatedAt ASC
                        ) s;
                    END

                    FETCH NEXT FROM deployment_cursor INTO @PortfolioId, @EACount, @DeploymentTime;
                END

                CLOSE deployment_cursor;
                DEALLOCATE deployment_cursor;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EANames",
                table: "PortfolioDeployments");
        }
    }
}
