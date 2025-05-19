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

namespace CastFlow.Api.Controllers
{
    [Route("api/candidatures")] 
    [ApiController]
    [Authorize] 
    public class CandidatureController : ControllerBase
    {
        private readonly ICandidatureService _candidatureService;
        private readonly ILogger<CandidatureController> _logger;

        public CandidatureController(ICandidatureService candidatureService, ILogger<CandidatureController> logger)
        {
            _candidatureService = candidatureService ?? throw new ArgumentNullException(nameof(candidatureService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(MyCandidatureResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        public async Task<IActionResult> ApplyToRole([FromForm] CandidatureCreateRequestDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var talentId = GetCurrentUserIdIfTalent();
            if (talentId == null) return Forbid("Seuls les talents peuvent postuler.");

            try
            {
                var candidature = await _candidatureService.ApplyToRoleAsync(talentId.Value, createDto);
                if (candidature == null)
                {
                    return BadRequest(new { message = "Impossible de créer la candidature. Vérifiez que le rôle est valide et que vous n'avez pas déjà postulé." });
                }
              
                return StatusCode(StatusCodes.Status201Created, candidature);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur création candidature par Talent ID {TalentId}", talentId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(IEnumerable<MyCandidatureResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyApplications()
        {
            var talentId = GetCurrentUserIdIfTalent();
            if (talentId == null) return Forbid("Accès réservé aux talents.");

            try { var applications = await _candidatureService.GetMyApplicationsAsync(talentId.Value); return Ok(applications); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération candidatures pour Talent ID {TalentId}", talentId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpDelete("{candidatureId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WithdrawApplication(long candidatureId)
        {
            var talentId = GetCurrentUserIdIfTalent();
            if (talentId == null) return Forbid("Seuls les talents peuvent retirer une candidature.");

            try
            {
                bool success = await _candidatureService.WithdrawApplicationAsync(candidatureId, talentId.Value);
                if (!success) return NotFound("Candidature non trouvée ou vous n'êtes pas autorisé à la retirer.");
                return NoContent();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur retrait candidature ID {CandidatureId} par Talent ID {TalentId}", candidatureId, talentId); return StatusCode(500, "Erreur interne serveur."); }
        }


      

        [HttpGet("{candidatureId}/details")]
        [ProducesResponseType(typeof(CandidatureDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApplicationDetails(long candidatureId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var details = await _candidatureService.GetApplicationDetailsForAdminAsync(candidatureId);
                if (details == null) return NotFound($"Candidature ID {candidatureId} non trouvée.");
                return Ok(details);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération détails candidature {CandidatureId}", candidatureId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpGet("~/api/roles/{roleId}/candidatures")]
                                                       
        [ProducesResponseType(typeof(IEnumerable<CandidatureSummaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetApplicationsForRole(long roleId)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            try
            {
                var applications = await _candidatureService.GetApplicationsForRoleAsync(roleId);
                return Ok(applications);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur récupération candidatures pour Rôle ID {RoleId}", roleId); return StatusCode(500, "Erreur interne serveur."); }
        }

        [HttpPut("{candidatureId}/status")]
        [ProducesResponseType(typeof(CandidatureSummaryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateApplicationStatus(long candidatureId, [FromBody] CandidatureUpdateStatusRequestDto statusDto)
        {
            if (!IsAdmin()) return Forbid("Accès réservé aux administrateurs.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedCandidature = await _candidatureService.UpdateApplicationStatusAsync(candidatureId, statusDto);
                if (updatedCandidature == null) return NotFound($"Candidature ID {candidatureId} non trouvée.");
                return Ok(updatedCandidature); 
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur MàJ statut candidature {CandidatureId}", candidatureId); return StatusCode(500, "Erreur interne serveur."); }
        }

        private long? GetCurrentUserIdIfTalent()
        {
            var userTypeClaim = User.FindFirstValue("userType");
            var userIdClaim = User.FindFirstValue("Id");
            if (userTypeClaim == "Talent" && long.TryParse(userIdClaim, out long talentId))
            {
                return talentId;
            }
            return null;
        }

        private bool IsAdmin()
        {
            var userTypeClaim = User.FindFirstValue("userType");
            return "Admin".Equals(userTypeClaim, StringComparison.OrdinalIgnoreCase);
        }
    }
}