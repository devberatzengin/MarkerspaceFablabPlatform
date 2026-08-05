using System.ComponentModel.DataAnnotations;

namespace MakerspaceFablabPlatform.Dtos.Equipment;

public class ScheduleRequest
{
    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public TimeSpan Span { get; set; }
}
