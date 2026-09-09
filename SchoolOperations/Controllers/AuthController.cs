using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Auth;
using SchoolOperations.Models;


namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SchoolDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SchoolDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }


        // POST: api/Auth/register
        // Creates a new application user
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            // Roles allowed through public registration
            var allowedRoles = new[] { "Student", "Teacher", "Parent" };

            // Prevent public registration for Admin or any other role
            if (!allowedRoles.Contains(
                    dto.Role,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid role.");
            }

            // Normalize the role so existing role checks remain consistent
            dto.Role = allowedRoles.First(
                role => string.Equals(
                    role,
                    dto.Role,
                    StringComparison.OrdinalIgnoreCase));

            // Check whether the email is already registered
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return BadRequest("Email is already registered.");
            }


            // -----------------------------------------
            // Validate the linked school record
            // -----------------------------------------

            // Student account
            if (dto.Role == "Student")
            {
                if (!dto.StudentId.HasValue)
                {
                    return BadRequest("StudentId is required for a Student account.");
                }

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == dto.StudentId.Value);

                if (student == null)
                {
                    return NotFound("Student does not exist.");
                }

                if (!student.IsActive)
                {
                    return BadRequest("Student is inactive.");
                }

                var studentAlreadyLinked = await _context.Users
                    .AnyAsync(u => u.StudentId == dto.StudentId.Value);

                if (studentAlreadyLinked)
                {
                    return BadRequest("This Student already has an account.");
                }
            }


            // Teacher account
            if (dto.Role == "Teacher")
            {
                if (!dto.TeacherId.HasValue)
                {
                    return BadRequest("TeacherId is required for a Teacher account.");
                }

                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Id == dto.TeacherId.Value);

                if (teacher == null)
                {
                    return NotFound("Teacher does not exist.");
                }

                if (!teacher.IsActive)
                {
                    return BadRequest("Teacher is inactive.");
                }

                var teacherAlreadyLinked = await _context.Users
                      .AnyAsync(u => u.TeacherId == dto.TeacherId.Value);

                if (teacherAlreadyLinked)
                {
                    return BadRequest("This Teacher already has an account.");
                }
            }


            // Parent account
            if (dto.Role == "Parent")
            {
                if (!dto.ParentId.HasValue)
                {
                    return BadRequest("ParentId is required for a Parent account.");
                }

                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == dto.ParentId.Value);

                if (parent == null)
                {
                    return NotFound("Parent does not exist.");
                }

                var parentAlreadyLinked = await _context.Users
                      .AnyAsync(u => u.ParentId == dto.ParentId.Value);

                if (parentAlreadyLinked)
                {
                    return BadRequest("This Parent already has an account.");
                }
            }


            // -----------------------------------------
            // Create ApplicationUser
            // -----------------------------------------

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                StudentId = dto.StudentId,
                TeacherId = dto.TeacherId,
                ParentId = dto.ParentId
            };


            // Create the user.
            // ASP.NET Core Identity securely hashes the password.
            var result = await _userManager.CreateAsync(
                user,
                dto.Password);


            // Check whether user creation succeeded
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description)
                    .ToList();

                return BadRequest(errors);
            }


            // Assign the requested role
            await _userManager.AddToRoleAsync(
                user,
                dto.Role);


            // Return successful response
            return Ok(new
            {
                message = "User registered successfully.",
                userId = user.Id,
                email = user.Email,
                role = dto.Role
            });
        }




        // POST: api/Auth/login
        // Authenticates a user and returns a JWT token
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Find the user using the supplied email
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // If no user exists, authentication fails
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Check whether the supplied password is correct
            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

            // If the password is incorrect, authentication fails
            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Get all roles assigned to this user
            var roles = await _userManager.GetRolesAsync(user);

            // Create JWT claims
            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id),

        new Claim(
            ClaimTypes.Email,
            user.Email!)
    };

            // Add each role to the JWT
            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }

            // Read the JWT secret key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            // Create signing credentials
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(
                        _configuration["Jwt:DurationInMinutes"])),
                signingCredentials: credentials);

            // Convert the token into a string
            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // Return the token and user information
            return Ok(new
            {
                token = tokenString,
                userId = user.Id,
                email = user.Email,
                roles = roles
            });
        }


        // GET: api/Auth/admin-test
        // Accessible only to Admin users
        [HttpGet("admin-test")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminTest()
        {
            return Ok("You are authorized as Admin.");
        }


        // GET: api/Auth/teacher-test
        // Accessible only to Teacher users
        [HttpGet("teacher-test")]
        [Authorize(Roles = "Teacher")]
        public IActionResult TeacherTest()
        {
            return Ok("You are authorized as Teacher.");
        }


        // GET: api/Auth/student-test
        // Accessible only to Student users
        [HttpGet("student-test")]
        [Authorize(Roles = "Student")]
        public IActionResult StudentTest()
        {
            return Ok("You are authorized as Student.");
        }


        // GET: api/Auth/parent-test
        // Accessible only to Parent users
        [HttpGet("parent-test")]
        [Authorize(Roles = "Parent")]
        public IActionResult ParentTest()
        {
            return Ok("You are authorized as Parent.");
        }
    }
}