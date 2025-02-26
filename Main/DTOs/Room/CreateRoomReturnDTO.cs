using System.ComponentModel.DataAnnotations;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.DTOs
{
    public record class CreateRoomReturnDTO
    (
        [Required] string RoomName,
        [Required] int RoomId,
        [Required] GameType GameType);
}
