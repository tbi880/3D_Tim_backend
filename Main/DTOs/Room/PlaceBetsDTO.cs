using System.ComponentModel.DataAnnotations;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.DTOs
{
    public record class PlaceBetsDTO
    (
        [Required][Range(0, 9999)] int RoomId,
        [Required] Dictionary<string, int> BetSidesWithAmount
        );
}
