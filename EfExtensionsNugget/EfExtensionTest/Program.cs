using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using Ralevex.EF.Data;
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
            var relativeDbPath = @"..\..\..\..\EfTestDb\Database\EFTestLdb_data.mdf";
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
            using (var ctx = new TestDbContext(ConnectionString))
                {

                var st = ctx.StudentsSet.Where(s => s.LastName == "Rurik")
                                        .SelectMany(sr => sr.RegistrationsCollection);




                Console.WriteLine("=========== Initial SELECT ==========");
                st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));

                Console.WriteLine("=========== Performing UPDATE ==========");
                st.Update(new { RegistrationDate = new DateTime(1925, 9, 20) });

                Console.WriteLine("=========== ????? ==========");
                var objectContext = ((IObjectContextAdapter)ctx).ObjectContext;


                Console.WriteLine("=========== REFRESH ==========");
                objectContext.Refresh(RefreshMode.StoreWins, st);

                Console.WriteLine("=========== Secondary Enumeration ==========");
                st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));
               
                }

            Console.WriteLine("\r\nDone");
            Console.ReadLine();
            }
        }
    }
