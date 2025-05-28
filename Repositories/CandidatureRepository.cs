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
                         .ThenInclude(r => r!.Projet)                                     
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
        public async Task<int> CountActiveByStatusForRoleAsync(long roleId, string statut)
        {
            return await _context.Candidatures
                .CountAsync(c => c.RoleId == roleId &&
                                 c.Statut == statut &&
                                 c.Talent != null && !c.Talent.IsDeleted);
        }

        public async Task<bool> IsRoleAlreadyAssignedToOtherAsync(long roleId, long currentCandidatureIdToExclude)
        {
            return await _context.Candidatures
                .AnyAsync(c => c.RoleId == roleId &&
                               c.CandidatureId != currentCandidatureIdToExclude && // Exclure la candidature actuelle
                               c.Statut == "ASSIGNE" && 
                               c.Talent != null && !c.Talent.IsDeleted);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public async Task<AdminCandidatureNote?> GetAdminNoteForCandidatureAsync(long candidatureId, long adminId)
        {
            _logger.LogDebug("Recherche de la note de l'Admin ID {AdminId} pour Candidature ID {CandidatureId}", adminId, candidatureId);
            return await _context.AdminCandidatureNotes
                .FirstOrDefaultAsync(n => n.CandidatureId == candidatureId && n.AdminId == adminId);
        }

        public async Task AddOrUpdateAdminNoteAsync(AdminCandidatureNote note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            var existingNote = await _context.AdminCandidatureNotes
                .FirstOrDefaultAsync(n => n.CandidatureId == note.CandidatureId && n.AdminId == note.AdminId);

            if (existingNote != null)
            {
                existingNote.NoteValue = note.NoteValue;
                existingNote.DateNote = DateTime.UtcNow;
                _context.AdminCandidatureNotes.Update(existingNote);
                _logger.LogInformation("Note mise à jour par Admin ID {AdminId} pour Candidature ID {CandidatureId}", note.AdminId, note.CandidatureId);
            }
            else
            {
                note.DateNote = DateTime.UtcNow; 
                await _context.AdminCandidatureNotes.AddAsync(note);
                _logger.LogInformation("Nouvelle note ajoutée par Admin ID {AdminId} pour Candidature ID {CandidatureId}", note.AdminId, note.CandidatureId);
            }
        }
        public async Task<List<AdminCandidatureNote>> GetNotesForCandidatureAsync(long candidatureId)
        {
            _logger.LogDebug("Récupération de toutes les notes pour Candidature ID {CandidatureId}", candidatureId);
            return await _context.AdminCandidatureNotes
                                 .Where(n => n.CandidatureId == candidatureId)
                                 .Include(n => n.Admin) 
                                 .OrderByDescending(n => n.DateNote) 
                                 .ToListAsync();
        }

        public async Task<Candidature?> GetByIdWithNotesAndTalentAsync(long candidatureId)
        {
            _logger.LogDebug("Récupération Candidature ID {CandidatureId} avec ses notes et le talent", candidatureId);
            return await _context.Candidatures
                                .Include(c => c.Talent) 
                                .Include(c => c.AdminNotes) 
                                    .ThenInclude(an => an.Admin) 
                                .FirstOrDefaultAsync(c => c.CandidatureId == candidatureId);
        }
        public async Task<Candidature?> GetByIdWithTalentAndRoleAsync(long candidatureId)
        {
            return await _context.Candidatures
                                .Include(c => c.Talent)
                                .Include(c => c.Role)
                                    .ThenInclude(r => r!.Projet)
                                .FirstOrDefaultAsync(c => c.CandidatureId == candidatureId);

        }

    }
}