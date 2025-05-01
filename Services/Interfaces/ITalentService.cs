using System.Threading.Tasks;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Collections.Generic;

namespace CastFlow.Api.Services.Interfaces
{
    public interface ITalentService
    {
        Task<AuthResponseDto> InitiateTalentRegistrationAsync(RegisterTalentRequestDto registerDto);
        Task<bool> VerifyTalentEmailAsync(VerificationRequestDto verificationDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<TalentProfileResponseDto?> GetTalentProfileByIdAsync(long talentId);
        Task<IEnumerable<TalentProfileResponseDto>> GetAllActiveTalentsAsync();
        // Utilise RegisterTalentRequestDto pour la mise à jour, comme demandé
        Task<TalentProfileResponseDto?> UpdateTalentProfileAsync(long talentId, RegisterTalentRequestDto updateDto);
        Task<bool> DeactivateTalentAccountAsync(long talentId);
    }
}