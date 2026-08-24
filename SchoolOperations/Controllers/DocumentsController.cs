using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Document;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public DocumentsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/documents
        // Gets active documents by default
        [HttpGet]
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
        // Gets a single document by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            // Find the document with the given ID
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If the document does not exist, return 404
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // Return the document
            return Ok(document);
        }


        // POST: api/documents
        // Creates a new document
        [HttpPost]
        public async Task<ActionResult<Document>> CreateDocument(
            CreateDocumentDto dto)
        {
            // Create the database entity from the DTO
            var document = new Document
            {
                Title = dto.Title,
                Category = dto.Category,
                Content = dto.Content
            };

            // Add the document to the database context
            _context.Documents.Add(document);

            // Save the document to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created document
            return Ok(document);
        }


        // PUT: api/documents/1
        // Updates an existing document
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(
            int id,
            UpdateDocumentDto dto)
        {
            // Find the existing document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If the document does not exist, return 404
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // Update the document information
            document.Title = dto.Title;
            document.Category = dto.Category;
            document.Content = dto.Content;

            // Update the modification timestamp
            document.UpdatedAt = DateTime.UtcNow;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return the updated document
            return Ok(document);
        }


        // PUT: api/documents/1/deactivate
        // Deactivates a document without deleting the database record
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateDocument(int id)
        {
            // Find the document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If the document does not exist, return 404
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // If the document is already inactive, return a bad request
            if (!document.IsActive)
            {
                return BadRequest("Document is already inactive.");
            }

            // Deactivate the document
            document.IsActive = false;

            // Update the modification timestamp
            document.UpdatedAt = DateTime.UtcNow;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated document
            return Ok(document);
        }


        // PUT: api/documents/1/activate
        // Activates a document that was previously deactivated
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateDocument(int id)
        {
            // Find the document
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // If the document does not exist, return 404
            if (document == null)
            {
                return NotFound("Document does not exist.");
            }

            // If the document is already active, return a bad request
            if (document.IsActive)
            {
                return BadRequest("Document is already active.");
            }

            // Activate the document
            document.IsActive = true;

            // Update the modification timestamp
            document.UpdatedAt = DateTime.UtcNow;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated document
            return Ok(document);
        }
    }
}