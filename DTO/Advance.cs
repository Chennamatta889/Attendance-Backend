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
