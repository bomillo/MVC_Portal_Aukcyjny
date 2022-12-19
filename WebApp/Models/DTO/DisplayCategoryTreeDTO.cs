namespace WebApp.Models.DTO
{
    public class DisplayCategoryTreeDTO
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; }

		public int ParentCategoryId { get; set; }
		public Category ParentCategory { get; set; }

        public List<DisplayCategoryTreeDTO> childList { get; set; } = new ();
    }
}
