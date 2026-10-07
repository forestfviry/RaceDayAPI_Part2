using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class ResultCreateRequest
{
    /// <summary>Format: hh:mm:ss e.g. "00:24:35"</summary>
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Required, Range(1, 100000)]
    public int Position { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class ResultUpdateRequest
{
    /// <summary>Format: hh:mm:ss e.g. "00:24:35"</summary>
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Required, Range(1, 100000)]
    public int Position { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class ResultResponse
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
}
