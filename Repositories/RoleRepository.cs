using CastFlow.Api.Data;
using CastFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastFlow.Api.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(ApplicationDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            role.IsDeleted = false;
            role.CreeLe = DateTime.UtcNow;
            role.ModifieLe = DateTime.UtcNow;
            await _context.Roles.AddAsync(role);
        }

        public async Task<Role?> GetByIdAsync(long id)
        {
            return await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.RoleId == id);
        }

        public async Task<Role?> GetActiveByIdAsync(long id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
        }

        public async Task<Role?> GetActiveByIdWithProjectAsync(long id)
        {
            return await _context.Roles
                                 .Include(r => r.Projet) 
                                 .FirstOrDefaultAsync(r => r.RoleId == id);
        }


        public async Task<IEnumerable<Role>> GetActiveRolesForProjetAsync(long projetId)
        {
            return await _context.Roles
                                 .Where(r => r.ProjetId == projetId) 
                                 .OrderBy(r => r.CreeLe) 
                                 .ToListAsync();
        }

        public void Update(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            role.ModifieLe = DateTime.UtcNow;
            _context.Roles.Update(role);
        }

        public void MarkAsDeleted(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            role.IsDeleted = true;
            role.EstPublie = false; 
            role.ModifieLe = DateTime.UtcNow;
            _context.Roles.Update(role);


        }

        public async Task<int> CountActiveRolesForProjectAsync(long projetId)
        {
            return await _context.Roles.CountAsync(r => r.ProjetId == projetId);
        }

        public async Task<bool> ActiveExistsAsync(long id)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}