public class AttendanceCreateDto
{
    public int EmployeeId { get; set; }
    public string? Status { get; set; } = "Present";
    public DateTime Date { get; set; }  // Will show actual attendance date

}

public class AttendanceReadDto
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    public DateTime Date { get; set; }  // Will show actual attendance date


    public string Status { get; set; } = "Present";
}

public class MonthlySalaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    public int PresentDays { get; set; }
    public int HalfDays { get; set; }
    public int Absents { get; set; }

    public decimal GrossSalary { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal NetSalary { get; set; }
}
