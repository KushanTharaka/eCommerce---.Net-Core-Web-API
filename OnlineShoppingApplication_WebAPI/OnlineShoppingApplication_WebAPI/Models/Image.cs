using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class Image
    {
        public string ProductId { get; set; }
        public string ImageId { get; set; }
        public string ImageLink { get; set; }

        public virtual Product Product { get; set; }
    }
}
