using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryStatus { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
