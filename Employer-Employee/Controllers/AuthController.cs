using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ✅ AllowAnonymous so login works without token
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required");

        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.Email == request.Email);

        if (employer == null)
            return Unauthorized("Invalid email or password");

        // ✅ Verify hashed password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, employer.PasswordHash))
            return Unauthorized("Invalid email or password");

        // ✅ Generate JWT token
        var token = GenerateJwtToken(employer, out DateTime expiry);

        return Ok(new
        {
            token,
            expiresAt = expiry,
            employerId = employer.EmployerId,
            email = employer.Email,
            role = "Admin"
        });
    }

    private string GenerateJwtToken(Employer employer, out DateTime expiry)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, employer.Email),
            new Claim("employerId", employer.EmployerId.ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = _config["Jwt:Key"] ?? throw new Exception("JWT Key not configured");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        expiry = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
