using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CastFlow.Api.Services.Interfaces; 
namespace CastFlow.Api.Controllers
{
    [Route("api/projets")] 
    [ApiController]
    [Authorize] 
    public class ProjetController : ControllerBase
    {
        private readonly IProjetService _projetService;
        private readonly ILogger<ProjetController> _logger;

        public ProjetController(IProjetService projetService, ILogger<ProjetController> logger)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProjetDetailResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Si non Admin
        public async Task<IActionResult> CreateProjet([FromBody] ProjetCreateRequestDto createDto)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var projet = await _projetService.CreateProjetAsync(createDto);
                if (projet == null) return BadRequest("Impossible de créer le projet.");
                return CreatedAtAction(nameof(GetProjetById), new { projetId = projet.ProjetId }, projet);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur création projet."); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjetSummaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllProjets()
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try { var projets = await _projetService.GetAllProjetsAsync(); return Ok(projets); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération projets."); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpGet("{projetId}")]
        [ProducesResponseType(typeof(ProjetDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjetById(long projetId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try { var projet = await _projetService.GetProjetByIdAsync(projetId); if (projet == null) return NotFound($"Projet ID {projetId} non trouvé."); return Ok(projet); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération projet {ProjetId}.", projetId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpPut("{projetId}")]
        [ProducesResponseType(typeof(ProjetDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProjet(long projetId, [FromBody] ProjetUpdateRequestDto updateDto)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { var updatedProjet = await _projetService.UpdateProjetAsync(projetId, updateDto); if (updatedProjet == null) return NotFound($"Projet ID {projetId} non trouvé."); return Ok(updatedProjet); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur mise à jour projet {ProjetId}.", projetId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpDelete("{projetId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProjet(long projetId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try { var success = await _projetService.DeleteProjetAsync(projetId); if (!success) return NotFound($"Projet ID {projetId} non trouvé."); return NoContent(); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur suppression projet {ProjetId}.", projetId); return StatusCode(500, "Erreur interne serveur."); }
        }

        private bool IsAdmin() { var userTypeClaim = User.FindFirstValue("userType"); return "Admin".Equals(userTypeClaim, StringComparison.OrdinalIgnoreCase); }


        [HttpGet("archived")] 
        [ProducesResponseType(typeof(IEnumerable<ProjetSummaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllArchivedProjets()
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var projets = await _projetService.GetAllArchivedProjetsAsync();
                return Ok(projets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur récupération des projets archivés.");
                return StatusCode(500, "Erreur interne serveur.");
            }
        }
      
    
}
}