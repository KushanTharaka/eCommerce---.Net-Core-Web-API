using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class CustomerOrder
    {
        public CustomerOrder()
        {
            Payments = new HashSet<Payment>();
        }

        public string CustomerOrderId { get; set; }
        public string ShoppingCartId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public DateTime DateAdded { get; set; }
        public double Price { get; set; }

        public virtual ShoppingCart ShoppingCart { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
