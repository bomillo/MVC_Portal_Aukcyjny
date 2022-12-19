using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;

namespace WebApp.Services
{
    public class CategoryTreeService
    {
        private readonly PortalAukcyjnyContext context;
        private List<Category> categories;

        public CategoryTreeService(PortalAukcyjnyContext context)
        {
            this.context = context;
            this.categories = context.Categories.ToList();
        }

        public bool hasChildren(int parentCategoryId)
        {
            var childCategory = context.Categories.Where(x => x.ParentCategoryId == parentCategoryId).Any();

            if(childCategory != null)
            {
                return true;
            }

            return false;
        }

		public bool HasParent(int CategoryId)
		{
			var category = context.Categories.Where(x => x.CategoryId == CategoryId).FirstOrDefault();

			if (category.ParentCategoryId != null)
			{
				return true;
			}

			return false;
		}


		public void AddChildrenToTree(DisplayCategoryTreeDTO categoryTreeDTO, Category category)
        {
            DisplayCategoryTreeDTO newCategoryTreeDTO = new DisplayCategoryTreeDTO();
            newCategoryTreeDTO.Category = category;
            newCategoryTreeDTO.CategoryId = category.CategoryId;
            categoryTreeDTO.childList.Add(newCategoryTreeDTO);

            var categoryChilds = context.Categories.Where(x => x.ParentCategoryId == category.CategoryId).ToList();

            newCategoryTreeDTO.ParentCategoryId = categoryTreeDTO.CategoryId;
            newCategoryTreeDTO.ParentCategory = categoryTreeDTO.Category;

            foreach(var child in categoryChilds)
            {
                AddChildrenToTree(newCategoryTreeDTO, child);
            }
        }

        public DisplayCategoryTreeDTO GetCategories()
        {
            var parents = context.Categories
                       .GroupBy(x => x.ParentCategoryId).Select(g => new
                       {
                           CategoryId = g.Key,
                       }).ToList();

            var topParents = context.Categories.Where(x => x.ParentCategoryId == null).ToList();

            DisplayCategoryTreeDTO CategoryTree = new DisplayCategoryTreeDTO();

            CategoryTree.ParentCategory = null;
            CategoryTree.ParentCategoryId = 0;
            foreach (var category in topParents)   // grouped parents by categoryId
            {
				AddChildrenToTree(CategoryTree, category);
            }

            return CategoryTree;
        }
    }
}
