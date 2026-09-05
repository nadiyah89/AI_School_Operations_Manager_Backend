using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Student;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/students
        // Gets active students by default
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(
        bool includeInactive = false,
        string? search = null)
        {
            // Start with all students
            var query = _context.Students.AsQueryable();

            // By default, return only active students
            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            // Apply optional name search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(s =>
                    s.FirstName.Contains(search) ||
                    s.LastName.Contains(search));
            }

            // Execute the query and get the students
            var students = await query.ToListAsync();

            return Ok(students);
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Admin can view any student, including inactive students
            if (User.IsInRole("Admin"))
            {
                return Ok(student);
            }

            // Non-admin users cannot access inactive students
            if (!student.IsActive)
            {
                return NotFound("Student not found.");
            }

            // Teacher can view any active student
            if (User.IsInRole("Teacher"))
            {
                return Ok(student);
            }

            // Get the currently logged-in ApplicationUser
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can view only their own record
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != student.Id)
                {
                    return Forbid();
                }

                return Ok(student);
            }

            // Parent can view only their own child's record
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                // Check whether this student belongs to this parent
                if (parent.StudentId != student.Id)
                {
                    return Forbid();
                }

                return Ok(student);
            }

            return Forbid();
        }


        // POST: api/students
        // Creates a new student
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
        {
            // Create a Student entity from the DTO
            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                IsActive = true
            };

            // Add the student to the database context
            _context.Students.Add(student);

            // Save the changes to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created student
            return Ok(student);
        }



        // PUT: api/students/1
        // Updates an existing student
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(
            int id,
            UpdateStudentDto dto)
        {
            // Find the existing student
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Update the student's information
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.DateOfBirth = dto.DateOfBirth;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return the updated student
            return Ok(student);
        }


        // PUT: api/students/1/deactivate
        // Deactivates a student without deleting the database record
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateStudent(int id)
        {
            // Find the student
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // If the student is already inactive, return a bad request
            if (!student.IsActive)
            {
                return BadRequest("Student is already inactive.");
            }

            // Deactivate the student
            student.IsActive = false;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated student
            return Ok(student);
        }


        // PUT: api/students/1/activate
        // Activates a previously deactivated student
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateStudent(int id)
        {
            // Find the student
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // If the student is already active, return a bad request
            if (student.IsActive)
            {
                return BadRequest("Student is already active.");
            }

            // Activate the student
            student.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated student
            return Ok(student);
        }


    }
}