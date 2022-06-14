using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Custom_Models
{
    public class JwtCustomerData_Model
    {
        public string Email { get; set; }
        public string Id { get; set; }      
        //public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Role { get; set; }
    }
}
