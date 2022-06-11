using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            CustomerOrders = new HashSet<CustomerOrder>();
        }

        public string ShoppingCartId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
        public string CartStatus { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<CustomerOrder> CustomerOrders { get; set; }
    }
}
