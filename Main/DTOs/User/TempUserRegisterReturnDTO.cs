
using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class TempUserRegisterReturnDTO
    (
        string Status,
        string Name,
        string Email,
        string CookieValue
    );
}
