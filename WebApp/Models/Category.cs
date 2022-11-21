using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Category 
    {
        [Key]
        public int CategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }
    }
}
