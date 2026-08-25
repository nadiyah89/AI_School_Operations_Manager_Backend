using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.FeeRecord;
using SchoolOperations.Models;

namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeRecordsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public FeeRecordsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/feerecords
        // Gets all active fee records by default
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeeRecord>>> GetFeeRecords(
            bool includeInactive = false)
        {
            // Start with all fee records
            var query = _context.FeeRecords
                .Include(f => f.Student)
                .AsQueryable();

            // By default, return only active fee records
            if (!includeInactive)
            {
                query = query.Where(f => f.IsActive);
            }

            // Execute the query
            var fees = await query.ToListAsync();

            return Ok(fees);
        }


        // GET: api/feerecords/1
        // Gets one fee record by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<FeeRecord>> GetFeeRecord(int id)
        {
            // Find the fee record
            var fee = await _context.FeeRecords
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.Id == id);

            // If the fee record does not exist, return 404
            if (fee == null)
            {
                return NotFound("Fee record does not exist.");
            }

            // Return the fee record
            return Ok(fee);
        }


        // GET: api/feerecords/student/3
        // Gets all active fee records for a specific student
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<FeeRecord>>> GetStudentFeeRecords(
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

            // Get active fee records for the student
            var fees = await _context.FeeRecords
                .Include(f => f.Student)
                .Where(f => f.StudentId == studentId && f.IsActive)
                .ToListAsync();

            return Ok(fees);
        }


        // POST: api/feerecords
        // Creates a new fee record
        [HttpPost]
        public async Task<ActionResult<FeeRecord>> CreateFeeRecord(
            CreateFeeRecordDto dto)
        {
            // Check whether the student exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            // If the student does not exist, return 404
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            // New fee records should only be created for active students
            if (!student.IsActive)
            {
                return BadRequest("Student is inactive.");
            }

            // Fee amount must be greater than zero
            if (dto.Amount <= 0)
            {
                return BadRequest("Fee amount must be greater than zero.");
            }

            // Paid amount cannot be negative
            if (dto.PaidAmount < 0)
            {
                return BadRequest("Paid amount cannot be negative.");
            }

            // Paid amount cannot be greater than the total fee
            if (dto.PaidAmount > dto.Amount)
            {
                return BadRequest("Paid amount cannot be greater than the fee amount.");
            }

            // Calculate the payment status
            string status;

            if (dto.PaidAmount == 0)
            {
                status = "Pending";
            }
            else if (dto.PaidAmount < dto.Amount)
            {
                status = "Partially Paid";
            }
            else
            {
                status = "Paid";
            }

            // Create the fee entity
            var fee = new FeeRecord
            {
                StudentId = dto.StudentId,
                FeeType = dto.FeeType,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                PaidAmount = dto.PaidAmount,
                PaymentDate = dto.PaymentDate,
                Status = status,
                IsActive = true
            };

            // Add the fee record to the database
            _context.FeeRecords.Add(fee);

            // Save the record to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created fee record
            return Ok(fee);
        }



        // PUT: api/feerecords/1
        // Updates an existing fee record
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeeRecord(
            int id,
            UpdateFeeRecordDto dto)
        {
            // Find the existing fee record
            var fee = await _context.FeeRecords
                .FirstOrDefaultAsync(f => f.Id == id);

            // If the fee record does not exist, return 404
            if (fee == null)
            {
                return NotFound("Fee record does not exist.");
            }

            // Fee amount must be greater than zero
            if (dto.Amount <= 0)
            {
                return BadRequest("Fee amount must be greater than zero.");
            }

            // Paid amount cannot be negative
            if (dto.PaidAmount < 0)
            {
                return BadRequest("Paid amount cannot be negative.");
            }

            // Paid amount cannot be greater than the total fee
            if (dto.PaidAmount > dto.Amount)
            {
                return BadRequest("Paid amount cannot be greater than the fee amount.");
            }

            // Calculate the payment status
            string status;

            if (dto.PaidAmount == 0)
            {
                status = "Pending";
            }
            else if (dto.PaidAmount < dto.Amount)
            {
                status = "Partially Paid";
            }
            else
            {
                status = "Paid";
            }

            // Update the fee information
            fee.FeeType = dto.FeeType;
            fee.Amount = dto.Amount;
            fee.DueDate = dto.DueDate;
            fee.PaidAmount = dto.PaidAmount;
            fee.PaymentDate = dto.PaymentDate;
            fee.Status = status;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return the updated fee record
            return Ok(fee);
        }


        // PUT: api/feerecords/1/deactivate
        // Deactivates a fee record without deleting it
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateFeeRecord(int id)
        {
            // Find the fee record
            var fee = await _context.FeeRecords
                .FirstOrDefaultAsync(f => f.Id == id);

            // If the fee record does not exist, return 404
            if (fee == null)
            {
                return NotFound("Fee record does not exist.");
            }

            // If the fee record is already inactive, return a bad request
            if (!fee.IsActive)
            {
                return BadRequest("Fee record is already inactive.");
            }

            // Deactivate the fee record
            fee.IsActive = false;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated fee record
            return Ok(fee);
        }

        // PUT: api/feerecords/2/activate
        // Activates a previously deactivated fee record
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateFeeRecord(int id)
        {
            // Find the fee record
            var fee = await _context.FeeRecords
                .FirstOrDefaultAsync(f => f.Id == id);

            // If the fee record does not exist, return 404
            if (fee == null)
            {
                return NotFound("Fee record does not exist.");
            }

            // If the fee record is already active, return a bad request
            if (fee.IsActive)
            {
                return BadRequest("Fee record is already active.");
            }

            // Activate the fee record
            fee.IsActive = true;

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Return the updated fee record
            return Ok(fee);
        }
    }
}