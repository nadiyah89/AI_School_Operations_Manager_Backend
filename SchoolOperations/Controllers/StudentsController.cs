using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.Models;

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
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students.ToListAsync();

            return Ok(students);
        }

        // POST: api/students
        [HttpPost]
        public async Task<IActionResult> CreateStudent(Student student)
        {
            // Add the new student to the database context
            _context.Students.Add(student);

            // Save the changes to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created student
            return Ok(student);
        }


    }
}