public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "";
    public decimal DailyWage { get; set; }      // Now used instead of FixedSalary
    public decimal MonthlySalary { get; set; }
    public DateTime JoiningDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class CreateEmployeeDto
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "";
    public decimal DailyWage { get; set; }     // Employer sets this
    public DateTime JoiningDate { get; set; }
}

public class UpdateEmployeeDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public decimal? DailyWage { get; set; }    // Optional update of wage
    public string? Status { get; set; }
}
