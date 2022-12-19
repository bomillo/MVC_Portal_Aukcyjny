namespace WebApp.Models.DTO
{
    public class DisplayCategoryTreeDTO
    {
        public int CategoryId { get; set; }
        public int ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }

        public bool hasChildren { get; set; } = false;
        public List<Category> childList { get; set; }
    }
}
