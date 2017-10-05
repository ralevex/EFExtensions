using System;
using System.Data.Entity;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using Ralevex.EF.Extensions;

namespace Ralevex.EF
    {
    public static class Program
        {
        public static void ForEach<T>(this IQueryable<T> source,Action<T> action)
            {
                foreach (T item in source)
                        action(item);
            }

        public static string ConnectionString;

        static Program()
            {
            var relativeDbPath = @"..\..\..\..\EfTestDb\Database\EFTestDb_data.mdf";
            var dbFileName = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeDbPath));
            var sb =
                    new SqlConnectionStringBuilder
                        {
                        AttachDBFilename = dbFileName,
                        DataSource = @"(localdb)\MsSqlLocalDb",
                        InitialCatalog = "EfTestDb",
                        IntegratedSecurity = true
                        };
            ConnectionString = sb.ConnectionString;
            }

        
        public static void Main()
            {
            var rng= new Random();
            using (var ctx = new TestDbContext(ConnectionString))
                {

                var studentRec = ctx.StudentsSet.First(s => s.LastName == "Petrov");

                var recordSet = (   from students in ctx.StudentsSet
                                    join registrations in ctx.RegistrationsSet 
                                        on students.StudentId equals registrations.StudentId
                                    where students.LastName == "Petrov"
                                    select registrations).AsNoTracking();

                Console.WriteLine(recordSet.ToTraceString());

                Console.ReadLine();
             
                

                Console.WriteLine("=========== Initial SELECT ==========");
 //               st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));

                Console.WriteLine("=========== Performing UPDATE ==========");
                recordSet.Update(new { RegistrationDate = new DateTime(rng.Next(1950, 2000), rng.Next(1, 13), rng.Next(1, 28)) });
                recordSet.Update(new { RegistrationDate = DateTime.UtcNow });



                Console.WriteLine("=========== Secondary Enumeration ==========");
                recordSet.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));


                Console.WriteLine("=========== Performing DELETE ==========");
                recordSet.Delete();

                Console.WriteLine("=========== Third Enumeration ==========");
                recordSet.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));
               
                }

            Console.WriteLine("\r\nDone");
            Console.ReadLine();
            }
        }
    }
