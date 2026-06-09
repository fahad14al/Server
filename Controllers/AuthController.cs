using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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

            string identifier = string.Empty;
            if (!string.IsNullOrEmpty(request.UsernameOrEmail))
            {
                identifier = request.UsernameOrEmail;
            }
            else if (!string.IsNullOrEmpty(request.UserEmail))
            {
                identifier = request.UserEmail;
            }
            else if (!string.IsNullOrEmpty(request.Username))
            {
                identifier = request.Username;
            }

            if (string.IsNullOrEmpty(identifier))
            {
                return BadRequest("Username or email is required");
            }

            var user = await _dbcontext.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == identifier || u.Username == identifier);

            if (user == null)
            {
                return Unauthorized("Invalid email/username or password");
            }

            bool isPasswordValid = false;

            // 1. Try BCrypt EnhancedVerify
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.HashPassword);
            }
            catch
            {
                // Ignore BCrypt verify errors and try other formats
            }

            // 2. Try ASP.NET Identity PasswordHasher if not already valid
            if (!isPasswordValid)
            {
                try
                {
                    var hasher = new PasswordHasher<User>();
                    var verifyResult = hasher.VerifyHashedPassword(user, user.HashPassword, request.Password);
                    if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        isPasswordValid = true;

                        // Rehash to BCrypt and save
                        user.HashPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
                        _dbcontext.Users.Update(user);
                        await _dbcontext.SaveChangesAsync();
                    }
                }
                catch
                {
                    // Ignore format exception
                }
            }

            // 3. Fallback: handle legacy/plaintext password storage
            if (!isPasswordValid)
            {
                if (user.HashPassword == request.Password)
                {
                    // Rehash using BCrypt and save
                    user.HashPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
                    _dbcontext.Users.Update(user);
                    await _dbcontext.SaveChangesAsync();

                    isPasswordValid = true;
                }
            }

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email/username or password");
            }

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
