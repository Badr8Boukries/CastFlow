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

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task CreateNotificationForTalentAsync(long talentId, string message, string? typeEntiteLiee = null, long? idEntiteLiee = null, string? lienNavigationFront = null)
        {
            _logger.LogInformation("🔍 DEBUT CreateNotificationForTalentAsync - TalentId: {TalentId}, Message: {Message}", talentId, message);

            if (talentId <= 0)
            {
                _logger.LogWarning("❌ TalentId invalide: {TalentId}", talentId);
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("❌ Tentative de création de notification avec message vide pour Talent ID {TalentId}", talentId);
                return;
            }

            try
            {
                // Vérifier que le talent existe
                _logger.LogInformation("🔍 Vérification existence du talent {TalentId}", talentId);
                var talentExists = await _context.UserTalents.AnyAsync(t => t.TalentId == talentId && !t.IsDeleted);

                if (!talentExists)
                {
                    _logger.LogWarning("❌ Talent avec ID {TalentId} n'existe pas ou est supprimé", talentId);
                    return;
                }

                _logger.LogInformation("✅ Talent {TalentId} existe", talentId);

                var notification = new Notification
                {
                    DestinataireTalentId = talentId,
                    Message = message.Trim(),
                    EstLu = false,
                    CreeLe = DateTime.UtcNow,
                    TypeEntiteLiee = typeEntiteLiee?.Trim(),
                    IdEntiteLiee = idEntiteLiee,
                    LienNavigationFront = lienNavigationFront?.Trim()
                };

                _logger.LogInformation("🔍 Ajout de la notification en base...");
                await _context.Notifications.AddAsync(notification);

                _logger.LogInformation("🔍 Sauvegarde en cours...");
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("🔍 Résultat SaveChanges: {Result}", result);

                if (result > 0)
                {
                    _logger.LogInformation("✅ Notification créée avec succès - ID: {NotificationId} pour Talent ID: {TalentId}",
                        notification.NotificationId, talentId);
                }
                else
                {
                    _logger.LogWarning("⚠️ SaveChanges a retourné 0 - Aucune notification créée pour Talent ID {TalentId}", talentId);
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ Erreur de base de données lors de la création de la notification pour Talent ID {TalentId}. InnerException: {InnerException}",
                    talentId, dbEx.InnerException?.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur générale lors de la création de la notification pour Talent ID {TalentId}", talentId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsForTalentAsync(long talentId, bool seulementNonLues = true, int limit = 20)
        {
            if (talentId <= 0)
            {
                _logger.LogWarning("TalentId invalide: {TalentId}", talentId);
                return new List<NotificationResponseDto>();
            }

            if (limit <= 0 || limit > 100)
            {
                limit = 20; // Valeur par défaut sécurisée
            }

            _logger.LogInformation("Récupération des notifications pour Talent ID {TalentId}. NonLuesSeulement: {SeulementNonLues}, Limite: {Limite}", talentId, seulementNonLues, limit);

            try
            {
                var query = _context.Notifications
                                .Where(n => n.DestinataireTalentId == talentId);

                if (seulementNonLues)
                {
                    query = query.Where(n => !n.EstLu);
                }

                var notifications = await query
                                        .OrderByDescending(n => n.CreeLe)
                                        .Take(limit)
                                        .AsNoTracking() // Optimisation pour lecture seule
                                        .ToListAsync();

                var result = _mapper.Map<List<NotificationResponseDto>>(notifications);

                _logger.LogInformation("Récupérées {Count} notifications pour Talent ID {TalentId}", result.Count, talentId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications pour Talent ID {TalentId}", talentId);
                return new List<NotificationResponseDto>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(long notificationId, long talentId)
        {
            if (notificationId <= 0 || talentId <= 0)
            {
                _logger.LogWarning("ID invalides - NotificationId: {NotificationId}, TalentId: {TalentId}", notificationId, talentId);
                return false;
            }

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

                // Pas besoin d'appeler Update explicitement, EF track automatiquement les changements
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Notification ID {NotificationId} marquée comme lue pour Talent ID {TalentId}.", notificationId, talentId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Aucune modification sauvegardée pour Notification ID {NotificationId}", notificationId);
                    return false;
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Erreur de base de données lors du marquage de Notification ID {NotificationId} comme lue pour Talent ID {TalentId}", notificationId, talentId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de Notification ID {NotificationId} comme lue pour Talent ID {TalentId}", notificationId, talentId);
                return false;
            }
        }

        // Méthode utilitaire pour compter les notifications non lues
        public async Task<int> GetUnreadNotificationCountAsync(long talentId)
        {
            if (talentId <= 0)
            {
                return 0;
            }

            try
            {
                return await _context.Notifications
                    .Where(n => n.DestinataireTalentId == talentId && !n.EstLu)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du comptage des notifications non lues pour Talent ID {TalentId}", talentId);
                return 0;
            }
        }
    }
}