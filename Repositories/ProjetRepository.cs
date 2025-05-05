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
            return await _context.Projets.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjetId == id);
        }

        public async Task<Projet?> GetActiveByIdWithRolesAsync(long id)
        {
            return await _context.Projets
                                 .Include(p => p.Roles.Where(r => !r.IsDeleted)) 
                                 .FirstOrDefaultAsync(p => p.ProjetId == id);
        }

        public async Task<IEnumerable<Projet>> GetAllActiveAsync()
        {
            return await _context.Projets.OrderByDescending(p => p.CreeLe).ToListAsync(); 
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
            return await _context.Projets.AnyAsync(p => p.ProjetId == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}