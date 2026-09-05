using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Parent;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParentsController(
            SchoolDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/parents
        // Gets all active parents
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Parent>>> GetParents(
           bool includeInactive = false,
           string? search = null)
        {
            // Start with all parents
            var query = _context.Parents
                .Include(p => p.Student)
                .AsQueryable();

            // By default, return only active parents
            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            // Apply optional name search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search));
            }

            // Execute the query
            var parents = await query.ToListAsync();

            return Ok(parents);
        }


        // GET: api/parents/1
        // Gets one parent by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetParent(int id)
        {
            // Find the parent
            var parent = await _context.Parents
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            // If the parent does not exist
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // Admin can view any parent, including inactive parents
            if (User.IsInRole("Admin"))
            {
                return Ok(parent);
            }

            // Non-admin users cannot access inactive parents
            if (!parent.IsActive)
            {
                return NotFound("Parent does not exist.");
            }

            // Teacher can view any active parent
            if (User.IsInRole("Teacher"))
            {
                return Ok(parent);
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Parent can view only their own parent record
            if (User.IsInRole("Parent"))
            {
                if (user.ParentId != parent.Id)
                {
                    return Forbid();
                }

                return Ok(parent);
            }

            // Students cannot access parent records
            return Forbid();
        }


        // GET: api/parents/student/3
        // Gets active parents for a specific student
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<Parent>>> GetParentsByStudent(
            int studentId)
        {
            // Check whether the student exists
            var studentExists = await _context.Students
                .AnyAsync(s => s.Id == studentId);

            // If the student does not exist
            if (!studentExists)
            {
                return NotFound("Student does not exist.");
            }

            // Get active parents belonging to the student
            var parents = await _context.Parents
                .Include(p => p.Student)
                .Where(p =>
                    p.StudentId == studentId &&
                    p.IsActive)
                .ToListAsync();

            return Ok(parents);
        }


        // POST: api/parents
        // Creates a new parent
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Parent>> CreateParent(
            CreateParentDto dto)
        {
            // Find the student
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // Check whether the student exists
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // Check whether the student is active
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Create a new Parent entity
            var parent = new Parent
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Relationship = dto.Relationship,
                StudentId = dto.StudentId,

                // New parents are active by default
                IsActive = true
            };

            // Add the parent to the database
            _context.Parents.Add(parent);

            // Save the changes
            await _context.SaveChangesAsync();

            // Return the newly created parent
            return Ok(parent);
        }


        // PUT: api/parents/1
        // Updates an existing parent
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateParent(
            int id,
            UpdateParentDto dto)
        {
            // Find the existing parent
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == id);

            // If the parent does not exist
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // Update parent information
            parent.FirstName = dto.FirstName;
            parent.LastName = dto.LastName;
            parent.PhoneNumber = dto.PhoneNumber;
            parent.Email = dto.Email;
            parent.Relationship = dto.Relationship;

            // Save the changes
            await _context.SaveChangesAsync();

            // Return the updated parent
            return Ok(parent);
        }


        // PUT: api/parents/1/deactivate
        // Deactivates a parent without deleting the database record
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateParent(int id)
        {
            // Find the parent
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == id);

            // If the parent does not exist
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // If the parent is already inactive
            if (!parent.IsActive)
            {
                return BadRequest("Parent is already inactive.");
            }

            // Deactivate the parent
            parent.IsActive = false;

            // Save the change
            await _context.SaveChangesAsync();

            // Return the updated parent
            return Ok(parent);
        }


        // PUT: api/parents/1/activate
        // Activates a previously deactivated parent
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateParent(int id)
        {
            // Find the parent
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == id);

            // If the parent does not exist
            if (parent == null)
            {
                return NotFound("Parent does not exist.");
            }

            // If the parent is already active
            if (parent.IsActive)
            {
                return BadRequest("Parent is already active.");
            }

            // Activate the parent
            parent.IsActive = true;

            // Save the change
            await _context.SaveChangesAsync();

            // Return the updated parent
            return Ok(parent);
        }
    }
}