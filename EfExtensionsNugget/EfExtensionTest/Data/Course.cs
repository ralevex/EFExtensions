using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ralevex.EF.Data
{
    [Table("[dbo].[Courses]")]
    public class Course
        {
        [Column("CourseId", Order = 1), Key]
        public Guid CourseId { get; set; }

        [Column("CourseName"), Required, MaxLength(64)]
        public string CourseName { get; set; }

        [InverseProperty(nameof(Registration.CourseObject))]
        public virtual ICollection<Registration> RegistrationsCollection { get; set; }

      //  public virtual ICollection<Student> StudentCollection { get; set; }
        }
    }