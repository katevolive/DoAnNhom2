using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebBanSua.ModelViews
{
    public class PaginationHelper
    {
        public static (List<T> itemsToDisplay, int totalPages, int currentPage) GetPagedItems<T>(IEnumerable<T> items, int pageSize, HttpContext context)
        {
            int pageNumber = 1;
            if (!string.IsNullOrEmpty(context.Request.Query["page"]))
            {
                int.TryParse(context.Request.Query["page"], out pageNumber);
            }

            pageNumber = Math.Max(1, pageNumber);

            var itemsToDisplay = items.Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();

            int totalPages = (int)Math.Ceiling((double)items.Count() / pageSize);

            return (itemsToDisplay, totalPages, pageNumber);
        }
    }
}
