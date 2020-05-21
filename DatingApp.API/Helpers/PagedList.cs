using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            //need to calculate it based on the informtion wich we pass in
            //how many items there are divided by the pageSize provided by the ceiling
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        //returning a new instance of a page list 
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, 
            int pageNumber, int pageSize)
        { 
            //uses Count Async agregarot method to count the number of items
            // count will be all of our users
            var count = await source.CountAsync();
            //then we use count
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            //returning a new page page list with those parameters
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}