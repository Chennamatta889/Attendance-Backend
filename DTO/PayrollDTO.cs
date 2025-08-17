
    public class PayrollPeriodCreateDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class PayrollDetailReadDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public int WorkingDays { get; set; }
        public int Presents { get; set; }
        public int HalfDays { get; set; }
        public int Absents { get; set; }
        public int PayableDays { get; set; }
        public decimal GrossPay { get; set; }
        public decimal AdvanceDeduction { get; set; }
        public decimal NetPay { get; set; }
    }

