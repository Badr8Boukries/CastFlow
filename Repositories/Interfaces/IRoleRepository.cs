using CastFlow.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public interface IRoleRepository
    {
        Task AddAsync(Role role);
        Task<Role?> GetByIdAsync(long id); 
        Task<Role?> GetActiveByIdAsync(long id); 
        Task<Role?> GetActiveByIdWithProjectAsync(long id); 
        Task<IEnumerable<Role>> GetActiveRolesForProjetAsync(long projetId); 
        void Update(Role role);
        void MarkAsDeleted(Role role); // Soft Delete
        Task<int> SaveChangesAsync();
        Task<bool> ActiveExistsAsync(long id);
    }
}