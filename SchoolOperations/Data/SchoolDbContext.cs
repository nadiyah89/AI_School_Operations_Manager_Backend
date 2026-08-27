using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolOperations.Models;

namespace SchoolOperations.Data
{
    public class SchoolDbContext : IdentityDbContext<ApplicationUser>
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
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

        // Represents the Meetings table in the database
        public DbSet<Meeting> Meetings { get; set; }

        // Represents the Notifications table in the database
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Required by ASP.NET Core Identity
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


            // Configure Meeting -> Student relationship
            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Student)
                .WithMany()
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Meeting -> Teacher relationship
            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Teacher)
                .WithMany()
                .HasForeignKey(m => m.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);


            // Configure Notification -> Student relationship
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Notification -> Parent relationship
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Parent)
                .WithMany()
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ApplicationUser -> Student relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Student)
                .WithMany()
                .HasForeignKey(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ApplicationUser -> Teacher relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Teacher)
                .WithMany()
                .HasForeignKey(u => u.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ApplicationUser -> Parent relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Parent)
                .WithMany()
                .HasForeignKey(u => u.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}