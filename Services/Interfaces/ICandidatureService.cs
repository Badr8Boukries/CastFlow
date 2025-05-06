using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Services.Interfaces
{
    public interface ICandidatureService
    {
       
        Task<MyCandidatureResponseDto?> ApplyToRoleAsync(long talentId, CandidatureCreateRequestDto createDto);

        Task<IEnumerable<MyCandidatureResponseDto>> GetMyApplicationsAsync(long talentId);

        
        Task<bool> WithdrawApplicationAsync(long candidatureId, long talentId);

        

        
        Task<CandidatureDetailResponseDto?> GetApplicationDetailsForAdminAsync(long candidatureId);

       
        Task<IEnumerable<CandidatureSummaryResponseDto>> GetApplicationsForRoleAsync(long roleId);

        Task<CandidatureSummaryResponseDto?> UpdateApplicationStatusAsync(long candidatureId, CandidatureUpdateStatusRequestDto statusDto/*, long adminId*/);
    }
}