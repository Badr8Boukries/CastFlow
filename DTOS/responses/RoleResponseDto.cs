using System;
// using System.Collections.Generic; // Pas besoin d'inclure les candidatures ici

namespace CastFlow.Api.Dtos.Response
{
    public class RoleResponseDto // Un seul DTO de réponse pour Role
    {
        public long RoleId { get; set; }
        public long ProjetId { get; set; }
        public string ProjetTitre { get; set; } = string.Empty; // Contexte utile
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }
        public string ExigenceSex { get; set; } = string.Empty;
        public DateTime DateLimiteCandidature { get; set; }
    
    }
}