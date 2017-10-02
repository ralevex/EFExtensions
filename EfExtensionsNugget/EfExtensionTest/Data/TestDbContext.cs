using System;
using System.Data.Entity;
using Ralevex.EF.Data;

namespace Ralevex.EF
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(string connectionString) : base(connectionString)
        {
            // Disable EF code-first migration checks 
            Database.SetInitializer<TestDbContext>(null);
#if DEBUG
            //To enable tracing of ALL database commands -- uncomment next line
            Database.Log = Console.WriteLine;
#endif
        }
        public DbSet<Student>       StudentsSet         { get; set; }
        public DbSet<Course>        CoursesSet          { get; set; }
        public DbSet<Registration>  RegistrationsSet    { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Configuration.AutoDetectChangesEnabled = false;

            //modelBuilder.Entity<Student>()
            //    .HasMany(s => s.CoursesCollection)
            //    .WithMany(c => c.StudentCollection)
            //    .Map(cs =>
            //    {
            //        cs.MapLeftKey("StudentId");
            //        cs.MapRightKey("CourseId");
            //        cs.ToTable("Registrations");
            //    });

            base.OnModelCreating(modelBuilder);
        }
    }
}