using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Custom_Models
{
    public class ProductDetails_Model
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }
        public string Images { get; set; }
        public int Quantity { get; set; }
        public string ProductStatus { get; set; }
    }
}
