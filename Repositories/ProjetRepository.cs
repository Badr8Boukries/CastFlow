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
    public class ProjetRepository : IProjetRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjetRepository> _logger;

        public ProjetRepository(ApplicationDbContext context, ILogger<ProjetRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Projet projet)
        {
            if (projet == null) throw new ArgumentNullException(nameof(projet));
            projet.IsDeleted = false;
            projet.CreeLe = DateTime.UtcNow;
            projet.ModifieLe = DateTime.UtcNow;
            await _context.Projets.AddAsync(projet);
        }

        public async Task<Projet?> GetByIdAsync(long id)
        {
            // If you want to include roles and talent even for "all" (including deleted), you'd add Includes here too.
            // For now, keeping it simple as it was.
            return await _context.Projets.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjetId == id);
        }

        public async Task<Projet?> GetActiveByIdWithRolesAsync(long id)
        {
            return await _context.Projets
                                 // Include only active roles related to the project
                                 .Include(p => p.Roles.Where(r => !r.IsDeleted))
                                     // Then, for each of those active roles, include its TalentAssigne
                                     .ThenInclude(r => r.TalentAssigne)
                                 .FirstOrDefaultAsync(p => p.ProjetId == id && !p.IsDeleted); // Ensure project itself is active
        }

        public async Task<IEnumerable<Projet>> GetAllActiveAsync()
        {
            // If GetAllActiveAsync is used by GetAllProjetsAsync in the service
            // and that service then manually fetches roles and talent, this is fine.
            // If AutoMapper directly maps from this to a DTO that needs nested talent info,
            // you'd need more includes here. For now, assuming it's for simpler summaries.
            return await _context.Projets
                                 .Where(p => !p.IsDeleted) // Ensure we only get active projects
                                 .OrderByDescending(p => p.CreeLe)
                                 .ToListAsync();
        }

        public void Update(Projet projet)
        {
            if (projet == null) throw new ArgumentNullException(nameof(projet));
            projet.ModifieLe = DateTime.UtcNow;
            _context.Projets.Update(projet);
        }

        public void MarkAsDeleted(Projet projet)
        {
            if (projet == null) throw new ArgumentNullException(nameof(projet));
            projet.IsDeleted = true;
            projet.Statut = "ARCHIVE";
            projet.ModifieLe = DateTime.UtcNow;
            _context.Projets.Update(projet);
        }

        public async Task<bool> ActiveExistsAsync(long id)
        {
            return await _context.Projets.AnyAsync(p => p.ProjetId == id && !p.IsDeleted); // Added !p.IsDeleted for clarity
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}