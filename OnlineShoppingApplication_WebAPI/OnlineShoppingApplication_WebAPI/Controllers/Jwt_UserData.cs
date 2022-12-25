using Microsoft.AspNetCore.Http;
using OnlineShoppingApplication_WebAPI.Custom_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Custom_Classes
{
    public class Jwt_UserData
    {
        private HttpContext httpContext;

        public string getUserRole()
        {          
            var identity = httpContext.User.Identities as ClaimsIdentity;
            string role = "";
            if (identity != null)
            {
                var userClaims = identity.Claims;
                role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;
            }
            return role;
        }

        public JwtAdminData_Model getAdminData(string role)
        {
            var identity = httpContext.User.Identities as ClaimsIdentity;
            var userClaims = identity.Claims;

            return new JwtAdminData_Model
            {
                Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                AdminId = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                FirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value,
                LastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
            };           
        }

        public JwtCustomerData_Model getCustomerData(string role)
        {
            var identity = httpContext.User.Identities as ClaimsIdentity;
            var userClaims = identity.Claims;

            return new JwtCustomerData_Model
            {
                Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                Id = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                FirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value,
                LastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                Gender = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Gender)?.Value,
                Address = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.StreetAddress)?.Value,
                ZipCode = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.PostalCode)?.Value,
                Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
            };
        }

    }
}
