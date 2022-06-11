using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineShoppingApplication_WebAPI.Models
{
    public partial class Admin
    {
        public string AdminId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual UsersRole AdminNavigation { get; set; }
    }
}
