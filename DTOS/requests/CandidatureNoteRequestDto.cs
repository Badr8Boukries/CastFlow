using System.ComponentModel.DataAnnotations;
namespace CastFlow.Api.Dtos.Request
{
    public class CandidatureNoteRequestDto
    {
        [Required][Range(0.5, 5.0)] public decimal NoteValue { get; set; }
    }
}