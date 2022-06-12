using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShoppingApplication_WebAPI.Custom_Models;
using OnlineShoppingApplication_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Customer_Login_RegisterController : ControllerBase
    {
        private readonly NewOnlineShoppingApplicationDBContext _context;

        public Customer_Login_RegisterController(NewOnlineShoppingApplicationDBContext context)
        {
            _context = context;
        }

        //Register New Customer
        [HttpPost("/registerCustomer")]
        public async Task<ActionResult<CustomerDetails_Model>> RegisterCustomer(CustomerDetails_Model customerDetails)
        {
            string AccType = "Customer";
            string UserID = GenerateUserID(AccType);

            UsersRole usersRole = new UsersRole();
            usersRole.Id = UserID;
            usersRole.Role = AccType;
            usersRole.Email = customerDetails.Email;
            usersRole.Password = customerDetails.Password;
            _context.UsersRoles.Add(usersRole);

            try
            {
                await _context.SaveChangesAsync();

                Customer customer = new Customer();
                customer.CustomerId = UserID;
                customer.Title = customerDetails.Title;
                customer.FirstName = customerDetails.FirstName;
                customer.LastName = customerDetails.LastName;
                customer.Dob = customerDetails.Dob;
                customer.Gender = customerDetails.Gender;
                customer.Address = customerDetails.Address;
                customer.ZipCode = customerDetails.ZipCode;
                _context.Customers.Add(customer);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (UsersRoleExists(usersRole.Id))
                    {
                        var usersRoleCreated = await _context.UsersRoles.FindAsync(usersRole.Id);
                        if (usersRoleCreated == null)
                        {
                            return NotFound();
                        }

                        _context.UsersRoles.Remove(usersRoleCreated);
                        await _context.SaveChangesAsync();

                        return NoContent();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            catch (DbUpdateException)
            {
                if (UsersRoleExists(usersRole.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUsersRole", new { email = customerDetails.Email, password = customerDetails.Password }, usersRole); //need to create GetUserRole endpoint for this to work
            //return NoContent();
        }

        //Register New Admin
        [HttpPost("/registerAdmin")]
        public async Task<ActionResult<AdminDetails_Model>> RegisterAdmin(AdminDetails_Model adminDetails)
        {
            string AccType = "Admin";
            string UserID = GenerateUserID(AccType);

            UsersRole usersRole = new UsersRole();
            usersRole.Id = UserID;
            usersRole.Role = AccType;
            usersRole.Email = adminDetails.Email;
            usersRole.Password = adminDetails.Password;
            _context.UsersRoles.Add(usersRole);

            try
            {
                await _context.SaveChangesAsync();

                Admin admin = new Admin();
                admin.AdminId = UserID;
                admin.FirstName = adminDetails.FirstName;
                admin.LastName = adminDetails.LastName;
                _context.Admins.Add(admin);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (UsersRoleExists(usersRole.Id))
                    {
                        var usersRoleCreated = await _context.UsersRoles.FindAsync(usersRole.Id);
                        if (usersRoleCreated == null)
                        {
                            return NotFound();
                        }

                        _context.UsersRoles.Remove(usersRoleCreated);
                        await _context.SaveChangesAsync();

                        return NoContent();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            catch (DbUpdateException)
            {
                if (UsersRoleExists(usersRole.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUsersRole", new { email = adminDetails.Email, password = adminDetails.Password }, usersRole); //need to create GetUserRole endpoint for this to work
            //return NoContent();
        }

        //Login
        [HttpGet("/Login/{email}/{password}")]
        public async Task<ActionResult<UsersRole>> GetUsersRole(string email, string password)
        {
            var usersRole = _context.UsersRoles
                .Where(u => u.Email == email)
                .Where(u => u.Password == password)
                .Include(cus => cus.Customer)
                .Include(adm => adm.Admin)
                // Add this to go insde an included table's connected table [.ThenInclude(customer => customer.ShoppingCarts)]
                .FirstOrDefault<UsersRole>();

            if (usersRole == null)
            {
                return NotFound();
            }
            string uRole = usersRole.Role;
            return usersRole;
        }

        private string GenerateUserID(string AccType)
        {
            bool availability = true;
            string newUserID = "";
            while(availability)
            {
                Random random = new Random();
                int randomNum = random.Next(1, 999999);
                string uID = AccType + randomNum;

                availability = UsersRoleExists(uID);
                if(!availability)
                {
                    newUserID = uID;
                }

            }
            return newUserID;
        }

        private bool UsersRoleExists(string id)
        {
            return _context.UsersRoles.Any(e => e.Id == id);
        }

    }
}
