// Services/Interfaces/ITalentService.cs
using System.Threading.Tasks;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response; // Toujours besoin d'AuthResponseDto

namespace CastFlow.Api.Services.Interfaces
{
    public interface ITalentService
    {
       
        Task<AuthResponseDto> InitiateTalentRegistrationAsync(RegisterTalentRequestDto registerDto);

      
        Task<bool> VerifyTalentEmailAsync(VerificationRequestDto verificationDto);

        
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);

       
    }
}