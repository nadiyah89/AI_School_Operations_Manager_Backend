using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.AcademicPerformance;
using SchoolOperations.Models;
using SchoolOperations.Services;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicPerformanceController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IAcademicPerformanceService _academicPerformanceService;

        public AcademicPerformanceController(
               SchoolDbContext context,
               UserManager<ApplicationUser> userManager,
               IAcademicPerformanceService academicPerformanceService)
        {
            _context = context;
            _userManager = userManager;
            _academicPerformanceService = academicPerformanceService;
        }


        // GET: api/academicperformance
        // Gets all academic performance records
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
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
        // Gets academic performance records for a specific student
        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AcademicPerformance>>> GetStudentAcademicPerformance(
            int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Admin and Teacher can view any student's performance
            if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
            {
                var performance = await _context.AcademicPerformances
                    .Where(a => a.StudentId == studentId && a.IsActive)
                    .ToListAsync();

                return Ok(performance);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can view only their own performance
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != studentId)
                {
                    return Forbid();
                }

                var performance = await _context.AcademicPerformances
                    .Where(a => a.StudentId == studentId && a.IsActive)
                    .ToListAsync();

                return Ok(performance);
            }

            // Parent can view only their child's performance
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                if (parent.StudentId != studentId)
                {
                    return Forbid();
                }

                var performance = await _context.AcademicPerformances
                    .Where(a => a.StudentId == studentId && a.IsActive)
                    .ToListAsync();

                return Ok(performance);
            }

            return Forbid();
        }



        // GET: api/academicperformance/poor?threshold=60&subject=Math
        // Gets students whose latest performance is below the given threshold
        [HttpGet("poor")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<PoorPerformanceSummaryDto>>>
            GetStudentsBelowPerformanceThreshold(
                [FromQuery] decimal threshold,
                [FromQuery] string? subject = null)
        {
            // Threshold must be between 0 and 100
            if (threshold < 0 || threshold > 100)
            {
                return BadRequest(
                    "Threshold must be between 0 and 100.");
            }

            // Ask the service to perform the academic analytics
            var results =
                await _academicPerformanceService
                    .GetStudentsBelowPerformanceThresholdAsync(
                        threshold,
                        subject);

            return Ok(results);
        }


        // GET: api/academicperformance/declining?subject=Math
        // Gets students whose latest performance is lower than their previous exam
        [HttpGet("declining")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<DecliningPerformanceSummaryDto>>>
            GetStudentsWithDecliningPerformance(
                [FromQuery] string? subject = null)
        {
            // Ask the service to perform declining performance analytics
            var results =
                await _academicPerformanceService
                    .GetStudentsWithDecliningPerformanceAsync(subject);

            return Ok(results);
        }




        // GET: api/academicperformance/1
        // Gets one academic performance record by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AcademicPerformance>> GetAcademicPerformanceById(
            int id)
        {
            // Find the academic performance record
            var performance = await _context.AcademicPerformances
                .FirstOrDefaultAsync(a => a.Id == id);

            if (performance == null)
            {
                return NotFound("Academic performance record does not exist.");
            }

            // Admin and Teacher can view any performance record
            if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
            {
                return Ok(performance);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can view only their own performance
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != performance.StudentId)
                {
                    return Forbid();
                }

                return Ok(performance);
            }

            // Parent can view only their child's performance
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                if (parent.StudentId != performance.StudentId)
                {
                    return Forbid();
                }

                return Ok(performance);
            }

            return Forbid();
        }


        // POST: api/academicperformance
        // Creates a new academic performance record
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
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
        [Authorize(Roles = "Admin,Teacher")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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