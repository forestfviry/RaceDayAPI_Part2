using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class EventCreateRequest
{
    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class EventUpdateRequest
{
    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class EventResponse
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
}
