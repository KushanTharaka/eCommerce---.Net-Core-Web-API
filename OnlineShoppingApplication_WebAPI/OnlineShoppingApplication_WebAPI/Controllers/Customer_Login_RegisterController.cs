using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineShoppingApplication_WebAPI.Custom_Models;
using OnlineShoppingApplication_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Customer_Login_RegisterController : ControllerBase
    {
        private IConfiguration _config;

        //public Customer_Login_RegisterController(IConfiguration config)
        //{
        //    _config = config;
        //}

        private readonly NewOnlineShoppingApplicationDBContext _context;

        public Customer_Login_RegisterController(NewOnlineShoppingApplicationDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //Register New Customer
        [AllowAnonymous]
        [HttpPost("/registerCustomer")]
        public async Task<ActionResult<CustomerDetails_Model>> RegisterCustomer(CustomerDetails_Model customerDetails)
        {
            if(UsersEmailExists(customerDetails.Email))
            {
                return Conflict();
            }
            else
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
        }

        //Register New Admin
        [Authorize(Roles = "Admin")]
        [HttpPost("/registerAdmin")]
        public async Task<ActionResult<AdminDetails_Model>> RegisterAdmin(AdminDetails_Model adminDetails)
        {
            if(UsersEmailExists(adminDetails.Email))
            {
                return Conflict();
            }
            else
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
        }

        //Login
        [AllowAnonymous]
        [HttpPost("/Login")]
        public async Task<ActionResult<string>> GetUsersRole(LoginDetails_Model loginDetails_Model)
        {
            var usersRole = _context.UsersRoles
                .Where(u => u.Email == loginDetails_Model.Email && u.Password == loginDetails_Model.Password)
                .Include(cus => cus.Customer)
                .Include(adm => adm.Admin)
                // Add this to go insde an included table's connected table [.ThenInclude(customer => customer.ShoppingCarts)]
                .FirstOrDefault<UsersRole>();

            if (usersRole == null)
            {
                return NotFound();
            }
            else if(usersRole.Role == "Admin")
            {
                var token = GenerateToken("Admin", usersRole);
                //return token;
                return Ok(new { response = token });
            }
            else
            {
                var token = GenerateToken("Customer", usersRole);
                return Ok(new { response = token });
            }
            
        }

        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [HttpGet("/getAllCustomers")]
        public async Task<ActionResult<IEnumerable<UsersRole>>> GetAllCustomers()
        {
            return _context.UsersRoles.Where(ur => ur.Role == "Customer")
                .Include(c => c.Customer).ToList();
        }

        [AllowAnonymous]
        [HttpGet("/getSearchedCustomers/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<UsersRole>>> GetSearchedCustomers(string searchTerm)
        {
            //return _context.Customers.Where(c => ( c.CustomerId.Contains("Customer") ) && c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm))
            //    .Include(ur => ur).ToList(); SEPERATE CODE >>> .CustomerId.Contains(searchTerm) || c.Customer.FirstName.Contains(searchTerm) || c.Customer.LastName.Contains(searchTerm)
            return _context.UsersRoles.Where(ur => ur.Role == "Customer").Where(c => c.Customer.CustomerId.Contains(searchTerm) || c.Customer.FirstName.Contains(searchTerm) || c.Customer.LastName.Contains(searchTerm))
                .Include(c => c.Customer).ToList();
        }

        private string GenerateToken(string role, UsersRole usersRole)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            if (role.Equals("Admin"))
            {
                var claims = new[]
                {
                new Claim("Email", usersRole.Email),
                new Claim("AdminId", usersRole.Admin.AdminId),
                new Claim("FirstName", usersRole.Admin.FirstName),
                new Claim("LastName", usersRole.Admin.LastName),
                new Claim(ClaimTypes.Role, usersRole.Role),
                new Claim("Role", usersRole.Role)
                };

                var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                  _config["Jwt:Audience"],
                  claims,
                  expires: DateTime.Now.AddMinutes(280),
                  signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            else
            {
                var claims = new[]
                {
                new Claim("Email", usersRole.Email),
                new Claim("CustomerId", usersRole.Customer.CustomerId),
                new Claim("Title", usersRole.Customer.Title),
                new Claim("FirstName", usersRole.Customer.FirstName),
                new Claim("LastName", usersRole.Customer.LastName),
                new Claim("Gender", usersRole.Customer.Gender),
                new Claim("Address", usersRole.Customer.Address),
                new Claim("PostalCode", usersRole.Customer.ZipCode),
                new Claim(ClaimTypes.Role, usersRole.Role),
                new Claim("Role", usersRole.Role)
                };

                var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                  _config["Jwt:Audience"],
                  claims,
                  expires: DateTime.Now.AddMinutes(280),
                  signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        [AllowAnonymous]
        [HttpPost("/sendOTP/{otp}")]
        public async Task<IActionResult> SendOTP(string otp, LoginDetails_Model loginDetails_Model)
        {
            try
            {
                var res = sendOTP(otp, loginDetails_Model.Email);
                return Ok(new { response = res });
            }
            catch
            {
                return Conflict();
            }
        }

        private string sendOTP(string otp, string email)
        {
            string dateTime = DateTime.Now.ToString();

            //With Mail Trap Service 
            /*var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("652356b7d2f938", "ff0aafd908067b"),
                EnableSsl = true
            };

            var from = new MailAddress("from@example.com");
            var to = new MailAddress("to@example.com");
            var subject = "Snap Buy";
            var body = "<p>Hello Customer, your OTP code is <b>" + otp + "</b></p>";

            var mail = new MailMessage();
            mail.Subject = subject;
            mail.From = from;
            mail.To.Add(to);
            mail.Body = body;
            mail.IsBodyHtml = true;

            client.Send(mail);
            Console.WriteLine("Sent");
            return "Sent"; */

            var from = "snapbuy.shopping@gmail.com";
            var pass = "jglcadhuqpvggvls";
            var to = email;

            var subject = "Snap Buy";
            var body = "<p>Hello Customer, your OTP code is <b>" + otp + "</b></p>";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(from);
            message.Subject = subject;
            message.To.Add(new MailAddress(to));
            message.Body = body;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(from, pass),
                EnableSsl = true,
            };
            smtpClient.Send(message);
            return "Sent";

        }

        [AllowAnonymous]
        [HttpPost("/pwdChangeAccCheck")]
        public async Task<ActionResult<string>> pwdChangeAccCheck(LoginDetails_Model loginDetails_Model)
        {
            //return _context.UsersRoles.Any(ur => ur.Email == email);
            //return UsersEmailExists(email);
            var user = _context.UsersRoles
                .Where(u => u.Email == loginDetails_Model.Email)
                .FirstOrDefault<UsersRole>();
            if(user == null)
            {
                return NotFound();
                //return Ok(new { response = "NotFound" });
            }
            else
            {
                return Ok(new { response = user.Id });
            }          
        }

        [AllowAnonymous]
        [HttpPut("/updatePassword/{id}/{pwd}")]
        public async Task<IActionResult> updatePassword(string id, string pwd)
        {
            UsersRole usersRole = new UsersRole();
            usersRole.Id = id;
            usersRole.Password = pwd;

            if (id != usersRole.Id)
            {
                return BadRequest();
            }

            _context.UsersRoles.Attach(usersRole);
            _context.Entry(usersRole).Property(ur => ur.Password).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersRoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
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

        private bool UsersEmailExists(string email)
        {
            return _context.UsersRoles.Any(e => e.Email == email);
        }
    }
}
