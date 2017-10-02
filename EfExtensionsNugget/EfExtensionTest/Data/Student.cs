using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ralevex.EF.Data
{
    [Table("[dbo].[Students]")]
    public class Student
        {
        [Column("StudentId", Order = 1), Key]
        public Guid StudentId { get; set; }

        [Column("FirstName"), Required, MaxLength(32)]
        public string FirstName { get; set; }

        [Column("LastName"), Required, MaxLength(64)]
        public string LastName { get; set; }

        [InverseProperty(nameof(Registration.StudentObject))]
        public virtual ICollection<Registration> RegistrationsCollection { get; set; }

 //       public virtual ICollection<Course> CoursesCollection { get; set; }

        }

    }
