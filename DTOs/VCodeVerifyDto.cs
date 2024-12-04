using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class VCodeVerifyDto(
        [Required][StringLength(10)] string VerificationCode
    );
}
