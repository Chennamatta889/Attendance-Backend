using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employer> Employers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<EmployeeMonthlyWage> EmployeeMonthlyWages { get; set; }     
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Advance> Advances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Employer with given credentials
            modelBuilder.Entity<Employer>().HasData(new Employer
            {
                EmployerId = 1,
                Name = "Admin",
                Email = "admin@company.com",
                PasswordHash = "Srik@NTh" // ⚠️ Replace later with hashed password for security
            });

            base.OnModelCreating(modelBuilder);
        }
    }
