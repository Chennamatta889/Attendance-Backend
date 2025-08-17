public class EmployeeMonthlyWage
{
    public int EmployeeMonthlyWageId { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public decimal DailyWage { get; set; }
}
