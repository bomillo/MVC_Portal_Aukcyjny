namespace WebApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public double VatRate { get; set; }
        public bool IsVatExclueded { get; set; }

    }
}
