using System.ComponentModel.DataAnnotations;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.DTOs
{
    public record class CreateRoomDTO
    (
        [Required][StringLength(50)] string RoomName,
        [Required][Range(2, 10)] int MaxUsers,
        [Required][StringLength(5)] string LevelOfBets,
        [Required] GameType GameType);
}
