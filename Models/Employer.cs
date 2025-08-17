  public class Employer
    {
        public int EmployerId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public ICollection<Employee>? Employees { get; set; }
    }
