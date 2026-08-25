using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Teacher;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public TeachersController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/teachers
        // Gets active teachers by default
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers(
            bool includeInactive = false)
        {
            // Start with all teachers
            var query = _context.Teachers.AsQueryable();

            // By default, return only active teachers
            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            // Execute the query and get the teachers
            var teachers = await query.ToListAsync();

            return Ok(teachers);
        }


        // GET: api/teachers/1
        // Gets a single teacher by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetTeacher(int id)
        {
            // Find the teacher with the given ID
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist, return 404
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // Return the teacher
            return Ok(teacher);
        }


        // POST: api/teachers
        // Creates a new teacher
        [HttpPost]
        public async Task<ActionResult<Teacher>> CreateTeacher(
            CreateTeacherDto dto)
        {
            // Create a new Teacher entity from the DTO
            var teacher = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email
            };

            // Add the teacher to the database
            _context.Teachers.Add(teacher);

            // Save the teacher to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created teacher
            return Ok(teacher);
        }


        // PUT: api/teachers/1
        // Updates an existing teacher
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(
            int id,
            UpdateTeacherDto dto)
        {
            // Find the existing teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist, return 404
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // Update the teacher's information
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.Email = dto.Email;

            // Save the changes to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }


        // PUT: api/teachers/1/deactivate
        // Deactivates a teacher without deleting the database record
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateTeacher(int id)
        {
            // Find the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist, return 404
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // If the teacher is already inactive, return a bad request
            if (!teacher.IsActive)
            {
                return BadRequest("Teacher is already inactive.");
            }

            // Deactivate the teacher
            teacher.IsActive = false;

            // Save the change to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }

        // PUT: api/teachers/1/activate
        // Activates a previously deactivated teacher
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateTeacher(int id)
        {
            // Find the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the teacher does not exist, return 404
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // If the teacher is already active, return a bad request
            if (teacher.IsActive)
            {
                return BadRequest("Teacher is already active.");
            }

            // Activate the teacher
            teacher.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated teacher
            return Ok(teacher);
        }

    }
}