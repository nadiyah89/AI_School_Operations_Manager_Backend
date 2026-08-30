using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.Attendance;

namespace SchoolOperations.Services
{
    public class AttendanceService
    {
        private readonly SchoolDbContext _context;

        public AttendanceService(SchoolDbContext context)
        {
            _context = context;
        }

        // Calculates attendance summaries for active students.
        public async Task<List<AttendanceSummaryDto>> GetAttendanceSummaryAsync(
            decimal? threshold = null)
        {
            // Get active students and their active attendance records.
            var query = _context.Students
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    StudentId = s.Id,

                    StudentName =
                        s.FirstName + " " + s.LastName,

                    TotalDays = _context.Attendances
                        .Count(a =>
                            a.StudentId == s.Id &&
                            a.IsActive),

                    PresentDays = _context.Attendances
                        .Count(a =>
                            a.StudentId == s.Id &&
                            a.IsActive &&
                            a.IsPresent)
                });

            // Execute the database query.
            var data = await query.ToListAsync();

            var summaries = new List<AttendanceSummaryDto>();

            foreach (var item in data)
            {
                // Students without attendance records
                // cannot have a meaningful percentage.
                if (item.TotalDays == 0)
                {
                    continue;
                }

                // Calculate absent days.
                var absentDays =
                    item.TotalDays - item.PresentDays;

                // Calculate attendance percentage.
                var attendancePercentage =
                    (decimal)item.PresentDays /
                    item.TotalDays *
                    100;

                // Apply threshold when supplied.
                if (threshold.HasValue &&
                    attendancePercentage >= threshold.Value)
                {
                    continue;
                }

                summaries.Add(new AttendanceSummaryDto
                {
                    StudentId = item.StudentId,

                    StudentName = item.StudentName,

                    TotalDays = item.TotalDays,

                    PresentDays = item.PresentDays,

                    AbsentDays = absentDays,

                    AttendancePercentage =
                        Math.Round(attendancePercentage, 2)
                });
            }

            return summaries;
        }
    }
}