using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Data;
using CastFlow.Api.Models;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public class UserTalentRepository : IUserTalentRepository
    {
        private readonly ApplicationDbContext _context;

        public UserTalentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(UserTalent userTalent)
        {
            if (userTalent == null) throw new ArgumentNullException(nameof(userTalent));
            userTalent.IsDeleted = false;
            await _context.UserTalents.AddAsync(userTalent);
        }

        public async Task<UserTalent?> GetByIdAsync(long id)
        {
           
            return await _context.UserTalents.FindAsync(id);
        }

        public async Task<UserTalent?> GetActiveByIdAsync(long id)
        {
            return await _context.UserTalents
                                 .FirstOrDefaultAsync(u => u.TalentId == id && !u.IsDeleted);
        }

        public async Task<UserTalent?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _context.UserTalents
                                 .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await _context.UserTalents
                                 .AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<IEnumerable<UserTalent>> GetAllActiveAsync()
        {
            return await _context.UserTalents
                                 .Where(u => !u.IsDeleted)
                                 .ToListAsync();
        }

        public void Update(UserTalent userTalent)
        {
            if (userTalent == null) throw new ArgumentNullException(nameof(userTalent));
            _context.UserTalents.Update(userTalent);
        }

        public void MarkAsDeleted(UserTalent userTalent)
        {
            if (userTalent == null) throw new ArgumentNullException(nameof(userTalent));
            userTalent.IsDeleted = true;
            userTalent.ModifieLe = DateTime.UtcNow;
            _context.UserTalents.Update(userTalent); 
        }


        public async Task AddEmailVerificationAsync(EmailVerifier emailVerifier)
        {
            if (emailVerifier == null) throw new ArgumentNullException(nameof(emailVerifier));
            var oldVerifications = _context.EmailVerifiers.Where(e => e.Email == emailVerifier.Email && !e.IsVerified);
            _context.EmailVerifiers.RemoveRange(oldVerifications);
            await _context.EmailVerifiers.AddAsync(emailVerifier);
        }

        public async Task<EmailVerifier?> GetValidEmailVerificationAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code)) return null;
            var now = DateTime.UtcNow;
            return await _context.EmailVerifiers
                .Where(e => e.Email.ToLower() == email.ToLower() && e.VerificationCode == code && e.ExpiresAt > now && !e.IsVerified)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public void MarkEmailVerificationAsUsed(EmailVerifier emailVerifier)
        {
            if (emailVerifier == null) throw new ArgumentNullException(nameof(emailVerifier));
            emailVerifier.IsVerified = true;
            _context.EmailVerifiers.Update(emailVerifier);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}