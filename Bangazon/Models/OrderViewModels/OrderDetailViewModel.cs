using System.Collections.Generic;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; }

        public List<OrderLineItem> LineItems { get; set; }

        public List<OrderProduct> orderProducts { get; set; }
    }
}