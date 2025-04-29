using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Pour [Table]

namespace CastFlow.Api.Models
{
    public enum UserType { TALENT, PRODUCTION }

    [Table("Users")]
    public class User
    {
        [Key]
        public long UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]

        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserType UserType { get; set; }


        public long? EnterpriseId { get; set; }
        [ForeignKey("EnterpriseId")]
        public virtual Enterprise? Enterprise { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public virtual TalentProfile? TalentProfile { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

        public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    }
}