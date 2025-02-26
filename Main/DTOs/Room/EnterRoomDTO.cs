using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class EnterRoomDTO
    (
        [Required][Range(0, 9999)] int RoomId
    );
}
