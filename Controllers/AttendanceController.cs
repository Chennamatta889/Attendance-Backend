using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    // Get all attendance records
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceReadDto>>> GetAll()
    {
        var records = await _context.Attendances
            .Include(a => a.Employee)
            .Select(a => new AttendanceReadDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();

        return Ok(records);
    }

    // Get attendance for a specific employee
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<AttendanceReadDto>>> GetByEmployee(int employeeId)
    {
        var records = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Employee)
            .Select(a => new AttendanceReadDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();

        return Ok(records);
    }

    // Create or update attendance with manual date
    [HttpPost]
    public async Task<ActionResult> CreateOrUpdateAttendance(AttendanceCreateDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return NotFound(new { Message = "Employee not found" });

        var attendanceDate = dto.Date.Date;

        // Check if attendance already exists for this employee & date
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == attendanceDate);

        if (attendance == null)
        {
            // Create new attendance
            attendance = new Attendance
            {
                EmployeeId = dto.EmployeeId,
                Date = attendanceDate,
                Status = dto.Status ?? "Present"
            };
            _context.Attendances.Add(attendance);
        }
        else
        {
            // Update existing attendance
            attendance.Status = dto.Status ?? attendance.Status;
        }

        await _context.SaveChangesAsync();

        // Update monthly salary based on attendance date
        await UpdateMonthlySalary(employee.EmployeeId, attendanceDate.Month, attendanceDate.Year);

        return Ok(new { Message = "Attendance recorded and salary updated successfully" });
    }

    // Update salary for the given month & year
    private async Task UpdateMonthlySalary(int employeeId, int month, int year)
    {
        var monthlyWage = await _context.EmployeeMonthlyWages
            .Where(w => w.EmployeeId == employeeId && w.Year == year && w.Month == month)
            .Select(w => w.DailyWage)
            .FirstOrDefaultAsync();

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return;

        var dailyWageToUse = monthlyWage != 0 ? monthlyWage : employee.DailyWage;

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
            .ToListAsync();

        int presentDays = attendances.Count(a => a.Status == "Present");
        int halfDays = attendances.Count(a => a.Status == "Half-day");

        decimal grossSalary = (presentDays + 0.5m * halfDays) * dailyWageToUse;

        var totalAdvances = await _context.Advances
            .Where(a => a.EmployeeId == employeeId && a.DateGiven.Month == month && a.DateGiven.Year == year)
            .SumAsync(a => (decimal?)a.Amount) ?? 0m;

        employee.MonthlySalary = grossSalary - totalAdvances;
        await _context.SaveChangesAsync();
    }
    // Get monthly attendance for a specific employee
    [HttpGet("monthly/{employeeId}")]
    public async Task<ActionResult<IEnumerable<AttendanceReadDto>>> GetMonthlyAttendance(int employeeId)
    {
        var today = DateTime.Now;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        var records = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                        a.Date.Month == currentMonth &&
                        a.Date.Year == currentYear)
            .Include(a => a.Employee)
            .Select(a => new AttendanceReadDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("monthly-report/{employeeId}/{month}/{year}")]
    
    public async Task<ActionResult<MonthlySalaryDto>> GetMonthlyReport(int employeeId, int month, int year)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        var firstOfMonth = new DateTime(year, month, 1);

        // If employee joined after this month, return all zeros
        if (employee.DateOfJoining > firstOfMonth.AddMonths(1).AddDays(-1))
        {
            return Ok(new MonthlySalaryDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.Name,
                PresentDays = 0,
                HalfDays = 0,
                Absents = 0,
                GrossSalary = 0,
                AdvanceDeduction = 0,
                NetSalary = 0
            });
        }

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                        a.Date.Month == month &&
                        a.Date.Year == year)
            .ToListAsync();

        int presentDays = attendances.Count(a => a.Status == "Present");
        int halfDays = attendances.Count(a => a.Status == "Half-day");
        int absents = attendances.Count(a => a.Status == "Absent" || a.Status == "Leave");

        decimal grossSalary = presentDays * employee.DailyWage + halfDays * employee.DailyWage * 0.5m;

        var totalAdvance = await _context.Advances
            .Where(a => a.EmployeeId == employeeId &&
                        a.DateGiven.Month == month &&
                        a.DateGiven.Year == year)
            .SumAsync(a => (decimal?)a.Amount) ?? 0m;

        return Ok(new MonthlySalaryDto
        {
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.Name,
            PresentDays = presentDays,
            HalfDays = halfDays,
            Absents = absents,
            GrossSalary = grossSalary,
            AdvanceDeduction = totalAdvance,
            NetSalary = grossSalary - totalAdvance
        });
    }
    [HttpGet("monthly-report/{employeeId}")]
    public async Task<IActionResult> GetMonthlyReportPdf(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        // Fetch Attendance
        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                        a.Date.Month == currentMonth &&
                        a.Date.Year == currentYear)
            .ToListAsync();

        int presentDays = attendances.Count(a => a.Status == "Present");
        int halfDays = attendances.Count(a => a.Status == "Half-day");
        int absents = attendances.Count(a => a.Status == "Absent" || a.Status == "Leave");

        // Fetch Advances
        var advances = await _context.Advances
            .Where(a => a.EmployeeId == employeeId &&
                        a.DateGiven.Month == currentMonth &&
                        a.DateGiven.Year == currentYear)
            .ToListAsync();

        decimal totalAdvance = advances.Sum(a => a.Amount);
        decimal grossSalary = presentDays * employee.DailyWage + halfDays * employee.DailyWage * 0.5m;
        decimal netSalary = grossSalary - totalAdvance;

        var pdfBytes = ReportGenerator.GenerateFullPdf(employee, attendances, advances, grossSalary, totalAdvance, netSalary);

        return File(pdfBytes, "application/pdf", $"MonthlyReport_{employee.Name}_{currentMonth}_{currentYear}.pdf");
    }




}
