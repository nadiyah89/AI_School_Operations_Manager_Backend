using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.AcademicPerformance;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademicPerformanceController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public AcademicPerformanceController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/academicperformance
        // Gets all academic performance records
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicPerformance>>> GetAcademicPerformance(
    bool includeInactive = false)
        {
            // Start with all academic performance records
            var query = _context.AcademicPerformances.AsQueryable();

            // By default, return only active records
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            // Execute the query
            var performance = await query.ToListAsync();

            return Ok(performance);
        }


        // GET: api/academicperformance/student/1
        // Gets all academic performance records for a specific student
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AcademicPerformance>>> GetStudentAcademicPerformance(
            int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            // If the student does not exist, return 404
            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Get all active academic performance records for the student
            var performance = await _context.AcademicPerformances
                .Where(a => a.StudentId == studentId && a.IsActive)
                .ToListAsync();

            return Ok(performance);
        }


        // GET: api/academicperformance/1
        // Gets one academic performance record by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AcademicPerformance>> GetAcademicPerformanceById(int id)
        {
            // Find the academic performance record
            var performance = await _context.AcademicPerformances
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the record does not exist, return 404
            if (performance == null)
            {
                return NotFound("Academic performance record does not exist.");
            }

            // Return the record
            return Ok(performance);
        }


        // POST: api/academicperformance
        // Creates a new academic performance record
        [HttpPost]
        public async Task<ActionResult<AcademicPerformance>> CreateAcademicPerformance(
            CreateAcademicPerformanceDto dto)
        {
            // Check whether the student exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Academic records should only be created for active students
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Maximum marks must be greater than zero
            if (dto.MaximumMarks <= 0)
            {
                return BadRequest("Maximum marks must be greater than zero.");
            }

            // Marks obtained cannot be negative
            if (dto.MarksObtained < 0)
            {
                return BadRequest("Marks obtained cannot be negative.");
            }

            // Marks obtained cannot exceed maximum marks
            if (dto.MarksObtained > dto.MaximumMarks)
            {
                return BadRequest("Marks obtained cannot be greater than maximum marks.");
            }

            // Create the database entity from the DTO
            var performance = new AcademicPerformance
            {
                StudentId = dto.StudentId,
                Subject = dto.Subject,
                ExamName = dto.ExamName,
                MarksObtained = dto.MarksObtained,
                MaximumMarks = dto.MaximumMarks,
                ExamDate = dto.ExamDate,
                IsActive = true
            };

            // Add the academic performance record to the database
            _context.AcademicPerformances.Add(performance);

            // Save the record to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created record
            return Ok(performance);
        }



        // PUT: api/academicperformance/1
        // Updates an existing academic performance record
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAcademicPerformance(
            int id,
            UpdateAcademicPerformanceDto dto)
        {
            // Find the existing academic performance record
            var performance = await _context.AcademicPerformances
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the record does not exist, return 404
            if (performance == null)
            {
                return NotFound("Academic performance record does not exist.");
            }

            // Maximum marks must be greater than zero
            if (dto.MaximumMarks <= 0)
            {
                return BadRequest("Maximum marks must be greater than zero.");
            }

            // Marks obtained cannot be negative
            if (dto.MarksObtained < 0)
            {
                return BadRequest("Marks obtained cannot be negative.");
            }

            // Marks obtained cannot exceed maximum marks
            if (dto.MarksObtained > dto.MaximumMarks)
            {
                return BadRequest("Marks obtained cannot be greater than maximum marks.");
            }

            // Update the academic performance information
            performance.Subject = dto.Subject;
            performance.ExamName = dto.ExamName;
            performance.MarksObtained = dto.MarksObtained;
            performance.MaximumMarks = dto.MaximumMarks;
            performance.ExamDate = dto.ExamDate;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return the updated record
            return Ok(performance);
        }


        // PUT: api/academicperformance/1/deactivate
        // Deactivates an academic performance record without deleting it
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateAcademicPerformance(int id)
        {
            // Find the academic performance record
            var performance = await _context.AcademicPerformances
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the record does not exist, return 404
            if (performance == null)
            {
                return NotFound("Academic performance record does not exist.");
            }

            // If the record is already inactive, return a bad request
            if (!performance.IsActive)
            {
                return BadRequest("Academic performance record is already inactive.");
            }

            // Deactivate the record
            performance.IsActive = false;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated record
            return Ok(performance);
        }



        // PUT: api/academicperformance/1/activate
        // Activates an academic performance record
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateAcademicPerformance(int id)
        {
            // Find the academic performance record
            var performance = await _context.AcademicPerformances
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the record does not exist, return 404
            if (performance == null)
            {
                return NotFound("Academic performance record does not exist.");
            }

            // If the record is already active, return a bad request
            if (performance.IsActive)
            {
                return BadRequest("Academic performance record is already active.");
            }

            // Activate the record
            performance.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated record
            return Ok(performance);
        }
    }
}