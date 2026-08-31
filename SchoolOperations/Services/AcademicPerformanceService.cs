using Microsoft.EntityFrameworkCore;
using SchoolOperations.Data;
using SchoolOperations.DTOs.AcademicPerformance;

namespace SchoolOperations.Services
{
    public class AcademicPerformanceService
        : IAcademicPerformanceService
    {
        private readonly SchoolDbContext _context;

        public AcademicPerformanceService(
            SchoolDbContext context)
        {
            _context = context;
        }

        private static string NormalizeSubject(string subject)
        {
            return subject
                .Trim()
                .ToLower();
        }

        public async Task<List<PoorPerformanceSummaryDto>>
            GetStudentsBelowPerformanceThresholdAsync(
                decimal threshold,
                string? subject)
        {
            // Start with active academic records
            var query = _context.AcademicPerformances
                .Include(a => a.Student)
                .Where(a =>
                    a.IsActive &&
                    a.Student.IsActive)
                .AsQueryable();

            // Filter by subject when one is provided
            if (!string.IsNullOrWhiteSpace(subject))
            {
                var normalizedSubject =
                    NormalizeSubject(subject);

                query = query.Where(a =>
                    a.Subject.Trim().ToLower() ==
                    normalizedSubject);
            }

            // Load the matching academic records
            var records = await query.ToListAsync();

            // Group records by student and subject
            var results = records
                .GroupBy(a => new
                {
                    a.StudentId,
                    a.Subject
                })
                .Select(group =>
                {
                    // Get the latest exam for this student and subject
                    var latestRecord = group
                        .OrderByDescending(a => a.ExamDate)
                        .ThenByDescending(a => a.Id)
                        .First();

                    // Calculate the latest performance percentage
                    var latestPercentage =
                        (latestRecord.MarksObtained /
                         latestRecord.MaximumMarks) * 100;

                    return new
                    {
                        Record = latestRecord,
                        Percentage = latestPercentage
                    };
                })

                // Keep only students below the threshold
                .Where(x => x.Percentage < threshold)

                // Convert the result into an analytics DTO
                .Select(x => new PoorPerformanceSummaryDto
                {
                    StudentId = x.Record.StudentId,

                    StudentName =
                        x.Record.Student.FirstName + " " +
                        x.Record.Student.LastName,

                    Subject = x.Record.Subject,

                    LatestExamName = x.Record.ExamName,

                    LatestExamDate = x.Record.ExamDate,

                    LatestPercentage =
                        Math.Round(x.Percentage, 2)
                })
                .ToList();

            return results;
        }





        public async Task<List<DecliningPerformanceSummaryDto>>
    GetStudentsWithDecliningPerformanceAsync(
        string? subject)
        {
            // Start with active academic records belonging to active students
            var query = _context.AcademicPerformances
                .Include(a => a.Student)
                .Where(a =>
                    a.IsActive &&
                    a.Student.IsActive)
                .AsQueryable();

            // Filter by subject when one is provided
            if (!string.IsNullOrWhiteSpace(subject))
            {
                var normalizedSubject =
                    NormalizeSubject(subject);

                query = query.Where(a =>
                    a.Subject.Trim().ToLower() ==
                    normalizedSubject);
            }

            // Load the matching academic records
            var records = await query.ToListAsync();

            // Analyze each student and subject separately
            var results = records
                .GroupBy(a => new
                {
                    a.StudentId,
                    a.Subject
                })
                .Where(group =>
                    group.Count() >= 2)
                .Select(group =>
                {
                    // Order exams chronologically
                    var orderedRecords = group
                        .OrderBy(a => a.ExamDate)
                        .ThenBy(a => a.Id)
                        .ToList();

                    // Get the latest two examinations
                    var previousRecord =
                        orderedRecords[^2];

                    var latestRecord =
                        orderedRecords[^1];

                    // Calculate normalized percentages
                    var previousPercentage =
                        (previousRecord.MarksObtained /
                         previousRecord.MaximumMarks) * 100;

                    var latestPercentage =
                        (latestRecord.MarksObtained /
                         latestRecord.MaximumMarks) * 100;

                    // Calculate the change between exams
                    var percentageChange =
                        latestPercentage - previousPercentage;

                    return new
                    {
                        PreviousRecord = previousRecord,
                        LatestRecord = latestRecord,
                        PreviousPercentage = previousPercentage,
                        LatestPercentage = latestPercentage,
                        PercentageChange = percentageChange
                    };
                })

                // Keep only students whose latest performance declined
                .Where(x =>
                    x.LatestPercentage <
                    x.PreviousPercentage)

                // Convert the result into the analytics DTO
                .Select(x =>
                    new DecliningPerformanceSummaryDto
                    {
                        StudentId =
                            x.LatestRecord.StudentId,

                        StudentName =
                            x.LatestRecord.Student.FirstName + " " +
                            x.LatestRecord.Student.LastName,

                        Subject =
                            x.LatestRecord.Subject,


                        PreviousExamName =
                            x.PreviousRecord.ExamName,

                        PreviousExamDate =
                            x.PreviousRecord.ExamDate,

                        PreviousPercentage =
                            Math.Round(
                                x.PreviousPercentage,
                                2),


                        LatestExamName =
                            x.LatestRecord.ExamName,

                        LatestExamDate =
                            x.LatestRecord.ExamDate,

                        LatestPercentage =
                            Math.Round(
                                x.LatestPercentage,
                                2),


                        PercentageChange =
                            Math.Round(
                                x.PercentageChange,
                                2)
                    })
                .ToList();

            return results;
        }
    }
}