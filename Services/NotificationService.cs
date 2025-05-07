using AutoMapper; 
using CastFlow.Api.Data;
using CastFlow.Api.Models;
using CastFlow.Api.Services.Interfaces;
using CastFlow.Api.Dtos.Response;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks;

namespace CastFlow.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IMapper mapper) // Ajout IMapper
        {
            _context = context;
            _logger = logger;
            _mapper = mapper; 
        }

        public async Task CreateNotificationForTalentAsync(long talentId, string message, string? typeEntiteLiee = null, long? idEntiteLiee = null, string? lienNavigationFront = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Tentative de création de notification avec message vide pour Talent ID {TalentId}", talentId);
                return;
            }

            var notification = new Notification
            {
                DestinataireTalentId = talentId,
                Message = message,
                EstLu = false,
                CreeLe = DateTime.UtcNow,
                TypeEntiteLiee = typeEntiteLiee,
                IdEntiteLiee = idEntiteLiee,
                LienNavigationFront = lienNavigationFront
            };

            try
            {
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Notification créée ID {NotificationId} pour Talent ID {TalentId}", notification.NotificationId, talentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification pour Talent ID {TalentId}", talentId);
            }
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsForTalentAsync(long talentId, bool seulementNonLues = true, int limit = 20)
        {
            _logger.LogInformation("Récupération des notifications pour Talent ID {TalentId}. NonLuesSeulement: {SeulementNonLues}, Limite: {Limite}", talentId, seulementNonLues, limit);
            try
            {
                var query = _context.Notifications
                                .Where(n => n.DestinataireTalentId == talentId);

                if (seulementNonLues)
                {
                    query = query.Where(n => !n.EstLu);
                }

                var notifications = await query.OrderByDescending(n => n.CreeLe)
                                             .Take(limit)
                                             .ToListAsync();

                return _mapper.Map<List<NotificationResponseDto>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications pour Talent ID {TalentId}", talentId);
                return new List<NotificationResponseDto>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(long notificationId, long talentId)
        {
            _logger.LogInformation("Tentative de marquage Notification ID {NotificationId} comme lue pour Talent ID {TalentId}", notificationId, talentId);
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.DestinataireTalentId == talentId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification ID {NotificationId} non trouvée pour Talent ID {TalentId} ou n'appartient pas à l'utilisateur.", notificationId, talentId);
                    return false;
                }

                if (notification.EstLu)
                {
                    _logger.LogInformation("Notification ID {NotificationId} est déjà marquée comme lue.", notificationId);
                    return true; 
                }

                notification.EstLu = true;
              
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification ID {NotificationId} marquée comme lue pour Talent ID {TalentId}.", notificationId, talentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de Notification ID {NotificationId} comme lue pour Talent ID {TalentId}", notificationId, talentId);
                return false;
            }
        }
    }
}