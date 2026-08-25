using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Notification;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public NotificationsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/notifications
        // Gets all active notifications by default
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(
            bool includeInactive = false)
        {
            // Start with all notifications
            var query = _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .AsQueryable();

            // By default, return only active notifications
            if (!includeInactive)
            {
                query = query.Where(n => n.IsActive);
            }

            // Execute the query
            var notifications = await query.ToListAsync();

            return Ok(notifications);
        }


        // GET: api/notifications/1
        // Gets one notification by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            // Find the notification
            var notification = await _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .FirstOrDefaultAsync(n => n.Id == id);

            // If the notification does not exist, return 404
            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // Return the notification
            return Ok(notification);
        }


        // GET: api/notifications/student/3
        // Gets all active notifications for a specific student
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetStudentNotifications(
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

            // Get active notifications for the student
            var notifications = await _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .Where(n => n.StudentId == studentId && n.IsActive)
                .ToListAsync();

            return Ok(notifications);
        }


        // GET: api/notifications/parent/2
        // Gets all active notifications for a specific parent
        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetParentNotifications(
            int parentId)
        {
            // Check whether the parent exists
            var parentExists = await _context.Parents
                .AnyAsync(p => p.Id == parentId);

            // If the parent does not exist, return 404
            if (!parentExists)
            {
                return NotFound("Parent does not exist.");
            }

            // Get active notifications for the parent
            var notifications = await _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .Where(n => n.ParentId == parentId && n.IsActive)
                .ToListAsync();

            return Ok(notifications);
        }

        // POST: api/notifications
        // Creates a new notification
        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification(
            CreateNotificationDto dto)
        {
            // Check whether the student exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Notifications should only be created for active students
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Check whether the parent exists
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == dto.ParentId);

            // If the parent does not exist, return 404
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }


            // Validate the notification channel
            if (dto.Channel != "SMS" &&
                dto.Channel != "Email")
            {
                return BadRequest("Invalid channel. Use SMS or Email.");
            }

            // Validate the notification type
            if (string.IsNullOrWhiteSpace(dto.NotificationType))
            {
                return BadRequest("Notification type is required.");
            }

            // Validate the notification message
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest("Notification message is required.");
            }

            // Create the notification entity
            var notification = new Notification
            {
                StudentId = dto.StudentId,
                ParentId = dto.ParentId,
                NotificationType = dto.NotificationType,
                Message = dto.Message,
                Channel = dto.Channel,

                // New notifications always start as Pending
                Status = "Pending",

                // CreatedAt is automatically set by the model
                CreatedAt = DateTime.UtcNow,

                // No successful sending has happened yet
                SentAt = null,

                // New notification is active
                IsActive = true
            };

            // Add the notification to the database
            _context.Notifications.Add(notification);

            // Save the notification
            await _context.SaveChangesAsync();

            // Return the newly created notification
            return Ok(notification);
        }


        // PUT: api/notifications/1
        // Updates an existing notification
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(
            int id,
            UpdateNotificationDto dto)
        {
            // Find the existing notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            // If the notification does not exist, return 404
            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // Validate the notification channel
            if (dto.Channel != "SMS" &&
                dto.Channel != "Email")
            {
                return BadRequest("Invalid channel. Use SMS or Email.");
            }

            // Validate the notification status
            if (dto.Status != "Pending" &&
                dto.Status != "Sent" &&
                dto.Status != "Failed")
            {
                return BadRequest(
                    "Invalid notification status. Use Pending, Sent, or Failed.");
            }

            // Notification type is required
            if (string.IsNullOrWhiteSpace(dto.NotificationType))
            {
                return BadRequest("Notification type is required.");
            }

            // Notification message is required
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest("Notification message is required.");
            }

            // If the notification is marked as Sent,
            // SentAt should contain the sending time
            if (dto.Status == "Sent" && dto.SentAt == null)
            {
                return BadRequest(
                    "SentAt is required when notification status is Sent.");
            }

            // If the notification is not Sent,
            // there should not be a successful sending time
            if (dto.Status != "Sent")
            {
                dto.SentAt = null;
            }

            // Update notification information
            notification.NotificationType = dto.NotificationType;
            notification.Message = dto.Message;
            notification.Channel = dto.Channel;
            notification.Status = dto.Status;
            notification.SentAt = dto.SentAt;

            // Save changes
            await _context.SaveChangesAsync();

            // Return updated notification
            return Ok(notification);
        }


        // PUT: api/notifications/1/deactivate
        // Deactivates a notification without deleting it
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateNotification(int id)
        {
            // Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            // If the notification does not exist, return 404
            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // If the notification is already inactive, return a bad request
            if (!notification.IsActive)
            {
                return BadRequest("Notification is already inactive.");
            }

            // Deactivate the notification
            notification.IsActive = false;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated notification
            return Ok(notification);
        }

        // PUT: api/notifications/1/activate
        // Activates a previously deactivated notification
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateNotification(int id)
        {
            // Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            // If the notification does not exist, return 404
            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // If the notification is already active, return a bad request
            if (notification.IsActive)
            {
                return BadRequest("Notification is already active.");
            }

            // Activate the notification
            notification.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated notification
            return Ok(notification);
        }
    }
}