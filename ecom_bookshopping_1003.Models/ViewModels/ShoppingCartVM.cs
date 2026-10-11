using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1003.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public OrderHeader orderHeader { get; set; }
        public IEnumerable<OrderHeader> PreviousAddresses { get; set; }


    }
}
