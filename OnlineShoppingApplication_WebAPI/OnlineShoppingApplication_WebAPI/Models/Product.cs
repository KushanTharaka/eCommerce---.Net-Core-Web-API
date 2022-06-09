using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class Product
    {
        public Product()
        {
            ImagesNavigation = new HashSet<Image>();
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        public string ProductId { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }
        public string Images { get; set; }
        public int Quantity { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<Image> ImagesNavigation { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
