using System.ComponentModel.DataAnnotations;
namespace CastFlow.Api.Dtos.Request
{
    public class ProjetUpdateRequestDto
    {
        [StringLength(150, MinimumLength = 3)] public string? Titre { get; set; }
        [StringLength(50)] public string? TypeProjet { get; set; }
        [StringLength(50)] public string? Statut { get; set; }
        [StringLength(100)] public string? Realisateur { get; set; }
        [StringLength(500)] public string? Logline { get; set; }
        public string? Synopsis { get; set; }
    }
}