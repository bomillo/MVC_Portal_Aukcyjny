namespace WebApp.Models
{
    public class Pager
    {
        public Pager() { }

        public Pager(int totalItems, int page, string controller, int pageSize = 20)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            int currentPage = page;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if(startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if(endPage > totalPages)
            {
                endPage = totalPages;
                if(endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            Controller = controller;
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;

        }

        public string Controller { get; set; }
        public int TotalItems { get;  set; }
        public int CurrentPage { get;  set; }
        public int PageSize { get;  set; }
        public int TotalPages { get;  set; }
        public int StartPage { get;  set; }
        public int EndPage { get;  set; }

    }
}
