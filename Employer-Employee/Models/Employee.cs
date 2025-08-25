using System.ComponentModel.DataAnnotations;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    [Required]
    public decimal DailyWage { get; set; } // <-- Employee-specific daily wage

    public decimal MonthlySalary { get; set; } // Computed

    [Required]
    public string Status { get; set; } = "Active";

    [Required]
    public DateTime DateOfJoining { get; set; }

    public ICollection<Advance> Advances { get; set; } = new List<Advance>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
