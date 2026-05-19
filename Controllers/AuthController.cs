using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserEmail == request.UserEmail);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, request.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return Ok(new { message = "Login successful", user = new { user.Username, user.UserEmail } });
            }

            return Unauthorized("Invalid email or password");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null) return BadRequest("User data is required");

            // Check if email already exists
            var existingUser = await _dbcontext.Users.AnyAsync(u => u.UserEmail == user.UserEmail);
            if (existingUser)
            {
                return Conflict("A user with this email already exists.");
            }

            // Hash the plain password (user.HashPassword currently holds the plain password)
            var hashed = _passwordHasher.HashPassword(user, user.HashPassword);
            user.HashPassword = hashed;

            // Persist user via _dbcontext
            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();

            return Ok("Registered");
        }
    }
}
