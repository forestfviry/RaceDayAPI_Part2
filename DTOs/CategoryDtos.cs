using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class CategoryCreateRequest
{
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [Required, Range(0.01, 999.99)]
    public decimal DistanceKm { get; set; }

    [Required, Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [Required, Range(0, 999999.99)]
    public decimal EntryFee { get; set; }
}

public class CategoryUpdateRequest
{
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [Required, Range(0.01, 999.99)]
    public decimal DistanceKm { get; set; }

    [Required, Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [Required, Range(0, 999999.99)]
    public decimal EntryFee { get; set; }
}

public class CategoryResponse
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
}
