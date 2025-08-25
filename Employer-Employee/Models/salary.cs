public class SalaryCarryForward
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public decimal GrossSalary { get; set; }           // Gross for this month
    public decimal AdvanceDeduction { get; set; }      // Total advance deductions applied
    public decimal NetSalary { get; set; }             // Final net salary for this month

    public decimal BalanceAmount { get; set; }         // 🔹 New: Any remaining balance carried to next month (if Net < 0)

    public int Month { get; set; }
    public int Year { get; set; }

    public bool IsSettled { get; set; } = false;
    public DateTime? SettledOn { get; set; }

    public int? SettledInMonth { get; set; }
    public int? SettledInYear { get; set; }
}

public class SalarySettlement
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }

    public decimal TotalGross { get; set; }
    public decimal TotalAdvance { get; set; }
    public decimal TotalNet { get; set; }

    public decimal CarriedForwardBalance { get; set; }   // 🔹 New: Balance left unsettled after settlement

    public DateTime SettledOn { get; set; }


}
