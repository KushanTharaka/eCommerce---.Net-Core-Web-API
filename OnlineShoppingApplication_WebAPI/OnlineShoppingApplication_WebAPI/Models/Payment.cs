using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class Payment
    {
        public string PaymentstId { get; set; }
        public string CustomerOrderId { get; set; }
        public double TotalAmt { get; set; }
        public DateTime PaymentDate { get; set; }

        public virtual CustomerOrder CustomerOrder { get; set; }
    }
}
