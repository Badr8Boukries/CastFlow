// Models/Notification.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public long NotificationId { get; set; }

        [Required]
        public long DestinataireTalentId { get; set; } 
        [ForeignKey("DestinataireTalentId")]
        public virtual UserTalent? DestinataireTalent { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public bool EstLu { get; set; } = false;

        [Required]
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? TypeEntiteLiee { get; set; } 

        public long? IdEntiteLiee { get; set; }    

        [StringLength(255)]
        public string? LienNavigationFront { get; set; } 
    }
}