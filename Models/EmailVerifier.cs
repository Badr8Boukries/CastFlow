// Models/EmailVerifier.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("EmailVerifiers")]
    public class EmailVerifier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty; 

        [Required]
        [StringLength(100)]
        public string VerificationCode { get; set; } = string.Empty; 

        [Required]
        public DateTime ExpiresAt { get; set; } 

       
        [Required]
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserTalent? UserTalent { get; set; } 

        [Required]
        public bool IsVerified { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}