namespace WebApp.Models
{
    public class Category 
    {
        public int CategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public string Name { get; set; }
    }
}
