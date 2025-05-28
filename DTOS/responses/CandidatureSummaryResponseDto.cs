using System;
using CastFlow.Api.Dtos.Response;
namespace CastFlow.Api.Dtos.Response
{
   
    public class CandidatureSummaryResponseDto
    {
       
        public long CandidatureId { get; set; }

        
        public long TalentId { get; set; }

      
        public string TalentNomComplet { get; set; } = string.Empty;

       
        public string? TalentUrlPhoto { get; set; }

        
        public int TalentAge { get; set; }

        
        public DateTime DateCandidature { get; set; }
        public decimal? NoteMoyenne { get; set; }

        public string Statut { get; set; } = string.Empty;

        
    }
}