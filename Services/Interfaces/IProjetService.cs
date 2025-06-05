// Services/Interfaces/IProjetService.cs
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Services.Interfaces 
{
    public interface IProjetService 
    {
        Task<ProjetDetailResponseDto?> CreateProjetAsync(ProjetCreateRequestDto createDto );
        Task<ProjetDetailResponseDto?> GetProjetByIdAsync(long projetId);
        Task<IEnumerable<ProjetSummaryResponseDto>> GetAllProjetsAsync();
        Task<ProjetDetailResponseDto?> UpdateProjetAsync(long projetId, ProjetUpdateRequestDto updateDto);
        Task<bool> DeleteProjetAsync(long projetId);
        Task<IEnumerable<ProjetSummaryResponseDto>> GetAllArchivedProjetsAsync();

    }
}