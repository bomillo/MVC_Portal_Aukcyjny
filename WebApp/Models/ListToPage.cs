using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;

namespace WebApp.Models
{
    public class ListToPage<T>
    {
        public List<T> Items { get; set; }
        public int PageItems { get; set; }
        public int CurrentPage { get; set; }

        public int StartPage { get; set; }
        public int EndPage { get; set; }

        public int TotalPages { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public ListToPage(List<T> items, int pageItems, int currentPage, string controller, string action)
        {
            this.Items = items;
            this.PageItems = pageItems;
            this.CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling((decimal)items.Count / pageItems);

            StartPage = currentPage - 5;
            EndPage = currentPage + 4;

            if (StartPage <= 0)
            {
                EndPage = EndPage - (StartPage - 1);
                StartPage = 1;
            }

            if (EndPage > TotalPages)
            {
                EndPage = TotalPages;
                if (EndPage > 10)
                {
                    StartPage = EndPage - 9;
                }
            }
            Controller = controller;
            Action = action;
        }

        public List<T> GetCurrentPages()
        {
            var currentItems = new List<T>();
            int lastIndex = PageItems * CurrentPage;
            int startIndex = lastIndex - PageItems;
            lastIndex = lastIndex < Items.Count ? lastIndex : Items.Count;
            for (int i = startIndex; i < lastIndex; i++)
            {
                currentItems.Add(Items[i]);
            }
            return currentItems;
        }


    }
}
