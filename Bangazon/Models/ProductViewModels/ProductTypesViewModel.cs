using System.Collections.Generic;
using Bangazon.Models;
using Bangazon.Data;

namespace Bangazon.Models.ProductViewModels
{
    public class ProductTypesViewModel
    { 
       public Product Product { get; set; }
       public ProductType ProductType { get; set; }
       public List<GroupedProducts> GroupedProducts { get; set; }
    }
}