using CastFlow.Api.Models; // Accès aux modèles
using System.Threading.Tasks;

namespace CastFlow.Api.Repository // Ou CastFlow.Api.Repository.Interfaces
{
    public interface IUserTalentRepository
    {
        
       
        Task AddAsync(UserTalent userTalent);
        Task<UserTalent?> GetByIdAsync(long id); 
        Task<UserTalent?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        void Update(UserTalent userTalent); 
        Task AddEmailVerificationAsync(EmailVerifier emailVerifier);
        Task<EmailVerifier?> GetValidEmailVerificationAsync(string email, string code);
        void MarkEmailVerificationAsUsed(EmailVerifier emailVerifier); 
        Task<int> SaveChangesAsync(); 
    }
}