using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("AdminCandidatureNotes")]
    public class AdminCandidatureNote
    {
        [Key]
        public long NoteId { get; set; }

        [Required]
        public long CandidatureId { get; set; }
        [ForeignKey("CandidatureId")]
        public virtual Candidature Candidature { get; set; } = null!;

        [Required]
        public long AdminId { get; set; } // FK vers UserAdmin
        [ForeignKey("AdminId")]
        public virtual UserAdmin Admin { get; set; } = null!;

        [Required]
        [Range(0.5, 5.0)]
        public decimal NoteValue { get; set; }

        public DateTime DateNote { get; set; } = DateTime.UtcNow;
    }
}