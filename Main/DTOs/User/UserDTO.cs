using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class UserDTO
    (
        [Required] int UserId,
        [Required][StringLength(50)] string Name,
        [Required][StringLength(100)] string Email,
        [Required] long Money,
        [Required] long TotalBets,
        [Required] string Role
                );
}
