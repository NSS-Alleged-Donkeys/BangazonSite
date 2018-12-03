using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductViewModels
{
    public class AllProductsViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Product> AllProducts { get; set; }
    }
}
