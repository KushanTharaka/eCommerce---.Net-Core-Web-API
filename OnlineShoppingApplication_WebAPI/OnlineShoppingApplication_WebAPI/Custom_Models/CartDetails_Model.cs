using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Custom_Models
{
    public class CartDetails_Model
    {
        public string CustomerID { get; set; }
        public string ProductID { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
