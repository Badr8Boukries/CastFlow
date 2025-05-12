using System.Threading.Tasks;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using Microsoft.AspNetCore.Identity;
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
        Task<IdentityResult> ChangePasswordAsync(long talentId, string currentPassword, string newPassword);
        // Utilise RegisterTalentRequestDto pour la mise à jour, comme demandé
        Task<TalentProfileResponseDto?> UpdateTalentProfileAsync(long talentId, TalentProfileUpdateRequestDto updateDto);
        Task<bool> DeactivateTalentAccountAsync(long talentId);

        Task<IEnumerable<RoleDetailResponseDto>> GetPublishedRolesAsync();
    }
}