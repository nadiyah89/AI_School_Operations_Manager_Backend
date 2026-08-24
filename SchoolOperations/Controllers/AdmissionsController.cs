using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Admission;
using SchoolOperations.Models;


namespace SchoolOperations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdmissionsController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public AdmissionsController(SchoolDbContext context)
        {
            _context = context;
        }


        // GET: api/admissions
        // Gets all admission applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdmissionApplication>>> GetAdmissions()
        {
            // Get all admission applications from the database
            var admissions = await _context.AdmissionApplications
                .ToListAsync();

            // Return the admission applications
            return Ok(admissions);
        }



        // GET: api/admissions/1
        // Gets a single admission application by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AdmissionApplication>> GetAdmission(int id)
        {
            // Find the admission application with the given ID
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Return the admission application
            return Ok(admission);
        }



        // POST: api/admissions
        // Creates a new admission application
        [HttpPost]
        public async Task<ActionResult<AdmissionApplication>> CreateAdmission(
            CreateAdmissionDto dto)
        {
            // Create a new admission application from the DTO
            var admission = new AdmissionApplication
            {
                ApplicantFirstName = dto.ApplicantFirstName,
                ApplicantLastName = dto.ApplicantLastName,
                DateOfBirth = dto.DateOfBirth,
                ApplyingForClass = dto.ApplyingForClass,
                ParentName = dto.ParentName,
                ParentPhoneNumber = dto.ParentPhoneNumber,
                ParentEmail = dto.ParentEmail,

                // The backend controls these values
                ApplicationDate = DateTime.UtcNow,
                Status = "Pending"
            };

            // Add the application to the database context
            _context.AdmissionApplications.Add(admission);

            // Save the application to SQL Server
            await _context.SaveChangesAsync();

            // Return the newly created application
            return Ok(admission);
        }


        // PUT: api/admissions/1
        // Updates an existing admission application
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmission(
            int id,
            UpdateAdmissionDto dto)
        {
            // Find the existing admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Update the applicant's information
            admission.ApplicantFirstName = dto.ApplicantFirstName;
            admission.ApplicantLastName = dto.ApplicantLastName;
            admission.DateOfBirth = dto.DateOfBirth;
            admission.ApplyingForClass = dto.ApplyingForClass;

            // Update parent/guardian information
            admission.ParentName = dto.ParentName;
            admission.ParentPhoneNumber = dto.ParentPhoneNumber;
            admission.ParentEmail = dto.ParentEmail;

            // Save the changes to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated application
            return Ok(admission);
        }


        // PUT: api/admissions/1/approve
        // Approves a pending admission application
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveAdmission(int id)
        {
            // Find the admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Only pending applications can be approved
            if (admission.Status != "Pending")
            {
                return BadRequest(
                    $"Admission application is already {admission.Status}.");
            }

            // Change the application status
            admission.Status = "Approved";

            // Save the change to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated application
            return Ok(admission);
        }


        // PUT: api/admissions/1/reject
        // Rejects a pending admission application
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectAdmission(int id)
        {
            // Find the admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Only pending applications can be rejected
            if (admission.Status != "Pending")
            {
                return BadRequest(
                    $"Admission application is already {admission.Status}.");
            }

            // Change the application status
            admission.Status = "Rejected";

            // Save the change to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated application
            return Ok(admission);
        }


        // PUT: api/admissions/1/waitlist
        // Places a pending admission application on the waiting list
        [HttpPut("{id}/waitlist")]
        public async Task<IActionResult> WaitlistAdmission(int id)
        {
            // Find the admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Only pending applications can be waitlisted
            if (admission.Status != "Pending")
            {
                return BadRequest(
                    $"Admission application is already {admission.Status}.");
            }

            // Change the application status
            admission.Status = "Waitlisted";

            // Save the change to SQL Server
            await _context.SaveChangesAsync();

            // Return the updated application
            return Ok(admission);
        }



        // PUT: api/admissions/1/create-student
        // Creates an official Student from an approved admission application
        [HttpPut("{id}/create-student")]
        public async Task<IActionResult> CreateStudentFromAdmission(int id)
        {
            // Find the admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the admission application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Only approved applications can create students
            if (admission.Status != "Approved")
            {
                return BadRequest(
                    $"Student cannot be created because the admission application is {admission.Status}.");
            }

            // Prevent creating multiple students from the same application
            if (admission.StudentId != null)
            {
                return BadRequest(
                    "Student has already been created from this admission application.");
            }

            // Create the Student entity
            var student = new Student
            {
                FirstName = admission.ApplicantFirstName,
                LastName = admission.ApplicantLastName,
                DateOfBirth = admission.DateOfBirth,

                // New students are active by default
                IsActive = true
            };

            // Add the Student to the database
            _context.Students.Add(student);

            // Save first so SQL Server generates the Student ID
            await _context.SaveChangesAsync();

            // Connect the admission application to the newly created Student
            admission.StudentId = student.Id;

            // Save the relationship
            await _context.SaveChangesAsync();

            // Return the newly created Student
            return Ok(student);
        }



        // PUT: api/admissions/1/create-parent
        // Creates a Parent from an approved admission application
        [HttpPut("{id}/create-parent")]
        public async Task<IActionResult> CreateParentFromAdmission(
            int id,
            CreateParentFromAdmissionDto dto)
        {
            // Find the admission application
            var admission = await _context.AdmissionApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            // If the admission application does not exist, return 404
            if (admission == null)
            {
                return NotFound("Admission application does not exist.");
            }

            // Only approved applications can create parents
            if (admission.Status != "Approved")
            {
                return BadRequest(
                    $"Parent cannot be created because the admission application is {admission.Status}.");
            }

            // A Student must already exist
            if (admission.StudentId == null)
            {
                return BadRequest(
                    "Student must be created before creating the Parent.");
            }

            // Prevent creating multiple parents from the same admission
            if (admission.ParentId != null)
            {
                return BadRequest(
                    "Parent has already been created from this admission application.");
            }

            // Check that the Student still exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == admission.StudentId);

            if (student == null)
            {
                return NotFound(
                    "The Student associated with this admission does not exist.");
            }

            // Create the Parent entity
            var parent = new Parent
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Relationship = dto.Relationship,

                // Connect the Parent to the Student created from this admission
                StudentId = student.Id
            };

            // Add the Parent to the database
            _context.Parents.Add(parent);

            // Save first so SQL Server generates the Parent ID
            await _context.SaveChangesAsync();

            // Connect the admission application to the newly created Parent
            admission.ParentId = parent.Id;

            // Save the relationship
            await _context.SaveChangesAsync();

            // Return the newly created Parent
            return Ok(parent);
        }


    }
}