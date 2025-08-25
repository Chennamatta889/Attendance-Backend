using System.ComponentModel.DataAnnotations.Schema;

public class Advance
{
    public int AdvanceId { get; set; }
    public int EmployeeId { get; set; }
    public decimal Amount { get; set; }


    [Column(TypeName = "date")]
    public DateOnly DateGiven { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Deducted
}
