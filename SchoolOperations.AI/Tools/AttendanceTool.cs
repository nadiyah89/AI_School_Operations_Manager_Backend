using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class AttendanceTool : ITool
{
    public string Name =>
        "GetStudentsBelowAttendanceThreshold";

    public string Description =>
        "Finds students whose attendance percentage " +
        "is below the specified threshold.";

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["threshold"] = new Schema
            {
                Type = "number",

                Description =
                    "Attendance percentage threshold. " +
                    "For example, 75 means students below 75%."
            }
        };

    public Task<object> ExecuteAsync(
        Dictionary<string, object> arguments)
    {
        // Read the threshold supplied by Gemini.
        var threshold =
            Convert.ToDouble(arguments["threshold"]);

        // Temporary fake data.
        // Later this will come from our real
        // Attendance API.
        var students = new[]
        {
            new
            {
                StudentId = 1,
                Name = "Ahmed",
                AttendancePercentage = 68
            },

            new
            {
                StudentId = 2,
                Name = "Sara",
                AttendancePercentage = 72
            },

            new
            {
                StudentId = 3,
                Name = "John",
                AttendancePercentage = 89
            }
        };

        // Apply deterministic filtering in C#.
        var result =
            students
                .Where(student =>
                    student.AttendancePercentage < threshold)
                .ToList();

        return Task.FromResult<object>(result);
    }
}