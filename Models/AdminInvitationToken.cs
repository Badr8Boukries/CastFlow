using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("AdminInvitationTokens")]
    public class AdminInvitationToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ActivationToken { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [Required]
        public long InvitedByAdminId { get; set; } 

        public long? CreatedAdminId { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("InvitedByAdminId")]
        public virtual UserAdmin? InvitedByAdmin { get; set; }

        [ForeignKey("CreatedAdminId")]
        public virtual UserAdmin? CreatedAdmin { get; set; }
    }
}