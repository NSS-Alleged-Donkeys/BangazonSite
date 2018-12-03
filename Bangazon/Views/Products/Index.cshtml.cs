using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;

namespace Bangazon.Views.Products
{
    public class ProductIndexModel : PageModel
    {
        private readonly Bangazon.Data.ApplicationDbContext _context;

        public ProductIndexModel(Bangazon.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; }

        public async Task OnGetAsync()
        {
            Product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User).ToListAsync();
        }
    }
}
