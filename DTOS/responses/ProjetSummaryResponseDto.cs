using System;

namespace CastFlow.Api.Dtos.Response
{
    public class ProjetSummaryResponseDto
    {
        public long ProjetId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string TypeProjet { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string Realisateur { get; set; } = string.Empty;
        public int NombreRoles { get; set; } // Calculé
        public int NombreCandidatures { get; set; } // Calculé
    }
}