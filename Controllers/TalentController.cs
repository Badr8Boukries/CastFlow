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

namespace CastFlow.Api.Controllers
{
    
    
    [Route("api/auth")]
    [ApiController]
    public class TalentController : ControllerBase 
    {
        private readonly ITalentService _talentService;
        private readonly ILogger<TalentController> _logger; 

        // Constructeur mis à jour avec le nom du contrôleur pour le logger
        public TalentController(ITalentService talentService, ILogger<TalentController> logger)
        {
            _talentService = talentService ?? throw new ArgumentNullException(nameof(talentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        [HttpPost("register/talent")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterTalent([FromBody] RegisterTalentRequestDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var response = await _talentService.InitiateTalentRegistrationAsync(registerDto);
                if (!response.IsSuccess && response.Message != null && response.Message.Contains("déjà utilisé"))
                {
                    return Conflict(response); // 409 si email existe
                }
                if (!response.IsSuccess)
                {
                    return BadRequest(response); // Autre erreur gérée par le service
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inscription talent pour {Email}", registerDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseDto(false, "Erreur interne serveur."));
            }
        }

     
        [HttpPost("verify-email/talent")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyTalentEmail([FromBody] VerificationRequestDto verificationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                bool isVerified = await _talentService.VerifyTalentEmailAsync(verificationDto);
                if (!isVerified)
                {
                    return BadRequest(new { message = "Code de vérification invalide ou expiré." });
                }
                return Ok(new { message = "Email vérifié avec succès. Vous pouvez maintenant vous connecter." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur vérification email pour {Email}", verificationDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne serveur." });
            }
        }


      
        [HttpPost("login")]
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

       
    }
}