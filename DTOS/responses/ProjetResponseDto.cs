using System;
using System.Collections.Generic;

namespace CastFlow.Api.Dtos.Response
{
    public class ProjetResponseDto 
    {
        public long ProjetId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string TypeProjet { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string Realisateur { get; set; } = string.Empty;
        public string? Logline { get; set; }
        public string? Synopsis { get; set; }
        public DateTime CreeLe { get; set; }

        // Inclure directement les rôles (en utilisant le DTO de réponse de Rôle)
        public List<RoleResponseDto> Roles { get; set; } = new List<RoleResponseDto>();
    }
}