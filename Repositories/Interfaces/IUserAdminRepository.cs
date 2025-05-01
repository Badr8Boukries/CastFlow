using CastFlow.Api.Models;
using System.Collections.Generic; 
using System.Threading.Tasks;

namespace CastFlow.Api.Repository 
{
    public interface IUserAdminRepository
    {
        
        Task AddAsync(UserAdmin userAdmin);    
        Task<UserAdmin?> GetByIdAsync(long id);
        Task<UserAdmin?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<UserAdmin>> GetAllAsync(); 
        void Update(UserAdmin userAdmin); 
        Task<bool> DeleteAsync(long id);
        Task<int> SaveChangesAsync();
    }
}