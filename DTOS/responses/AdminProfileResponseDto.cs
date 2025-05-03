using System;

namespace CastFlow.Api.Dtos.Response
{
    
    public class AdminProfileResponseDto
    {
      
        public long AdminId { get; set; }

       
        public string Prenom { get; set; } = string.Empty;

        
        public string Nom { get; set; } = string.Empty;

       
        public string Email { get; set; } = string.Empty;

        
    }
}