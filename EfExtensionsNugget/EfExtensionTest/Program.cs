using System;
using System.Data.Entity;
using System.Linq;
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
            var rng= new Random();
            using (var ctx = new TestDbContext(ConnectionString))
                {

                //var carouselData = from carousel in ctx.LandingPageCarouselSet
                //                   join page in ctx.LandingPageCarouselPageVariantSet
                //                       on carousel.LandingPageCarouselPageId equals page.LandingPageCarouselPageId
                //                   join bgAsset in ctx.LandingPageAssetSet
                //                       on page.BackgroundAssetId equals bgAsset.LandingPageAssetId
                //                   join iconAsset in ctx.LandingPageAssetSet
                //                       on page.BrandIconAssetId equals iconAsset.LandingPageAssetId
                //                   where carousel.LandingPageCarouselId == carouselId
                //                   where page.LanguageId == languageid
                //                   where page.ClientTypeId == 3
                //                   orderby carousel.Position
                //                   select new
                //                       {
                //                       Header = page.HeaderText,
                //                       carousel.Position,
                //                       Content = page.ContentText,
                //                       PageId = page.LandingPageCarouselPageId,
                //                       BgSource = bgAsset.Data,
                //                       IconSource = iconAsset.Data
                //                       };
                var oneSt = ctx.StudentsSet.First(s => s.LastName == "Grozniy");

                var st = (from students in ctx.StudentsSet
                         join registrations in ctx.RegistrationsSet on students.StudentId equals registrations.StudentId
                         where students.StudentId == oneSt.StudentId
                         select registrations).AsNoTracking();



                //var st = ctx.StudentsSet.Where(s => s.LastName == "Rurik")
                //                        .SelectMany(sr => sr.RegistrationsCollection)
                //                        .AsNoTracking();


                Console.WriteLine("=========== Initial SELECT ==========");
                st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));

                Console.WriteLine("=========== Performing UPDATE ==========");
                st.Update(new { RegistrationDate = new DateTime(rng.Next(1950,2000), rng.Next(1,13), rng.Next(1,28)) });



                Console.WriteLine("=========== Secondary Enumeration ==========");
                st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));


                Console.WriteLine("=========== Performing DELETE ==========");
                st.Delete();

                Console.WriteLine("=========== Third Enumeration ==========");
                st.ForEach(r => Console.WriteLine($"{r.CourseId} {r.StudentId} {r.RegistrationDate:d}"));
               
                }

            Console.WriteLine("\r\nDone");
            Console.ReadLine();
            }
        }
    }
