using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CastFlow.Api.Models;
namespace CastFlow.Api.Models
{
    [Table("UserAdmins")]
    public class UserAdmin
    {
        [Key]
        public long AdminId { get; set; }

        [Required]
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MotDePasseHash { get; set; } = string.Empty;

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;

        
    }
}