using System.Threading.Tasks;
using System.Collections.Generic;
using CastFlow.Api.Dtos.Response; 

namespace CastFlow.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationForTalentAsync(long talentId, string message, string? typeEntiteLiee = null, long? idEntiteLiee = null, string? lienNavigationFront = null);

    
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsForTalentAsync(long talentId, bool seulementNonLues = true, int limit = 20);

        
        Task<bool> MarkNotificationAsReadAsync(long notificationId, long talentId);

       
    }
}