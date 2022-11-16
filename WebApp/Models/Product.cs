using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public double VatRate { get; set; }
        public bool IsVatExcluded { get; set; }
    }
}
