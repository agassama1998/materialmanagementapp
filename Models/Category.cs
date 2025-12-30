using System.ComponentModel.DataAnnotations;

namespace MaterialManagement.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}
