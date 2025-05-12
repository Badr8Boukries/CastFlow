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
using System.Collections.Generic;

namespace CastFlow.Api.Controllers
{
    [Route("api/talent")]
    [ApiController]
    public class TalentController : ControllerBase
    {
        private readonly ITalentService _talentService;
        private readonly ILogger<TalentController> _logger;

        public TalentController(ITalentService talentService, ILogger<TalentController> logger)
        {
            _talentService = talentService ?? throw new ArgumentNullException(nameof(talentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterTalent([FromForm] RegisterTalentRequestDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var response = await _talentService.InitiateTalentRegistrationAsync(registerDto);
                if (!response.IsSuccess)
                {
                    return response.Message != null && response.Message.Contains("déjà utilisé")
                        ? Conflict(response)
                        : BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inscription talent pour {Email}", registerDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseDto(false, "Erreur interne serveur."));
            }
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyTalentEmail([FromBody] VerificationRequestDto verificationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                bool isVerified = await _talentService.VerifyTalentEmailAsync(verificationDto);
                if (!isVerified) { return BadRequest(new { message = "Code de vérification invalide ou expiré." }); }
                return Ok(new { message = "Email vérifié avec succès. Vous pouvez maintenant vous connecter." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur vérification email pour {Email}", verificationDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }

        [HttpPost("~/api/auth/login")] 
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var authResponse = await _talentService.LoginAsync(loginDto);
                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Échec connexion pour {Email}: {Reason}", loginDto.Email, ex.Message);
                return Unauthorized(new AuthResponseDto(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur connexion pour {Email}", loginDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseDto(false, "Erreur interne serveur."));
            }
        }


        [HttpGet("profile/me")]
        [Authorize]
        [ProducesResponseType(typeof(TalentProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirstValue("Id"); 
            var userTypeClaim = User.FindFirstValue("userType");

            if (userTypeClaim != "Talent" || !long.TryParse(userIdClaim, out long talentId))
            {
                _logger.LogWarning("Tentative d'accès GetMyProfile par non-talent ou ID invalide. Type: {UserType}, IdClaim: {UserIdClaim}", userTypeClaim, userIdClaim);
                return Forbid();
            }

            var profile = await _talentService.GetTalentProfileByIdAsync(talentId);
            if (profile == null)
            {
                _logger.LogWarning("Profil non trouvé pour Talent ID {TalentId} lors de GetMyProfile", talentId);
                return NotFound(new { message = "Profil non trouvé." });
            }

            return Ok(profile);
        }
        [HttpPost("change-password")]
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Add for unauthorized
        [ProducesResponseType(StatusCodes.Status404NotFound)]    // Add for not found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState); // Check ModelState

            var userIdClaim = User.FindFirstValue("Id");

            if (!long.TryParse(userIdClaim, out long userId))
                return BadRequest("Invalid user ID.");

            try
            {
                var result = await _talentService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

                if (result.Succeeded)
                    return Ok("Password changed successfully.");
                else
                    return BadRequest(result.Errors.Select(e => e.Description)); // Return detailed errors
            }
            catch (InvalidOperationException ex) // Catch specific exceptions for better handling
            {
                _logger.LogError(ex, "Invalid operation during password change for user {UserId}", userId);
                return StatusCode(500, "An error occurred while processing the request."); // Consider more specific error messages.
            }

        }



        [HttpPut("profile/me")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(TalentProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        
        public async Task<IActionResult> UpdateMyProfile([FromForm] TalentProfileUpdateRequestDto updateDto) // <<<--- CHANGEMENT ICI
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue("Id"); 
            var userTypeClaim = User.FindFirstValue("userType");

            if (userTypeClaim != "Talent" || !long.TryParse(userIdClaim, out long talentId))
            {
                _logger.LogWarning("Tentative d'accès UpdateMyProfile par non-talent ou ID invalide.");
                return Forbid();
            }

            try
            {
                var updatedProfile = await _talentService.UpdateTalentProfileAsync(talentId, updateDto);
                if (updatedProfile == null)
                {
                    _logger.LogWarning("Profil non trouvé pour mise à jour Talent ID {TalentId}", talentId);
                    return NotFound(new { message = "Profil non trouvé." });
                }
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour profil pour Talent ID {TalentId}", talentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }

        [HttpDelete("profile/me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateMyAccount()
        {
            var userIdClaim = User.FindFirstValue("Id");
                                                      
            var userTypeClaim = User.FindFirstValue("userType");

            if (userTypeClaim != "Talent" || !long.TryParse(userIdClaim, out long talentId))
            {
                _logger.LogWarning("Tentative d'accès DeactivateMyAccount par non-talent ou ID invalide. Type: {UserType}, IdClaim: {UserIdClaim}", userTypeClaim, userIdClaim);
                return Forbid();
            }

            try
            {
                bool success = await _talentService.DeactivateTalentAccountAsync(talentId);
                if (!success)
                {
                    _logger.LogWarning("Compte Talent non trouvé pour désactivation ID {TalentId}", talentId);
                    return NotFound(new { message = "Compte non trouvé." });
                }
                _logger.LogInformation("Compte Talent ID {TalentId} désactivé par l'utilisateur.", talentId);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur désactivation compte pour Talent ID {TalentId}", talentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }

        [HttpGet("roles/published")] 
        [Authorize] 
        [ProducesResponseType(typeof(IEnumerable<RoleDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublishedRoles()
        {
            var userTypeClaim = User.FindFirstValue("userType");
            
            if (userTypeClaim != "Talent")
            {
                _logger.LogWarning("Tentative d'accès GetPublishedRoles par un non-talent. Type: {UserType}", userTypeClaim);
                return Forbid("Accès réservé aux talents.");
            }

            try
            {
                var roles = await _talentService.GetPublishedRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des rôles publiés.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne du serveur." });
            }
        }


    }
}