using System;
namespace CastFlow.Api.Dtos.Response
{
    public class AdminNoteResponseDto
    {
        public long NoteId { get; set; }
        public long AdminId { get; set; }
        public string AdminNomComplet { get; set; } = string.Empty;
        public decimal NoteValue { get; set; }
        public DateTime DateNote { get; set; }
    }
}