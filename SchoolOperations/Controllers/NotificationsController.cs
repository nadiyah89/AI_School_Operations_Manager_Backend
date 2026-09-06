using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Notification;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/notifications
        // Admin can view all notifications.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(
            bool includeInactive = false,
            string? status = null,
            int? studentId = null,
            int? parentId = null)
        {
            var query = _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .AsQueryable();

            // By default, return only active notifications.
            if (!includeInactive)
            {
                query = query.Where(n => n.IsActive);
            }

            // Filter by notification status when provided.
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(n => n.Status == status);
            }

            // Filter by student when provided.
            if (studentId.HasValue)
            {
                query = query.Where(n => n.StudentId == studentId.Value);
            }

            // Filter by parent when provided.
            if (parentId.HasValue)
            {
                query = query.Where(n => n.ParentId == parentId.Value);
            }

            var notifications = await query.ToListAsync();

            return Ok(notifications);
        }


        // GET: api/notifications/1
        // Admin can view any notification.
        // Student can view their own notification.
        // Parent can view their child's notification.
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.Student)
                .Include(n => n.Parent)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // Admin can view any notification.
            if (User.IsInRole("Admin"))
            {
                return Ok(notification);
            }

            // Non-admin users cannot access inactive notifications.
            if (!notification.IsActive)
            {
                return NotFound("Notification does not exist.");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can view only their own notification.
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != notification.StudentId)
                {
                    return Forbid();
                }

                return Ok(notification);
            }

            // Parent can view only notifications belonging
            // to their child.
            if (User.IsInRole("Parent"))
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Id == user.ParentId);

                if (parent == null)
                {
                    return Forbid();
                }

                if (parent.Id != notification.ParentId)
                {
                    return Forbid();
                }

                return Ok(notification);
            }

            // Teachers do not have notification viewing access.
            return Forbid();
        }


        // GET: api/notifications/student/3
        // Admin can view any student's notifications.
        // Student can view only their own notifications.
        // Parent can view their child's notifications.
        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Notification>>> GetStudentNotifications(
            int studentId)
        {
            // Check whether the student exists.
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Admin can view any student's notifications.
            if (User.IsInRole("Admin"))
            {
                var adminNotifications = await _context.Notifications
                    .Include(n => n.Student)
                    .Include(n => n.Parent)
                    .Where(n =>
                        n.StudentId == studentId &&
                        n.IsActive)
                    .ToListAsync();

                return Ok(adminNotifications);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Student can view only their own notifications.
            if (User.IsInRole("Student"))
            {
                if (user.StudentId != studentId)
                {
                    return Forbid();
                }

                var studentNotifications = await _context.Notifications
                    .Include(n => n.Student)
                    .Include(n => n.Parent)
                    .Where(n =>
                        n.StudentId == studentId &&
                        n.IsActive)
                    .ToListAsync();

                return Ok(studentNotifications);
            }

            // Parent can view notifications for their child.
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

                var parentNotifications = await _context.Notifications
                    .Include(n => n.Student)
                    .Include(n => n.Parent)
                    .Where(n =>
                        n.StudentId == studentId &&
                        n.IsActive)
                    .ToListAsync();

                return Ok(parentNotifications);
            }

            // Teachers do not have access.
            return Forbid();
        }


        // GET: api/notifications/parent/2
        // Admin can view any parent's notifications.
        // Parent can view only their own notifications.
        [HttpGet("parent/{parentId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Notification>>> GetParentNotifications(
            int parentId)
        {
            // Check whether the parent exists.
            var parentExists = await _context.Parents
                .AnyAsync(p => p.Id == parentId);

            if (!parentExists)
            {
                return NotFound("Parent does not exist.");
            }

            // Admin can view any parent's notifications.
            if (User.IsInRole("Admin"))
            {
                var adminNotifications = await _context.Notifications
                    .Include(n => n.Student)
                    .Include(n => n.Parent)
                    .Where(n =>
                        n.ParentId == parentId &&
                        n.IsActive)
                    .ToListAsync();

                return Ok(adminNotifications);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Parent can view only their own notifications.
            if (User.IsInRole("Parent"))
            {
                if (user.ParentId != parentId)
                {
                    return Forbid();
                }

                var parentNotifications = await _context.Notifications
                    .Include(n => n.Student)
                    .Include(n => n.Parent)
                    .Where(n =>
                        n.ParentId == parentId &&
                        n.IsActive)
                    .ToListAsync();

                return Ok(parentNotifications);
            }

            // Students and Teachers cannot use this endpoint.
            return Forbid();
        }


        // POST: api/notifications
        // Admin and Teacher can create notifications.
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Notification>> CreateNotification(
            CreateNotificationDto dto)
        {
            // Check whether the student exists.
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Notifications should only be created for active students.
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Check whether the parent exists.
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == dto.ParentId);

            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // Make sure the parent actually belongs to the student.
            if (parent.StudentId != dto.StudentId)
            {
                return BadRequest(
                    "The selected parent does not belong to the selected student.");
            }

            // Validate notification channel.
            if (dto.Channel != "SMS" &&
                dto.Channel != "Email")
            {
                return BadRequest("Invalid channel. Use SMS or Email.");
            }

            // Validate notification type.
            if (string.IsNullOrWhiteSpace(dto.NotificationType))
            {
                return BadRequest("Notification type is required.");
            }

            // Validate notification message.
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest("Notification message is required.");
            }

            // Create the notification.
            var notification = new Notification
            {
                StudentId = dto.StudentId,
                ParentId = dto.ParentId,
                NotificationType = dto.NotificationType,
                Message = dto.Message,
                Channel = dto.Channel,

                // New notifications start as Pending.
                Status = "Pending",

                CreatedAt = DateTime.UtcNow,

                SentAt = null,

                IsActive = true
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(notification);
        }


        // PUT: api/notifications/1
        // Only Admin can update notifications.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotification(
            int id,
            UpdateNotificationDto dto)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            // Validate channel.
            if (dto.Channel != "SMS" &&
                dto.Channel != "Email")
            {
                return BadRequest("Invalid channel. Use SMS or Email.");
            }

            // Validate status.
            if (dto.Status != "Pending" &&
                dto.Status != "Sent" &&
                dto.Status != "Failed")
            {
                return BadRequest(
                    "Invalid notification status. Use Pending, Sent, or Failed.");
            }

            // Notification type is required.
            if (string.IsNullOrWhiteSpace(dto.NotificationType))
            {
                return BadRequest("Notification type is required.");
            }

            // Notification message is required.
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest("Notification message is required.");
            }

            // Sent notifications must have SentAt.
            if (dto.Status == "Sent" && dto.SentAt == null)
            {
                return BadRequest(
                    "SentAt is required when notification status is Sent.");
            }

            // Pending and Failed notifications should not
            // contain a successful sending time.
            if (dto.Status != "Sent")
            {
                dto.SentAt = null;
            }

            notification.NotificationType = dto.NotificationType;
            notification.Message = dto.Message;
            notification.Channel = dto.Channel;
            notification.Status = dto.Status;
            notification.SentAt = dto.SentAt;

            await _context.SaveChangesAsync();

            return Ok(notification);
        }


        // PUT: api/notifications/1/deactivate
        // Only Admin can deactivate notifications.
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateNotification(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            if (!notification.IsActive)
            {
                return BadRequest("Notification is already inactive.");
            }

            notification.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(notification);
        }


        // PUT: api/notifications/1/activate
        // Only Admin can activate notifications.
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateNotification(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound("Notification does not exist.");
            }

            if (notification.IsActive)
            {
                return BadRequest("Notification is already active.");
            }

            notification.IsActive = true;

            await _context.SaveChangesAsync();

            return Ok(notification);
        }
    }
}