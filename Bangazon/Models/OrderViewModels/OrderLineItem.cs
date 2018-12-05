using System.Collections.Generic;

namespace Bangazon.Models.OrderViewModels {
    public class OrderLineItem {
        public Product Product { get; set; }
        public int Units { get; set; }
        public double Cost { get; set; }

        public List<OrderProduct> orderProducts { get; set; }
    }
}