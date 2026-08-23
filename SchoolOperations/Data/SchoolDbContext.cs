using Microsoft.EntityFrameworkCore;
using SchoolOperations.Models;

namespace SchoolOperations.Data
{
    public class SchoolDbContext : DbContext
    {

      public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
      {

      }
        // Represents the Students table in the database
        public DbSet<Student> Students { get; set; }

        // Represents the Attendances table in the database
        public DbSet<Attendance> Attendances { get; set; }

        // Represents the Parents table in the database
        public DbSet<Parent> Parents { get; set; }

        // Represents the Teachers table in the database
        public DbSet<Teacher> Teachers { get; set; }
    }
}
