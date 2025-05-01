using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Data;
using CastFlow.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public class UserAdminRepository : IUserAdminRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserAdminRepository> _logger; // Ajout Logger

        public UserAdminRepository(ApplicationDbContext context, ILogger<UserAdminRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync(UserAdmin userAdmin)
        {
            if (userAdmin == null)
            {
                throw new ArgumentNullException(nameof(userAdmin));
            }
            // Le mot de passe doit être hashé AVANT d'appeler cette méthode
            _logger.LogInformation("Ajout de UserAdmin avec email {Email}", userAdmin.Email);
            await _context.UserAdmins.AddAsync(userAdmin);
        }

        public async Task<UserAdmin?> GetByIdAsync(long id)
        {
            _logger.LogDebug("Recherche de UserAdmin par ID {AdminId}", id);
            return await _context.UserAdmins.FindAsync(id);
        }

        public async Task<UserAdmin?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            _logger.LogDebug("Recherche de UserAdmin par Email {Email}", email);
            return await _context.UserAdmins
                                 .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await _context.UserAdmins
                                 .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<UserAdmin>> GetAllAsync()
        {
            _logger.LogDebug("Récupération de tous les UserAdmins");
            return await _context.UserAdmins.ToListAsync();
        }

        public void Update(UserAdmin userAdmin)
        {
            if (userAdmin == null)
            {
                throw new ArgumentNullException(nameof(userAdmin));
            }
            _logger.LogInformation("Mise à jour de UserAdmin ID {AdminId}", userAdmin.AdminId);
            _context.UserAdmins.Update(userAdmin);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            _logger.LogWarning("Tentative de suppression de UserAdmin ID {AdminId}", id); // Log en Warning car c'est une action sensible
            var userAdmin = await _context.UserAdmins.FindAsync(id);
            if (userAdmin == null)
            {
                _logger.LogWarning("UserAdmin ID {AdminId} non trouvé pour suppression.", id);
                return false;
            }
            _context.UserAdmins.Remove(userAdmin);
            
            _logger.LogInformation("UserAdmin ID {AdminId} marqué pour suppression.", id);
            return true; 
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}