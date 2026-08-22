using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.Models;
using SchoolOperations.DTOs.Student;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public StudentsController(SchoolDbContext context)
        {
            _context = context;
        }

        // GET: api/students
        // Gets active students by default
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(
            bool includeInactive = false)
        {
            // Start with all students
            var query = _context.Students.AsQueryable();

            // By default, return only active students
            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            // Execute the query and get the students
            var students = await query.ToListAsync();

            return Ok(students);
        }


        // GET: api/students/1
        // Gets a single student by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            // Find the student with the given ID
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Return the student
            return Ok(student);
        }


        // POST: api/students
        // Creates a new student
        [HttpPost]
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



    }
}