
using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class TempUserRegisterReturnDTO
    (
        string Name,
        string Email,
        string JwtToken
    );
}
