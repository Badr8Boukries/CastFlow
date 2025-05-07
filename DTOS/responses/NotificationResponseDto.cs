using System;

namespace CastFlow.Api.Dtos.Response
{
    public class NotificationResponseDto
    {
        public long NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool EstLu { get; set; }
        public DateTime CreeLe { get; set; }
        public string? TypeEntiteLiee { get; set; }
        public long? IdEntiteLiee { get; set; }
        public string? LienNavigationFront { get; set; }
    }
}