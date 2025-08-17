
    public class Salary
    {
        public int SalaryId { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal AdvanceDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? PaidOn { get; set; }

        public Employee? Employee { get; set; }
    }
