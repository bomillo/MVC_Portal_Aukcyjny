using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }
        public double VatRate { get; set; }
        public bool IsVatExclueded { get; set; }

    }
}
