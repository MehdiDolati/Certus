using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillStrategySlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill StrategySlots for existing strategies that were created by deployments
            // but never had their slots persisted (the bug we just fixed).
            //
            // Strategy: For each deployment, find strategies without slots and assign them
            // with equal weight based on the deployment's EA count.

            migrationBuilder.Sql(@"
                -- For each deployment, assign unassigned strategies to its portfolio
                DECLARE @PortfolioId UNIQUEIDENTIFIER;
                DECLARE @EACount INT;
                DECLARE @Weight DECIMAL(18,6);

                DECLARE deployment_cursor CURSOR FOR
                    SELECT PortfolioId, EACount FROM PortfolioDeployments
                    WHERE Status NOT IN ('Error', 'Stopped');

                OPEN deployment_cursor;
                FETCH NEXT FROM deployment_cursor INTO @PortfolioId, @EACount;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF @EACount > 0
                    BEGIN
                        SET @Weight = 1.0 / @EACount;

                        INSERT INTO StrategySlots (Id, StrategyId, PortfolioId, WeightValue, AssignedAt)
                        SELECT NEWID(), s.Id, @PortfolioId, @Weight, GETUTCDATE()
                        FROM (
                            SELECT TOP (@EACount) s.Id
                            FROM Strategies s
                            WHERE NOT EXISTS (
                                SELECT 1 FROM StrategySlots ss
                                WHERE ss.StrategyId = s.Id AND ss.PortfolioId = @PortfolioId
                            )
                            ORDER BY s.CreatedAt ASC
                        ) s;
                    END

                    FETCH NEXT FROM deployment_cursor INTO @PortfolioId, @EACount;
                END

                CLOSE deployment_cursor;
                DEALLOCATE deployment_cursor;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
