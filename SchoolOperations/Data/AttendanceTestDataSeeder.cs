using Microsoft.EntityFrameworkCore;
using SchoolOperations.Models;

namespace SchoolOperations.Data
{
    public static class AttendanceTestDataSeeder
    {
        public static async Task SeedAttendanceAsync(
            SchoolDbContext context)
        {
            // Get the first six active students.
            var students = await context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.Id)
                .Take(6)
                .ToListAsync();

            // We need at least six students for our test scenarios.
            if (students.Count < 6)
            {
                Console.WriteLine(
                    "Attendance test data was not seeded. " +
                    "At least 6 active students are required.");

                return;
            }

            // Use a fixed date range so the seed data is deterministic.
            var startDate = new DateTime(2026, 8, 1);

            // Attendance patterns:
            //
            // Student 1 -> 18/20 = 90%
            // Student 2 -> 15/20 = 75%
            // Student 3 -> 14/20 = 70%
            // Student 4 -> 10/20 = 50%
            // Student 5 -> 9/10  = 90%
            // Student 6 -> 0 records

            var testStudents = new[]
            {
                new
                {
                    Student = students[0],
                    TotalDays = 20,
                    PresentDays = 18
                },
                new
                {
                    Student = students[1],
                    TotalDays = 20,
                    PresentDays = 15
                },
                new
                {
                    Student = students[2],
                    TotalDays = 20,
                    PresentDays = 14
                },
                new
                {
                    Student = students[3],
                    TotalDays = 20,
                    PresentDays = 10
                },
                new
                {
                    Student = students[4],
                    TotalDays = 10,
                    PresentDays = 9
                }
            };

            foreach (var testStudent in testStudents)
            {
                // Check whether this student's test attendance
                // has already been seeded.
                var alreadySeeded = await context.Attendances
                    .AnyAsync(a =>
                        a.StudentId == testStudent.Student.Id &&
                        a.Date >= startDate &&
                        a.Date < startDate.AddDays(20));

                if (alreadySeeded)
                {
                    continue;
                }

                for (int day = 0;
                     day < testStudent.TotalDays;
                     day++)
                {
                    // First N days are present.
                    // Remaining days are absent.
                    var isPresent =
                        day < testStudent.PresentDays;

                    var attendance = new Attendance
                    {
                        StudentId = testStudent.Student.Id,

                        Date = startDate.AddDays(day),

                        IsPresent = isPresent,

                        IsActive = true
                    };

                    context.Attendances.Add(attendance);
                }
            }

            await context.SaveChangesAsync();

            Console.WriteLine(
                "Attendance test data seeding completed.");
        }
    }
}