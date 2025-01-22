using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class UserLoginDTO
    (
        [Required][StringLength(100)] string Email,
        [Required][StringLength(100)] string Password
    );
}
