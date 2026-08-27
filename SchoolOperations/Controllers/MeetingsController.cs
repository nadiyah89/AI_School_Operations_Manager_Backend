using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Meeting;
using SchoolOperations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeetingsController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/meetings
        // Gets all active meetings by default
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetMeetings(
            bool includeInactive = false)
        {
            // Start with all meetings
            var query = _context.Meetings
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .AsQueryable();

            // By default, return only active meetings
            if (!includeInactive)
            {
                query = query.Where(m => m.IsActive);
            }

            // Execute the query
            var meetings = await query.ToListAsync();

            return Ok(meetings);
        }


        // GET: api/meetings/1
        // Gets one meeting by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Meeting>> GetMeeting(int id)
        {
            // Find the meeting
            var meeting = await _context.Meetings
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the meeting does not exist
            if (meeting == null)
            {
                return NotFound("Meeting does not exist.");
            }

            // Admin can view any meeting
            if (User.IsInRole("Admin"))
            {
                return Ok(meeting);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Teacher can view only meetings assigned to them
            if (User.IsInRole("Teacher"))
            {
                if (user.TeacherId != meeting.TeacherId)
                {
                    return Forbid();
                }

                return Ok(meeting);
            }

            // Parent can view only meetings related to their child
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                if (parent.StudentId != meeting.StudentId)
                {
                    return Forbid();
                }

                return Ok(meeting);
            }

            // Student has no meeting access
            return Forbid();
        }


        // GET: api/meetings/student/3
        // Gets meetings for a specific student
        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetStudentMeetings(
            int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Admin can view any student's meetings
            if (User.IsInRole("Admin"))
            {
                var adminMeetings = await _context.Meetings
                    .Include(m => m.Student)
                    .Include(m => m.Teacher)
                    .Where(m => m.StudentId == studentId && m.IsActive)
                    .ToListAsync();

                return Ok(adminMeetings);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Teacher can view meetings assigned to them
            if (User.IsInRole("Teacher"))
            {
                var teacherMeetings = await _context.Meetings
                    .Include(m => m.Student)
                    .Include(m => m.Teacher)
                    .Where(m =>
                        m.StudentId == studentId &&
                        m.TeacherId == user.TeacherId &&
                        m.IsActive)
                    .ToListAsync();

                return Ok(teacherMeetings);
            }

            // Parent can view only their child's meetings
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

                var parentMeetings = await _context.Meetings
                    .Include(m => m.Student)
                    .Include(m => m.Teacher)
                    .Where(m => m.StudentId == studentId && m.IsActive)
                    .ToListAsync();

                return Ok(parentMeetings);
            }

            return Forbid();
        }


        // GET: api/meetings/teacher/1
        // Gets all active meetings for a specific teacher
        [HttpGet("teacher/{teacherId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetTeacherMeetings(
            int teacherId)
        {
            // Check whether the teacher exists
            var teacherExists = await _context.Teachers
                .AnyAsync(t => t.Id == teacherId);

            if (!teacherExists)
            {
                return NotFound("Teacher does not exist.");
            }

            // Admin can view any teacher's meetings
            if (User.IsInRole("Admin"))
            {
                var adminMeetings = await _context.Meetings
                    .Include(m => m.Student)
                    .Include(m => m.Teacher)
                    .Where(m => m.TeacherId == teacherId && m.IsActive)
                    .ToListAsync();

                return Ok(adminMeetings);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Teacher can view only their own meetings
            if (User.IsInRole("Teacher"))
            {
                if (user.TeacherId != teacherId)
                {
                    return Forbid();
                }

                var teacherMeetings = await _context.Meetings
                    .Include(m => m.Student)
                    .Include(m => m.Teacher)
                    .Where(m => m.TeacherId == teacherId && m.IsActive)
                    .ToListAsync();

                return Ok(teacherMeetings);
            }

            // Parents and Students cannot use this endpoint
            return Forbid();
        }


        // POST: api/meetings
        // Creates a new meeting
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Meeting>> CreateMeeting(
            CreateMeetingDto dto)
        {
            // Check whether the student exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Meetings should only be created for active students
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Check whether the teacher exists
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == dto.TeacherId);

            // If the teacher does not exist, return 404
            if (teacher == null)
            {
                return NotFound("Teacher does not exist.");
            }

            // Meetings should only be created with active teachers
            if (!teacher.IsActive)
            {
                return BadRequest("Teacher is inactive.");
            }

            // A Teacher can create a meeting only for themselves
            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                if (user.TeacherId != dto.TeacherId)
                {
                    return Forbid();
                }
            }

            // Create the meeting entity
            var meeting = new Meeting
            {
                StudentId = dto.StudentId,
                TeacherId = dto.TeacherId,
                MeetingDate = dto.MeetingDate,
                Purpose = dto.Purpose,
                Notes = dto.Notes,

                // New meetings always start as Scheduled
                Status = "Scheduled",

                // New meetings are active
                IsActive = true
            };

            // Add the meeting to the database
            _context.Meetings.Add(meeting);

            // Save the meeting
            await _context.SaveChangesAsync();

            // Return the newly created meeting
            return Ok(meeting);
        }


        // PUT: api/meetings/1
        // Updates an existing meeting
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateMeeting(
            int id,
            UpdateMeetingDto dto)
        {
            // Find the existing meeting
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the meeting does not exist, return 404
            if (meeting == null)
            {
                return NotFound("Meeting does not exist.");
            }

            // A Teacher can update only meetings assigned to them
            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                if (user.TeacherId != meeting.TeacherId)
                {
                    return Forbid();
                }
            }

            // Validate the meeting status
            if (dto.Status != "Scheduled" &&
                dto.Status != "Completed" &&
                dto.Status != "Cancelled")
            {
                return BadRequest(
                    "Invalid meeting status. Use Scheduled, Completed, or Cancelled.");
            }

            // Update the meeting information
            meeting.MeetingDate = dto.MeetingDate;
            meeting.Purpose = dto.Purpose;
            meeting.Notes = dto.Notes;
            meeting.Status = dto.Status;

            // Save the changes
            await _context.SaveChangesAsync();

            // Return the updated meeting
            return Ok(meeting);
        }

        // PUT: api/meetings/1/deactivate
        // Deactivates a meeting without deleting it
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateMeeting(int id)
        {
            // Find the meeting
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the meeting does not exist, return 404
            if (meeting == null)
            {
                return NotFound("Meeting does not exist.");
            }

            // If the meeting is already inactive, return a bad request
            if (!meeting.IsActive)
            {
                return BadRequest("Meeting is already inactive.");
            }

            // Deactivate the meeting
            meeting.IsActive = false;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated meeting
            return Ok(meeting);
        }

        // PUT: api/meetings/1/activate
        // Activates a previously deactivated meeting
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateMeeting(int id)
        {
            // Find the meeting
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the meeting does not exist, return 404
            if (meeting == null)
            {
                return NotFound("Meeting does not exist.");
            }

            // If the meeting is already active, return a bad request
            if (meeting.IsActive)
            {
                return BadRequest("Meeting is already active.");
            }

            // Activate the meeting
            meeting.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated meeting
            return Ok(meeting);
        }
    }
}