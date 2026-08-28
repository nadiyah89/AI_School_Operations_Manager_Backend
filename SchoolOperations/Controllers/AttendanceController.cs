using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Attendance;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ============================================================
        // GET: api/attendance
        // Admin and Teacher can view all attendance records
        // Active records are returned by default
        // ============================================================

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAttendance(
            bool includeInactive = false)
        {
            // Start with all attendance records
            var query = _context.Attendances
                .Include(a => a.Student)
                .AsQueryable();

            // By default, return only active attendance records
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            // Execute the query
            var attendance = await query
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return Ok(attendance);
        }


        // ============================================================
        // GET: api/attendance/student/1
        // Gets attendance for a specific student
        //
        // Admin   -> Any student
        // Teacher -> Any student
        // Student -> Only themselves
        // Parent  -> Only their child
        // ============================================================

        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetStudentAttendance(
            int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }


            // --------------------------------------------------------
            // ADMIN
            // --------------------------------------------------------

            if (User.IsInRole("Admin"))
            {
                var adminAttendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a =>
                        a.StudentId == studentId &&
                        a.IsActive)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return Ok(adminAttendance);
            }


            // --------------------------------------------------------
            // TEACHER
            // --------------------------------------------------------

            if (User.IsInRole("Teacher"))
            {
                var teacherAttendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a =>
                        a.StudentId == studentId &&
                        a.IsActive)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return Ok(teacherAttendance);
            }


            // Get the currently logged-in ApplicationUser
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            // --------------------------------------------------------
            // STUDENT
            // --------------------------------------------------------

            if (User.IsInRole("Student"))
            {
                // Student can only see their own attendance
                if (user.StudentId != studentId)
                {
                    return Forbid();
                }

                var studentAttendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a =>
                        a.StudentId == studentId &&
                        a.IsActive)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return Ok(studentAttendance);
            }


            // --------------------------------------------------------
            // PARENT
            // --------------------------------------------------------

            if (User.IsInRole("Parent"))
            {
                // Find the parent associated with the logged-in user
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                // Parent can only see their own child's attendance
                if (parent.StudentId != studentId)
                {
                    return Forbid();
                }

                var parentAttendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a =>
                        a.StudentId == studentId &&
                        a.IsActive)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return Ok(parentAttendance);
            }


            // Any other role is not allowed
            return Forbid();
        }


        // ============================================================
        // GET: api/attendance/1
        // Gets a single attendance record
        //
        // Admin   -> Any attendance record
        // Teacher -> Any attendance record
        // Student -> Only their own attendance
        // Parent  -> Only their child's attendance
        // ============================================================

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            // Find the attendance record
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            // Check whether the record exists
            if (attendance == null)
            {
                return NotFound("Attendance record does not exist.");
            }

            // Only active records should normally be visible
            if (!attendance.IsActive)
            {
                return NotFound("Attendance record does not exist.");
            }

            // Admin and Teacher can view any attendance record
            if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
            {
                return Ok(attendance);
            }

            // Get the currently logged-in ApplicationUser
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can only view their own attendance
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != attendance.StudentId)
                {
                    return Forbid();
                }

                return Ok(attendance);
            }

            // Parent can only view their child's attendance
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                if (parent.StudentId != attendance.StudentId)
                {
                    return Forbid();
                }

                return Ok(attendance);
            }

            // Any other role is not allowed
            return Forbid();
        }


        // ============================================================
        // POST: api/attendance
        // Creates a new attendance record
        //
        // Admin and Teacher can create attendance
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateAttendance(
            CreateAttendanceDto dto)
        {
            // --------------------------------------------------------
            // Check whether the student exists
            // --------------------------------------------------------

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            if (student == null)
            {
                return NotFound("Student does not exist.");
            }


            // --------------------------------------------------------
            // Attendance should only be recorded for active students
            // --------------------------------------------------------

            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }


            // --------------------------------------------------------
            // Prevent duplicate attendance
            //
            // One student can have only one attendance record
            // for a particular date.
            // --------------------------------------------------------

            var attendanceAlreadyExists = await _context.Attendances
                .AnyAsync(a =>
                    a.StudentId == dto.StudentId &&
                    a.Date.Date == dto.Date.Date);

            if (attendanceAlreadyExists)
            {
                return BadRequest(
                    "Attendance has already been recorded for this student on this date.");
            }


            // --------------------------------------------------------
            // Create Attendance entity
            // --------------------------------------------------------

            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                Date = dto.Date,
                IsPresent = dto.IsPresent,

                // New attendance records are active
                IsActive = true
            };


            // Add attendance to database
            _context.Attendances.Add(attendance);

            // Save to SQL Server
            await _context.SaveChangesAsync();


            // Load Student navigation property
            await _context.Entry(attendance)
                .Reference(a => a.Student)
                .LoadAsync();


            // Return newly created attendance
            return Ok(attendance);
        }


        // ============================================================
        // PUT: api/attendance/1
        // Updates an existing attendance record
        //
        // Admin and Teacher can update attendance
        // ============================================================

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAttendance(
            int id,
            UpdateAttendanceDto dto)
        {
            // --------------------------------------------------------
            // Find existing attendance record
            // --------------------------------------------------------

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound(
                    "Attendance record does not exist.");
            }


            // --------------------------------------------------------
            // Check whether the student still exists
            // --------------------------------------------------------

            if (attendance.Student == null)
            {
                return NotFound(
                    "The student associated with this attendance record does not exist.");
            }


            // --------------------------------------------------------
            // Prevent duplicate attendance
            //
            // Example:
            // Student 1 already has attendance on 20 Aug.
            // We cannot update another record to 20 Aug.
            // --------------------------------------------------------

            var duplicateExists = await _context.Attendances
                .AnyAsync(a =>
                    a.Id != id &&
                    a.StudentId == attendance.StudentId &&
                    a.Date.Date == dto.Date.Date);

            if (duplicateExists)
            {
                return BadRequest(
                    "Attendance has already been recorded for this student on this date.");
            }


            // --------------------------------------------------------
            // Update attendance information
            // --------------------------------------------------------

            attendance.Date = dto.Date;
            attendance.IsPresent = dto.IsPresent;


            // Save changes
            await _context.SaveChangesAsync();


            // Return updated attendance
            return Ok(attendance);
        }


        // ============================================================
        // PUT: api/attendance/1/deactivate
        // Deactivates an attendance record
        //
        // Only Admin can deactivate
        // ============================================================

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAttendance(
            int id)
        {
            // Find the attendance record
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            // Check whether the record exists
            if (attendance == null)
            {
                return NotFound(
                    "Attendance record does not exist.");
            }


            // Check whether it is already inactive
            if (!attendance.IsActive)
            {
                return BadRequest(
                    "Attendance record is already inactive.");
            }


            // Deactivate the record
            attendance.IsActive = false;


            // Save the change
            await _context.SaveChangesAsync();


            // Return the updated record
            return Ok(attendance);
        }


        // ============================================================
        // PUT: api/attendance/1/activate
        // Activates a previously deactivated attendance record
        //
        // Only Admin can activate
        // ============================================================

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateAttendance(
            int id)
        {
            // Find the attendance record
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            // Check whether the record exists
            if (attendance == null)
            {
                return NotFound(
                    "Attendance record does not exist.");
            }


            // Check whether it is already active
            if (attendance.IsActive)
            {
                return BadRequest(
                    "Attendance record is already active.");
            }


            // Activate the record
            attendance.IsActive = true;


            // Save the change
            await _context.SaveChangesAsync();


            // Return the updated record
            return Ok(attendance);
        }
    }
}