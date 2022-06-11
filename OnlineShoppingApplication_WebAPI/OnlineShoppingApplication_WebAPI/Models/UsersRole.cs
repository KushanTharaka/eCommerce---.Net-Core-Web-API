using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class UsersRole
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual Admin Admin { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
