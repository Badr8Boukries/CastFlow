using Microsoft.AspNetCore.Mvc;
using CastFlow.Api.Services.Interfaces;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CastFlow.Api.Services;

namespace CastFlow.Api.Controllers
{
    [Route("api/admin/manage")]
    [ApiController]
    [Authorize] 
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementService _adminMgmtService;
        private readonly ILogger<AdminManagementController> _logger;
        private readonly ITalentService _talentService;

        public AdminManagementController(
            IAdminManagementService adminMgmtService,
            ITalentService talentService,
            ILogger<AdminManagementController> logger)
        {
            _adminMgmtService = adminMgmtService ?? throw new ArgumentNullException(nameof(adminMgmtService));
            _talentService = talentService ?? throw new ArgumentNullException(nameof(talentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("invite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InviteAdmin([FromBody] InviteAdminRequestDto inviteDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var invitingAdminIdClaim = User.FindFirstValue("Id");
            var userTypeClaim = User.FindFirstValue("userType");

            if (userTypeClaim != "Admin" || !long.TryParse(invitingAdminIdClaim, out long invitingAdminId))
            {
                _logger.LogWarning("Tentative d'invitation par non-admin ou token invalide.");
                return Forbid();
            }

            try
            {
                bool success = await _adminMgmtService.InviteAdminAsync(inviteDto, invitingAdminId);
                if (!success)
                {
                    return BadRequest(new { message = "Impossible d'envoyer l'invitation. L'email est peut-être déjà utilisé ou une invitation est déjà en attente." });
                }
                return Ok(new { message = "Invitation envoyée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'invitation admin pour {Email} par {InvitingAdminId}", inviteDto.Email, invitingAdminId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }


        [HttpPost("~/api/auth/activate-admin")] 
        [AllowAnonymous] 
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetupAdminAccount([FromBody] SetupAdminAccountRequestDto setupDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var authResponse = await _adminMgmtService.SetupAdminAccountAsync(setupDto);
                return Ok(authResponse);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Échec activation admin: {ErrorMessage}", ex.Message);
                return BadRequest(new AuthResponseDto(false, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Échec activation admin: {ErrorMessage}", ex.Message);
                return BadRequest(new AuthResponseDto(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors activation admin.");
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseDto(false, "Erreur interne serveur."));
            }
        }
        [HttpGet("talents")] 
        //[Authorize(Roles = "Admin")] 
        [ProducesResponseType(typeof(IEnumerable<TalentProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTalents()
        {
            
            var userTypeClaim = User.FindFirstValue("userType");
            if (userTypeClaim != "Admin")
            {
                _logger.LogWarning("Tentative d'accès à GetAllTalents par un non-admin. Type: {UserType}", userTypeClaim);
                return Forbid(); // Seuls les admins peuvent lister tous les talents
            }

            try
            {
                var talents = await _talentService.GetAllActiveTalentsAsync();
                return Ok(talents); // Retourne la liste des DTOs TalentProfileResponseDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la liste des talents.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }

        [HttpGet("talents/{talentId}")] 
       //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TalentProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTalentById(long talentId) 
        {
            
            var userTypeClaim = User.FindFirstValue("userType");
            if (userTypeClaim != "Admin")
            {
                _logger.LogWarning("Tentative d'accès GetTalentById par non-admin. Type: {UserType}", userTypeClaim);
                return Forbid(); 
            }

            try
            {
                var profile = await _talentService.GetTalentProfileByIdAsync(talentId);

                if (profile == null)
                {
                    _logger.LogWarning("Profil Talent non trouvé pour ID {TalentId} (demandé par Admin)", talentId);
                    return NotFound(new { message = $"Aucun profil talent actif trouvé pour l'ID {talentId}." });
                }

                return Ok(profile); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du profil talent ID {TalentId}", talentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }
    }
}