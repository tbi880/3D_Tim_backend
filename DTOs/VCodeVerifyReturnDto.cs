using System.ComponentModel.DataAnnotations;

namespace _3D_Tim_backend.DTOs
{
    public record class VCodeVerifyReturnDto(
        string status,
        string Name
    );
}

