public class AdvanceCreateDto
{
    public int EmployeeId { get; set; }
    public decimal Amount { get; set; }

    public DateOnly DateGiven {  get; set; }
    
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
}

public class AdvanceReadDto
{
    public int AdvanceId { get; set; }
    public int EmployeeId { get; set; }
    public decimal Amount { get; set; }

    public DateOnly DateGiven { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
    public string Status { get; set; } = "Pending";


}


public class MonthlySalaDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int PresentDays { get; set; }
    public int HalfDays { get; set; }
    public int Absents { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public decimal DailyWage { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal CarryForwardBalance { get; set; }

    public List<AdvanceReadDto> Advances { get; set; } = new List<AdvanceReadDto>();
    public int? settledMonth { get; set; }

    public int? settledYear { get;set; }
    public decimal TotalAmount { get;  set; }
}
