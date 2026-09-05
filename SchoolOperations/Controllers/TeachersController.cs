using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Teacher;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeachersController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/teachers
        // Gets active teachers by default
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers(
           bool includeInactive = false,
           string? search = null)
        {
            // Start with all teachers
            var query = _context.Teachers.AsQueryable();

            // By default, return only active teachers
            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            // Apply optional name search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(t =>
                    t.FirstName.Contains(search) ||
                    t.LastName.Contains(search));
            }

            // Execute the query
            var teachers = await query.ToListAsync();

            return Ok(teachers);
        }


        // GET: api/teachers/1
        // Gets a single teacher by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTeacher(int id)
        {
            // Find the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // Admin can view any teacher
            if (User.IsInRole("Admin"))
            {
                return Ok(teacher);
            }

            // Non-admin users cannot access inactive teachers
            if (!teacher.IsActive)
            {
                return NotFound("Teacher does not exist.");
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Teacher can view only their own record
            if (User.IsInRole("Teacher"))
            {
                if (user.TeacherId != teacher.Id)
                {
                    return Forbid();
                }

                return Ok(teacher);
            }

            // Other roles cannot access teacher records
            return Forbid();
        }


        // POST: api/teachers
        // Creates a new teacher
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Teacher>> CreateTeacher(
            CreateTeacherDto dto)
        {
            // Create a new Teacher entity
            var teacher = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,

                // New teachers are active by default
                IsActive = true
            };

            // Add the teacher to the database
            _context.Teachers.Add(teacher);

            // Save the teacher
            await _context.SaveChangesAsync();

            // Return the newly created teacher
            return Ok(teacher);
        }


        // PUT: api/teachers/1
        // Updates an existing teacher
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(
            int id,
            UpdateTeacherDto dto)
        {
            // Find the existing teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // Update teacher information
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.Email = dto.Email;

            // Save the changes
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }


        // PUT: api/teachers/1/deactivate
        // Deactivates a teacher without deleting the database record
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateTeacher(int id)
        {
            // Find the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // If the teacher is already inactive
            if (!teacher.IsActive)
            {
                return BadRequest("Teacher is already inactive.");
            }

            // Deactivate the teacher
            teacher.IsActive = false;

            // Save the change
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }


        // PUT: api/teachers/1/activate
        // Activates a previously deactivated teacher
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateTeacher(int id)
        {
            // Find the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // If the teacher is already active
            if (teacher.IsActive)
            {
                return BadRequest("Teacher is already active.");
            }

            // Activate the teacher
            teacher.IsActive = true;

            // Save the change
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }
    }
}