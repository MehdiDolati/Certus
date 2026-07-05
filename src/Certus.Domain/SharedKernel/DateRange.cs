namespace Certus.Domain.SharedKernel;

public record DateRange(DateTime? From, DateTime? To)
{
    public static DateRange Last30Days => new(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
    public static DateRange Last90Days => new(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
    public static DateRange YearToDate => new(new DateTime(DateTime.UtcNow.Year, 1, 1), DateTime.UtcNow);
    public static DateRange Last1Year => new(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
    public static DateRange All => new(null, null);
}
