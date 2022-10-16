namespace WebApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public double VatRate { get; set; }
        public bool IsVatExclueded { get; set; }

    }
}
