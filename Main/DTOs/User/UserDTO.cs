using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class UserDTO
    (
        [Required][StringLength(50)] string Name,
        [Required][StringLength(100)] string Email);
}
