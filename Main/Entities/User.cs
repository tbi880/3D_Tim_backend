
using System.ComponentModel.DataAnnotations;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(1000)]
        public string? PasswordHash { get; set; }

        public Role Role { get; set; } = Role.Guest;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastVisitAt { get; set; } = DateTime.Now;
    }

}
