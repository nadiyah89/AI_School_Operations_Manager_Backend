using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Document;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public DocumentsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/documents
        // Admin, Teacher, Parent and Student can view documents
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments(
            bool includeInactive = false)
        {
            // Start with all documents
            var query = _context.Documents.AsQueryable();

            // By default, return only active documents
            if (!includeInactive)
            {
                query = query.Where(d => d.IsActive);
            }

            // Execute the query
            var documents = await query.ToListAsync();

            return Ok(documents);
        }


        // GET: api/documents/1
        // Gets one document
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            // Find the document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If document does not exist
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            return Ok(document);
        }


        // POST: api/documents
        // Only Admin can create documents
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Document>> CreateDocument(
            CreateDocumentDto dto)
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Document title is required.");
            }

            // Validate category
            if (string.IsNullOrWhiteSpace(dto.Category))
            {
                return BadRequest("Document category is required.");
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Document content is required.");
            }

            // Create document entity
            var document = new Document
            {
                Title = dto.Title,
                Category = dto.Category,
                Content = dto.Content,

                // New documents are active
                IsActive = true,

                // CreatedAt and UpdatedAt are set automatically
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add document
            _context.Documents.Add(document);

            // Save to database
            await _context.SaveChangesAsync();

            return Ok(document);
        }


        // PUT: api/documents/1
        // Only Admin can update documents
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDocument(
            int id,
            UpdateDocumentDto dto)
        {
            // Find existing document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If document does not exist
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Document title is required.");
            }

            // Validate category
            if (string.IsNullOrWhiteSpace(dto.Category))
            {
                return BadRequest("Document category is required.");
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Document content is required.");
            }

            // Update document
            document.Title = dto.Title;
            document.Category = dto.Category;
            document.Content = dto.Content;

            // Update modification time
            document.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _context.SaveChangesAsync();

            return Ok(document);
        }


        // PUT: api/documents/1/deactivate
        // Only Admin can deactivate
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateDocument(int id)
        {
            // Find document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // Check current state
            if (!document.IsActive)
            {
                return BadRequest("Document is already inactive.");
            }

            // Deactivate
            document.IsActive = false;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(document);
        }


        // PUT: api/documents/1/activate
        // Only Admin can activate
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateDocument(int id)
        {
            // Find document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // Check current state
            if (document.IsActive)
            {
                return BadRequest("Document is already active.");
            }

            // Activate
            document.IsActive = true;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(document);
        }
    }
}