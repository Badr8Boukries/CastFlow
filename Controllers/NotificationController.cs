using Microsoft.AspNetCore.Mvc;
using CastFlow.Api.Services.Interfaces;
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
    [Route("api/notifications")] 
    [ApiController]
    [Authorize] 
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private long? GetCurrentTalentIdStrict()
        {
            var userTypeClaim = User.FindFirstValue("userType");
            if (userTypeClaim != "Talent") 
            {
                _logger.LogWarning("Tentative d'accès aux notifications personnelles par un non-talent. Type: {UserType}", userTypeClaim);
                return null;
            }

            var userIdClaim = User.FindFirstValue("Id"); 
            if (long.TryParse(userIdClaim, out long talentId))
            {
                return talentId;
            }
            _logger.LogWarning("Impossible de parser TalentId depuis le token pour l'utilisateur connecté.");
            return null;
        }

       
        [HttpGet("my")]
        [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool seulementNonLues = true, [FromQuery] int limit = 20)
        {
            var talentId = GetCurrentTalentIdStrict();
            if (talentId == null)
            {
                return Forbid("Accès réservé aux talents connectés.");
            }

            try
            {
                var notifications = await _notificationService.GetNotificationsForTalentAsync(talentId.Value, seulementNonLues, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications pour Talent ID {TalentId}", talentId.Value);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne du serveur lors de la récupération des notifications." });
            }
        }

        
        [HttpPost("{notificationId}/mark-read")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkNotificationAsRead(long notificationId)
        {
            var talentId = GetCurrentTalentIdStrict();
            if (talentId == null)
            {
                return Forbid("Accès réservé aux talents connectés.");
            }

            try
            {
                bool success = await _notificationService.MarkNotificationAsReadAsync(notificationId, talentId.Value);
                if (!success)
                {
                    _logger.LogWarning("Échec du marquage comme lu pour Notification ID {NotificationId} par Talent ID {TalentId}. Notification non trouvée ou non autorisée.", notificationId, talentId.Value);
                    return NotFound(new { message = "Notification non trouvée ou vous n'êtes pas autorisé à la modifier." });
                }
                return NoContent(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de la notification ID {NotificationId} comme lue pour Talent ID {TalentId}", notificationId, talentId.Value);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur interne du serveur lors du marquage de la notification." });
            }
        }

       
    }
}