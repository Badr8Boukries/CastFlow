using System.ComponentModel.DataAnnotations;
namespace CastFlow.Api.Dtos.Request
{
    public class ProjetCreateRequestDto
    {
        [Required][StringLength(150, MinimumLength = 3)] public string Titre { get; set; } = string.Empty;
        [Required][StringLength(50)] public string TypeProjet { get; set; } = string.Empty;
        [Required][StringLength(50)] public string Statut { get; set; } = "CASTING_OUVERT";
        [Required][StringLength(100)] public string Realisateur { get; set; } = string.Empty;
        [StringLength(500)] public string? Logline { get; set; }
        public string? Synopsis { get; set; }
    }
}