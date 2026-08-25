using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Meeting;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public MeetingsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/meetings
        // Gets all active meetings by default
        [HttpGet]
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
        public async Task<ActionResult<Meeting>> GetMeeting(int id)
        {
            // Find the meeting
            var meeting = await _context.Meetings
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the meeting does not exist, return 404
            if (meeting == null)
            {
                return NotFound("Meeting does not exist.");
            }

            // Return the meeting
            return Ok(meeting);
        }


        // GET: api/meetings/student/3
        // Gets all active meetings for a specific student
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetStudentMeetings(
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

            // Get active meetings for the student
            var meetings = await _context.Meetings
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .Where(m => m.StudentId == studentId && m.IsActive)
                .ToListAsync();

            return Ok(meetings);
        }


        // GET: api/meetings/teacher/1
        // Gets all active meetings for a specific teacher
        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetTeacherMeetings(
            int teacherId)
        {
            // Check whether the teacher exists
            var teacherExists = await _context.Teachers
                .AnyAsync(t => t.Id == teacherId);

            // If the teacher does not exist, return 404
            if (!teacherExists)
            {
                return NotFound("Teacher does not exist.");
            }

            // Get active meetings for the teacher
            var meetings = await _context.Meetings
                .Include(m => m.Student)
                .Include(m => m.Teacher)
                .Where(m => m.TeacherId == teacherId && m.IsActive)
                .ToListAsync();

            return Ok(meetings);
        }


        // POST: api/meetings
        // Creates a new meeting
        [HttpPost]
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