using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AdvanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdvanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdvanceReadDto>>> GetAll()
    {
        var advances = await _context.Advances
            .Select(a => new AdvanceReadDto
            {
                AdvanceId = a.AdvanceId,
                EmployeeId = a.EmployeeId,
                Amount = a.Amount,
                DateGiven = a.DateGiven,
                Reason = a.Reason,
                ReferenceNo = a.ReferenceNo,
                Status = a.Status
            })
            .ToListAsync();

        return Ok(advances);
    }


    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<AdvanceReadDto>>> GetByEmployee(int employeeId)
    {
        var records = await _context.Advances
            .Where(a => a.EmployeeId == employeeId)
            .Select(a => new AdvanceReadDto
            {
                AdvanceId = a.AdvanceId,
                EmployeeId = a.EmployeeId,
                DateGiven  = a.DateGiven,
                Amount = a.Amount,
                Reason = a.Reason,
                Status = a.Status
            }).ToListAsync();

        return Ok(records);
    }

    [HttpPost]
    public async Task<ActionResult> Create(AdvanceCreateDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return NotFound(new { Message = "Employee not found" });

      

        var advance = new Advance
        {
            EmployeeId = dto.EmployeeId,
            Amount = dto.Amount,
            DateGiven = dto.DateGiven,
            Reason = dto.Reason,
            ReferenceNo = dto.ReferenceNo,
            Status = "Pending"
        };

        _context.Advances.Add(advance);
        await _context.SaveChangesAsync();

        // ✅ Salary update logic (same as before)
        var totalAdvancesThisMonth = await _context.Advances
            .Where(a => a.EmployeeId == employee.EmployeeId &&
                        a.DateGiven.Month == DateTime.Now.Month &&
                        a.DateGiven.Year == DateTime.Now.Year)
            .SumAsync(a => a.Amount);

        var totalWorkedDays = await _context.Attendances
            .Where(a => a.EmployeeId == employee.EmployeeId &&
                        a.Date.Month == DateTime.Now.Month &&
                        a.Date.Year == DateTime.Now.Year)
            .CountAsync(a => a.Status == "Present") +
                              (await _context.Attendances
                                .Where(a => a.EmployeeId == employee.EmployeeId &&
                                            a.Date.Month == DateTime.Now.Month &&
                                            a.Date.Year == DateTime.Now.Year)
                                .CountAsync(a => a.Status == "Half-day")) * 0.5m;

        employee.MonthlySalary = (totalWorkedDays * employee.DailyWage) - totalAdvancesThisMonth;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Advance recorded successfully, salary updated" });
    }

}
