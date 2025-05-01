using CastFlow.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public interface IUserTalentRepository
    {
        Task AddAsync(UserTalent userTalent);
        Task<UserTalent?> GetByIdAsync(long id); 
        Task<UserTalent?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email); 

      
        Task<UserTalent?> GetActiveByIdAsync(long id);

      
        Task<IEnumerable<UserTalent>> GetAllActiveAsync();


        void Update(UserTalent userTalent);
        Task AddEmailVerificationAsync(EmailVerifier emailVerifier);
        Task<EmailVerifier?> GetValidEmailVerificationAsync(string email, string code);
        void MarkEmailVerificationAsUsed(EmailVerifier emailVerifier);

      
        void MarkAsDeleted(UserTalent userTalent); 


        Task<int> SaveChangesAsync();
    }
}