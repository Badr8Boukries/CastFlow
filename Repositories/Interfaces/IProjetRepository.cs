using CastFlow.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public interface IProjetRepository
    {
        Task AddAsync(Projet projet);
        Task<Projet?> GetByIdAsync(long id); 
        Task<Projet?> GetActiveByIdWithRolesAsync(long id);
        Task<IEnumerable<Projet>> GetAllActiveAsync();
        void Update(Projet projet);
        void MarkAsDeleted(Projet projet);
        Task<int> SaveChangesAsync();
        Task<bool> ActiveExistsAsync(long id);
        Task<IEnumerable<Projet>> GetAllArchivedAsync();
    }
}