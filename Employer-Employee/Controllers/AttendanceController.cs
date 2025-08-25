using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

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

    // Create or update attendance
    [HttpPost]
    public async Task<ActionResult> CreateOrUpdateAttendance(AttendanceCreateDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return NotFound(new { Message = "Employee not found" });

        var attendanceDate = dto.Date.Date;

        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == attendanceDate);

        if (attendance == null)
        {
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
            attendance.Status = dto.Status ?? attendance.Status;
        }

        await _context.SaveChangesAsync();

        // Update monthly salary after attendance change
        await UpdateMonthlySalary(employee.EmployeeId, attendanceDate.Month, attendanceDate.Year);

        return Ok(new { Message = "Attendance recorded and salary updated successfully" });
    }

    // Update salary including carry forwards
    private async Task UpdateMonthlySalary(int employeeId, int month, int year)
    
    {
        
        
       var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return;

        var monthlyWage = await _context.EmployeeMonthlyWages
            .Where(w => w.EmployeeId == employeeId && w.Year == year && w.Month == month)
            .Select(w => w.DailyWage)
            .FirstOrDefaultAsync();

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

        // Sum of all previous unsettled carry forwards
        //var previousBalance = await _context.SalaryCarryForwards
        //    .Where(c => c.EmployeeId == employeeId && !c.IsSettled &&
        //                (c.Year < year || (c.Year == year && c.Month < month)))
        //    .SumAsync(c => c.BalanceAmount);

        var previousMonth = month == 1 ? 12 : month - 1;
        var previousYear = month == 1 ? year - 1 : year;

        var previousCarry = await _context.SalaryCarryForwards
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId
                                   && !c.IsSettled
                                   && c.Year == previousYear
                                   && c.Month == previousMonth);

        decimal previousBalance = previousCarry?.BalanceAmount ?? 0m;

        decimal netSalary = grossSalary - totalAdvances ;

        // Check if a carry forward record already exists for this month/year
        var existingCarry = await _context.SalaryCarryForwards
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Month == month && c.Year == year);

        if (existingCarry != null)
        {
            // Update existing record
            existingCarry.GrossSalary = grossSalary;
            existingCarry.AdvanceDeduction = totalAdvances;
            existingCarry.NetSalary = netSalary ;
            existingCarry.BalanceAmount = netSalary + previousBalance;
        }
        else
        {
            // Create new record if it doesn't exist
            var carryForwardRecord = new SalaryCarryForward
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                GrossSalary = grossSalary,
                AdvanceDeduction = totalAdvances ,
                NetSalary = netSalary,
                BalanceAmount = netSalary,
                IsSettled = false
            };
            _context.SalaryCarryForwards.Add(carryForwardRecord);
        }

        // Update employee monthly salary
        employee.MonthlySalary = netSalary;

        await _context.SaveChangesAsync();
    }


    [HttpGet("monthly-report/{employeeId}/{month}/{year}")]
    public async Task<ActionResult<MonthlySalaDto>> GetMonthlyReport(int employeeId, int month, int year)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
            .ToListAsync();

        int presentDays = attendances.Count(a => a.Status == "Present");
        int halfDays = attendances.Count(a => a.Status == "Half-day");
        int absents = attendances.Count(a => a.Status == "Absent" || a.Status == "Leave");

        decimal grossSalary = presentDays * employee.DailyWage + halfDays * employee.DailyWage * 0.5m;

        var advances = await _context.Advances
            .Where(a => a.EmployeeId == employeeId && a.DateGiven.Month == month && a.DateGiven.Year == year)
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

        decimal totalAdvance = advances.Sum(a => a.Amount);

        // 🔹 Sum of previous unsettled carry forwards
        //        decimal previousBalance = await _context.SalaryCarryForwards
        //            .Where(c => c.EmployeeId == employeeId && !c.IsSettled &&
        //(c.Year < year || (c.Year == year && c.Month < month))).SumAsync(c => c.BalanceAmount);

        var previousMonth = month == 1 ? 12 : month - 1;
        var previousYear = month == 1 ? year - 1 : year;

        var previousCarry = await _context.SalaryCarryForwards
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId
                                   && !c.IsSettled
                                   && c.Year == previousYear
                                   && c.Month == previousMonth);

        decimal previousBalance = previousCarry?.BalanceAmount ?? 0m;

        decimal netSalary = grossSalary - totalAdvance ;

        decimal grossAmount = netSalary + previousBalance ;

        var settlement = await _context.SalaryCarryForwards
        .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.SettledInMonth == month && x.SettledInYear == year);


        var result = new MonthlySalaDto
        {
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.Name,
            PresentDays = presentDays,
            HalfDays = halfDays,
            Absents = absents,
            GrossSalary = grossSalary,
            AdvanceDeduction = totalAdvance,
            CarryForwardBalance = previousBalance,
            NetSalary = netSalary,
            settledMonth = settlement?.SettledInMonth,
            settledYear = settlement?.SettledInYear,
            DailyWage = employee.DailyWage,
            TotalAmount = grossAmount,
            Month = month,
            Year = year,
            Advances = advances
        };

        return Ok(result);
        }

    [HttpGet("yearly-report/{employeeId}/{year}")]
    public async Task<IActionResult> GetYearlyReport(int employeeId, int year)
    {
        // Fetch salary records for the year
        var salaryRecords = await _context.SalaryCarryForwards
            .Where(x => x.EmployeeId == employeeId && x.Year == year)
            .OrderBy(x => x.Month)
            .ToListAsync();

        if (!salaryRecords.Any())
            return NotFound("No salary records found for this employee in given year.");

        // Employee details
        var employee = await _context.Employees.FindAsync(employeeId);

        // Build response
        var monthlyReports = salaryRecords.Select(s => new
        {
            month = s.Month,
            grossSalary = s.GrossSalary,
            advanceDeduction = s.AdvanceDeduction,
            netSalary = s.NetSalary,
            totalAmount = s.NetSalary + s.BalanceAmount,  // include carry forward if applicable
            isSettled = s.IsSettled,
            settledOn = s.SettledOn,
            settledInMonth = s.SettledInMonth,
            settledInYear = s.SettledInYear
        }).ToList();

        var response = new
        {
            employeeName = employee?.Name,
            year = year,
            monthlyReports = monthlyReports,
            totalGross = monthlyReports.Sum(m => m.grossSalary),
            totalAdvances = monthlyReports.Sum(m => m.advanceDeduction),
            totalNet = monthlyReports.Sum(m => m.netSalary),
            totalAmount = monthlyReports.Sum(m => m.totalAmount)
        };

        return Ok(response);
    }




    [HttpPost("settle/{employeeId}/{month}/{year}")]
    public async Task<IActionResult> SettleSalary(int employeeId, int month, int year)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        // Get all unsettled salaries up to selected month
        var unsettled = await _context.SalaryCarryForwards
            .Where(c => c.EmployeeId == employeeId &&
                        !c.IsSettled &&
                        (c.Year < year || (c.Year == year && c.Month <= month)))
            .OrderBy(c => c.Year).ThenBy(c => c.Month)
            .ToListAsync();

        if (!unsettled.Any())
            return BadRequest(new { message = "Nothing to settle up to this month" });

        decimal totalGross = unsettled.Sum(s => s.GrossSalary);
        decimal totalAdvance = unsettled.Sum(s => s.AdvanceDeduction);
        decimal totalNet = unsettled.Sum(s => s.NetSalary);

        foreach (var record in unsettled)
        {
            record.IsSettled = true;
            record.SettledOn = DateTime.Now;
            record.SettledInMonth = month;
            record.SettledInYear = year;
            record.BalanceAmount = 0;
        }

        var settlement = new SalarySettlement
        {
            EmployeeId = employeeId,
            TotalGross = totalGross,
            TotalAdvance = totalAdvance,
            TotalNet = totalNet,
            CarriedForwardBalance = 0,
            SettledOn = DateTime.Now
        };

        _context.SalarySettlements.Add(settlement);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Salary settled successfully",
            employee = employee.Name,
            totalGross,
            totalAdvance,
            totalNet,
            settledUpTo = $"{month}/{year}"
        });
    }

    [HttpPost("carryforward/{employeeId}/{month}/{year}")]
    public async Task<IActionResult> CarryForwardSalary(int employeeId, int month, int year)
    {
        var carry = await _context.SalaryCarryForwards
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Month == month && c.Year == year);

        if (carry == null)
            return NotFound(new { message = "No record found" });

        if (carry.IsSettled)
            return BadRequest(new { message = "Already settled, cannot carry forward" });

        var nextMonth = month == 12 ? 1 : month + 1;
        var nextYear = month == 12 ? year + 1 : year;

        var nextCarry = await _context.SalaryCarryForwards
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Month == nextMonth && c.Year == nextYear);

        if (nextCarry != null)
        {
            // Add carry forward into next month’s balance & net salary
            nextCarry.BalanceAmount += carry.BalanceAmount;
            nextCarry.NetSalary += carry.BalanceAmount;
        }
        else
        {
            nextCarry = new SalaryCarryForward
            {
                EmployeeId = employeeId,
                Month = nextMonth,
                Year = nextYear,
                GrossSalary = 0, // if not worked yet
                NetSalary = carry.BalanceAmount,
                BalanceAmount = carry.BalanceAmount,
                IsSettled = false
            };
            _context.SalaryCarryForwards.Add(nextCarry);
        }

        // Reset current month balance (but keep gross/net for history)
        carry.BalanceAmount = 0;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Carried forward successfully",
            fromMonth = carry.Month,
            toMonth = nextCarry.Month,
            toYear = nextCarry.Year,
            carriedAmount = nextCarry.BalanceAmount
        });
    }


    [HttpGet("salary-summary/{employeeId}")]
    public async Task<IActionResult> GetSalarySummary(int employeeId)
    {
        var carryForwards = await _context.SalaryCarryForwards
            .Where(c => c.EmployeeId == employeeId)
            .ToListAsync();

        if (!carryForwards.Any())
            return NotFound("No salary records found for this employee");

        var totalAdvance = carryForwards.Sum(c => c.AdvanceDeduction);
        var totalSettled = carryForwards.Where(c => c.IsSettled).Sum(c => c.NetSalary);
        var totalCarryForward = carryForwards.Where(c => !c.IsSettled).Sum(c => c.BalanceAmount);
        var totalGross = carryForwards.Sum(c => c.GrossSalary);
        var totalPending = totalGross - totalSettled - totalAdvance - totalCarryForward;

        return Ok(new
        {
            employeeId,
            totalGross,
            totalAdvance,
            totalSettled,
            totalCarryForward,
            totalPending
        });
    }

    [HttpGet("salary-summary")]
    public async Task<IActionResult> GetSalarySummary()
    {
        var salaries = await _context.SalaryCarryForwards.ToListAsync();

        var advanceTotal = salaries.Sum(s => s.AdvanceDeduction);
        var settledTotal = salaries.Where(s => s.IsSettled).Sum(s => s.NetSalary);
        var carryForwardTotal = salaries.Where(s => !s.IsSettled).Sum(s => s.BalanceAmount);
        var toBeSettledTotal = salaries.Where(s => !s.IsSettled).Sum(s => s.NetSalary);

        return Ok(new
        {
            advanceTotal,
            settledTotal,
            carryForwardTotal,
            toBeSettledTotal
        });
    }


}
