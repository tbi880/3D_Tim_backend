
using System.ComponentModel.DataAnnotations;


namespace _3D_Tim_backend.Entities
{
    public class EmailContact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public bool AllowSaveEmail { get; set; } = true;

        [Required]
        public string VCode { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? VerifiedAt { get; set; }
    }

}
