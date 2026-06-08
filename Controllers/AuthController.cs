using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;
        private readonly IJwtHelper _jwtHelper;

        public AuthController(AppDbContext dbcontext, IJwtHelper jwtHelper)
        {
            _dbcontext = dbcontext;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            if (request == null)
            {
                return BadRequest("Login request is required");
            }

            var user = await _dbcontext.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == request.UserEmail);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            bool isPasswordValid;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.HashPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Handle cases where the database hash is legacy, invalid, or empty
                return Unauthorized("Invalid email or password");
            }

            if (isPasswordValid)
            {
                var roles = user.UserRoles?
                    .Select(ur => ur.Role?.RoleName ?? "")
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList() ?? new List<string>();

                var token = _jwtHelper.GenerateToken(user, roles);

                return Ok(new 
                { 
                    message = "Login successful", 
                    token = token, 
                    user = new { user.Username, user.UserEmail, roles } 
                });
            }

            return Unauthorized("Invalid email or password");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            if (request == null) return BadRequest("User data is required");

            // Check if email already exists
            var existingUser = await _dbcontext.Users.AnyAsync(u => u.UserEmail == request.UserEmail);
            if (existingUser)
            {
                return Conflict("A user with this email already exists.");
            }

            // Hash the plain password using BCrypt
            var hashed = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

            // Map DTO to User model
            var user = new User
            {
                Username = request.Username,
                UserEmail = request.UserEmail,
                HashPassword = hashed
            };

            
            var defaultRole = await _dbcontext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Developer");

            if (defaultRole != null) {
                var userRole = new UserRole
                {
                    User = user,
                    Role = defaultRole
                };
               await _dbcontext.UserRoles.AddAsync(userRole);
            }

            // Persist user via _dbcontext
            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();

            return Ok("Registered");
        }
    }
}
