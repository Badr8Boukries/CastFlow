using System;

namespace CastFlow.Api.Dtos.Response
{
   
    public class TalentProfileResponseDto
    {
       
        public long TalentId { get; set; }

       
        public string Prenom { get; set; } = string.Empty;

       
        public string Nom { get; set; } = string.Empty;

       
        public string Email { get; set; } = string.Empty;

       
        public DateTime DateNaissance { get; set; }

        
        public int Age { get; set; }

        
        public string Sex { get; set; } = string.Empty;

       
        public string? Telephone { get; set; }

      
        public string? UrlPhoto { get; set; }

        
        public string? UrlCv { get; set; }

        
    }
}