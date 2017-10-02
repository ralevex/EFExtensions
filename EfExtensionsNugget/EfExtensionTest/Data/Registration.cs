using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ralevex.EF.Data
{
    [Table("[dbo].[Registrations]")]
    public class Registration
    {
        [Column("CourseId", Order = 1), Key]
        public Guid CourseId { get; set; }

        [Column("StudentId", Order = 2), Key]
        public Guid StudentId { get; set; }

        [Column("RegistrationDate"), Required]
        public DateTime RegistrationDate { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course CourseObject { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student StudentObject { get; set; }

    }
}