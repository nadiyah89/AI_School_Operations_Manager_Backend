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

        // Represents the AdmissionApplications table in the database
        public DbSet<AdmissionApplication> AdmissionApplications { get; set; }

        // Represents the Documents table in the database
        public DbSet<Document> Documents { get; set; }

        // Represents the AcademicPerformance table in the database
        public DbSet<AcademicPerformance> AcademicPerformances { get; set; }

        // Represents the FeeRecords table in the database
        public DbSet<FeeRecord> FeeRecords { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for academic marks
            modelBuilder.Entity<AcademicPerformance>()
                .Property(a => a.MarksObtained)
                .HasPrecision(5, 2);

            modelBuilder.Entity<AcademicPerformance>()
                .Property(a => a.MaximumMarks)
                .HasPrecision(5, 2);


            // Configure decimal precision for Fee amounts
            modelBuilder.Entity<FeeRecord>()
                .Property(f => f.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<FeeRecord>()
                .Property(f => f.PaidAmount)
                .HasPrecision(10, 2);
        }
    }
}
