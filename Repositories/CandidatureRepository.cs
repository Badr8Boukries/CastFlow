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
    public class CandidatureRepository : ICandidatureRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CandidatureRepository> _logger;

        public CandidatureRepository(ApplicationDbContext context, ILogger<CandidatureRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Candidature candidature)
        {
            if (candidature == null) throw new ArgumentNullException(nameof(candidature));
            candidature.CreeLe = DateTime.UtcNow;
            await _context.Candidatures.AddAsync(candidature);
        }

        public async Task<Candidature?> GetByIdWithDetailsAsync(long id)
        {
            return await _context.Candidatures
                         .Include(c => c.Talent) 
                         .Include(c => c.Role)   
                         .FirstOrDefaultAsync(c => c.CandidatureId == id && c.Talent != null && !c.Talent.IsDeleted);
        }

        public async Task<IEnumerable<Candidature>> GetActiveApplicationsForRoleAsync(long roleId)
        {
            return await _context.Candidatures
                         .Include(c => c.Talent) 
                         .Where(c => c.RoleId == roleId && c.Talent != null && !c.Talent.IsDeleted)
                         .OrderBy(c => c.DateCandidature)
                         .ToListAsync();
        }

        public async Task<IEnumerable<Candidature>> GetActiveApplicationsForTalentAsync(long talentId)
        {
            return await _context.Candidatures
                         .Include(c => c.Role) 
                             .ThenInclude(r => r.Projet) 
                         .Where(c => c.TalentId == talentId && c.Talent != null && !c.Talent.IsDeleted) 
                         .OrderByDescending(c => c.DateCandidature)
                         .ToListAsync();
        }

        public async Task<bool> HasActiveApplicationAsync(long talentId, long roleId)
        {
            
            return await _context.Candidatures
                         .AnyAsync(c => c.TalentId == talentId && c.RoleId == roleId && c.Talent != null && !c.Talent.IsDeleted);
        }
        public async Task<Candidature?> GetByIdForAdminDetailsAsync(long id)
        {
            return await _context.Candidatures
                                 .Include(c => c.Talent) 
                                 .Include(c => c.Role)   
                                     .ThenInclude(r => r.Projet) 
                                 .FirstOrDefaultAsync(c => c.CandidatureId == id && c.Talent != null && !c.Talent.IsDeleted);
        }

        public void Update(Candidature candidature)
        {
            if (candidature == null) throw new ArgumentNullException(nameof(candidature));
            _context.Candidatures.Update(candidature);
        }

        public async Task<Candidature?> GetByIdWithTalentAsync(long id)
        {  
            return await _context.Candidatures
                                 .Include(c => c.Talent)
                                 .FirstOrDefaultAsync(c => c.CandidatureId == id);
         
        }

        public void Delete(Candidature candidature)
        {
            if (candidature == null) throw new ArgumentNullException(nameof(candidature));
            _context.Candidatures.Remove(candidature);
            _logger.LogInformation("Candidature ID {CandidatureId} marquée pour suppression physique.", candidature.CandidatureId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}