using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    // GET all employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var employees = await _context.Employees
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Role = e.Role,

                DailyWage = _context.EmployeeMonthlyWages
                    .Where(w => w.EmployeeId == e.EmployeeId &&
                                w.Month == currentMonth &&
                                w.Year == currentYear)
                    .Select(w => w.DailyWage)
                    .FirstOrDefault(),

                MonthlySalary = (
                    _context.Attendances
                        .Where(a => a.EmployeeId == e.EmployeeId &&
                                    a.Date.Month == currentMonth &&
                                    a.Date.Year == currentYear &&
                                    a.Status == "Present")
                        .Count()
                    +
                    _context.Attendances
                        .Where(a => a.EmployeeId == e.EmployeeId &&
                                    a.Date.Month == currentMonth &&
                                    a.Date.Year == currentYear &&
                                    a.Status == "Half-day")
                        .Count() * 0.5m
                ) *
                (_context.EmployeeMonthlyWages
                    .Where(w => w.EmployeeId == e.EmployeeId &&
                                w.Month == currentMonth &&
                                w.Year == currentYear)
                    .Select(w => w.DailyWage)
                    .FirstOrDefault()
                 )
                -
                (_context.Advances
                    .Where(a => a.EmployeeId == e.EmployeeId &&
                                a.DateGiven.Month == currentMonth &&
                                a.DateGiven.Year == currentYear)
                    .Sum(a => (decimal?)a.Amount) ?? 0),

                JoiningDate = e.DateOfJoining,
                Status = e.Status
            }).ToListAsync();

        return Ok(employees);
    }

    // GET single employee
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var employee = await _context.Employees
            .Where(e => e.EmployeeId == id)
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Role = e.Role,

                DailyWage = _context.EmployeeMonthlyWages
                    .Where(w => w.EmployeeId == e.EmployeeId &&
                                w.Month == currentMonth &&
                                w.Year == currentYear)
                    .Select(w => w.DailyWage)
                    .FirstOrDefault(),

                MonthlySalary = (
                    _context.Attendances
                        .Where(a => a.EmployeeId == e.EmployeeId &&
                                    a.Date.Month == currentMonth &&
                                    a.Date.Year == currentYear &&
                                    a.Status == "Present")
                        .Count()
                    +
                    _context.Attendances
                        .Where(a => a.EmployeeId == e.EmployeeId &&
                                    a.Date.Month == currentMonth &&
                                    a.Date.Year == currentYear &&
                                    a.Status == "Half-day")
                        .Count() * 0.5m
                ) *
                (_context.EmployeeMonthlyWages
                    .Where(w => w.EmployeeId == e.EmployeeId &&
                                w.Month == currentMonth &&
                                w.Year == currentYear)
                    .Select(w => w.DailyWage)
                    .FirstOrDefault()
                 )
                -
                (_context.Advances
                    .Where(a => a.EmployeeId == e.EmployeeId &&
                                a.DateGiven.Month == currentMonth &&
                                a.DateGiven.Year == currentYear)
                    .Sum(a => (decimal?)a.Amount) ?? 0),

                JoiningDate = e.DateOfJoining,
                Status = e.Status
            }).FirstOrDefaultAsync();

        if (employee == null) return NotFound();
        return Ok(employee);
    }

    // CREATE employee
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            DailyWage  = dto.DailyWage,
            DateOfJoining = dto.JoiningDate,
            Status = "Active"
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Record monthly wage for joining month
        var wageRecord = new EmployeeMonthlyWage
        {
            EmployeeId = employee.EmployeeId,
            DailyWage = dto.DailyWage,
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        };
        _context.EmployeeMonthlyWages.Add(wageRecord);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, dto);
    }

    // UPDATE employee
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        employee.Name = dto.Name ?? employee.Name;
        employee.Phone = dto.Phone ?? employee.Phone;
        employee.Role = dto.Role ?? employee.Role;
        employee.Status = dto.Status ?? employee.Status;

        if (dto.DailyWage.HasValue)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var existingWage = await _context.EmployeeMonthlyWages
                .FirstOrDefaultAsync(w => w.EmployeeId == id && w.Month == currentMonth && w.Year == currentYear);

            if (existingWage != null)
            {
                existingWage.DailyWage = dto.DailyWage.Value;
                _context.Entry(existingWage).State = EntityState.Modified;
            }
            else
            {
                _context.EmployeeMonthlyWages.Add(new EmployeeMonthlyWage
                {
                    EmployeeId = id,
                    DailyWage = dto.DailyWage.Value,
                    Month = currentMonth,
                    Year = currentYear
                });
            }
            employee.DailyWage = dto.DailyWage.Value;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE employee
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
