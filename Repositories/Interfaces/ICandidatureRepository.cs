using CastFlow.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public interface ICandidatureRepository
    {
        Task AddAsync(Candidature candidature);

        Task<Candidature?> GetByIdWithDetailsAsync(long id);

        Task<IEnumerable<Candidature>> GetActiveApplicationsForRoleAsync(long roleId);

        Task<IEnumerable<Candidature>> GetActiveApplicationsForTalentAsync(long talentId);

        Task<bool> HasActiveApplicationAsync(long talentId, long roleId); 
        Task<Candidature?> GetByIdForAdminDetailsAsync(long id);
        void Update(Candidature candidature);
        Task<Candidature?> GetByIdWithTalentAsync(long id);
        void Delete(Candidature candidature);

        Task<int> SaveChangesAsync();
    }
}