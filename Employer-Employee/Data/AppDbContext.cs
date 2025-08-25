using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employer> Employers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<EmployeeMonthlyWage> EmployeeMonthlyWages { get; set; }     
        public DbSet<Advance> Advances { get; set; }
        public DbSet<SalaryCarryForward> SalaryCarryForwards { get; set; }  
        public DbSet<SalarySettlement> SalarySettlements { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
                base.OnModelCreating(modelBuilder);

        // ✅ Hash the admin password
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("M@hesh9390");

        // Seed Employer with given credentials
        modelBuilder.Entity<Employer>().HasData(new Employer
            {
                EmployerId = 1,
                Name = "Admin",
                Email = "admin@srirudrainfradevelopers.in",
                PasswordHash = adminPasswordHash // ⚠️ Replace later with hashed password for security
        });

            base.OnModelCreating(modelBuilder);
        }
    }
