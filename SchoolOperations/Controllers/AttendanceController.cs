using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Attendance;
using SchoolOperations.Models;


namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public AttendanceController(SchoolDbContext context)
        {
            _context = context;
        }

        // GET: api/attendance
        [HttpGet]
        public async Task<IActionResult> GetAttendance()
        {
            var attendance = await _context.Attendances
                .ToListAsync();

            return Ok(attendance);
        }



        // GET: api/attendance/student/1
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance([FromRoute] int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Get all attendance records for the student
            var attendance = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            return Ok(attendance);
        }



        // POST: api/attendance
        [HttpPost]
        public async Task<IActionResult> CreateAttendance(CreateAttendanceDto dto)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == dto.StudentId);

            if (!studentExists)
            {
                return BadRequest("Student does not exist.");
            }

            // Check whether attendance has already been recorded for this student today
            var attendanceAlreadyExists = await _context.Attendances
                .AnyAsync(a =>
                    a.StudentId == dto.StudentId &&
                    a.Date.Date == dto.Date.Date);

            if (attendanceAlreadyExists)
            {
                return BadRequest("Attendance has already been recorded for this student on this date.");
            }

            // Create the database entity from the DTO
            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                Date = dto.Date,
                IsPresent = dto.IsPresent
            };

            // Add the attendance record to the database
            _context.Attendances.Add(attendance);

            // Save the record to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created attendance record
            return Ok(attendance);
        }


    }
}