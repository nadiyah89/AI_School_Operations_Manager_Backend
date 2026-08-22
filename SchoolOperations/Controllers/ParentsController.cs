using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Parent;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public ParentsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/parents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parent>>> GetParents()
        {
            // Get all parents from the database
            var parents = await _context.Parents.ToListAsync();

            return Ok(parents);
        }


        // GET /api/parents/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Parent>>> GetParentsByStudent(int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Get all parents belonging to the student
            var parents = await _context.Parents
                .Where(p => p.StudentId == studentId)
                .ToListAsync();

            return Ok(parents);
        }





        // POST /api/parents
        [HttpPost]
        public async Task<ActionResult<Parent>> CreateParent(CreateParentDto dto)
        {
            // Find the student
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // Check whether the student exists
            if (student == null)
            {
                return BadRequest("Student does not exist.");
            }

            // Check whether the student is active
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Create a new Parent entity from the DTO
            var parent = new Parent
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Relationship = dto.Relationship,
                StudentId = dto.StudentId
            };

            // Add the parent to the database
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            // Return the newly created parent
            return Ok(parent);
        }




        // PUT: api/parents/1
        // Updates an existing parent
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParent(
            int id,
            UpdateParentDto dto)
        {
            // Find the existing parent
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == id);

            // If the parent does not exist, return 404
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // Update the parent's information
            parent.FirstName = dto.FirstName;
            parent.LastName = dto.LastName;
            parent.PhoneNumber = dto.PhoneNumber;
            parent.Email = dto.Email;
            parent.Relationship = dto.Relationship;

            // Save the changes to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated parent
            return Ok(parent);
        }

    }
}