using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShoppingApplication_WebAPI.Custom_Models;
using OnlineShoppingApplication_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OnlineShoppingApplication_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly NewOnlineShoppingApplicationDBContext _context;

        public ShoppingCartController(NewOnlineShoppingApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/ShoppingCart
        [Authorize(Roles = "Customer")]
        [HttpGet("/notPurchasedItems/{id}")]
        public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetShoppingCartNotPurchased(string id)
        {
            return  _context.ShoppingCarts.Where(sc => sc.CartStatus == "NotPurchased" && sc.CustomerId == id)
                .Where(prod => prod.Product.ProductStatus == "Available" && prod.Product.Quantity > 0)
                .OrderByDescending(sc => sc.DateAdded)
                .Include(prod => prod.Product).ToList();
        }

        [AllowAnonymous]
        [HttpGet("/purchasedItems/{id}")]
        public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetShoppingCartPurchased(string id)
        {
            return _context.ShoppingCarts.Where(sc => sc.CartStatus == "Purchased" && sc.CustomerId == id)
                .OrderByDescending(sc => sc.DatePurchased)
                .Include(prod => prod.Product).ToList();
        }

        // GET: api/ShoppingCart/5
        [AllowAnonymous]
        [HttpGet("/getSingleCartItem/{id}")]
        public async Task<ActionResult<ShoppingCart>> GetCartItemWithId(string id)
        {
            var cart = await _context.ShoppingCarts.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/ShoppingCart/5
        //[Authorize(Roles = "Customer")]
        [AllowAnonymous]
        [HttpPut("/changeCartItemQuantity/{cartId}/{customerId}/{productId}/{qty}")]
        public async Task<IActionResult> PutCartItemQuantity(string cartId, string customerId, string productId, string qty)
        {
            int quantity = int.Parse(qty);

            ShoppingCart shoppingCart = new ShoppingCart();
            shoppingCart.ShoppingCartId = cartId;
            shoppingCart.CustomerId = customerId;
            shoppingCart.ProductId = productId;
            shoppingCart.Quantity = quantity;

            if (cartId != shoppingCart.ShoppingCartId)
            {
                return BadRequest();
            }

            _context.ShoppingCarts.Attach(shoppingCart);
            _context.Entry(shoppingCart).Property(sc => sc.Quantity).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(cartId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //[Authorize(Roles = "Customer")]
        [AllowAnonymous]
        [HttpPut("/changeCartItemStatus/{customerId}")]
        public async Task<IActionResult> PutCartItem(string customerId)
        {
            string dateTime = DateTime.Now.ToString();
            var res = await _context.ShoppingCarts.Where(sc => sc.CartStatus == "NotPurchased" && sc.CustomerId == customerId).ToListAsync();

            //var updateList = _context.ShoppingCarts.Where(c => res.Contains(c.CartStatus = "Purchased")).ToList();
            //updateList.ForEach(sc => sc.CartStatus == "Purchased" && sc.DatePurchased == dateTime);

            foreach (var item in res)
            {
                item.CartStatus = "Purchased";
                item.DatePurchased = dateTime;
            }

            ShoppingCart shoppingCart = new ShoppingCart();
            shoppingCart.CustomerId = customerId;
            //shoppingCart.CartStatus = "Purchased";
            //shoppingCart.DatePurchased = dateTime;

            //if (cartId != shoppingCart.ShoppingCartId)
            //{
            //    return BadRequest();
            //}

            //_context.ShoppingCarts.Attach(shoppingCart);
            //_context.Entry(shoppingCart).Property(sc => sc.CartStatus).IsModified = true;
            //_context.Entry(shoppingCart).Property(sc => sc.DatePurchased).IsModified = true;
            ////_context.Entry(shoppingCart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                //sendInvoice(customerId, res);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(customerId))//should be cart Id
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

        // POST: api/ShoppingCart
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> PostCartItem(CartDetails_Model cartDetails_Model)
        {
            string cartId = GenerateCartID();
            string dateTime = DateTime.Now.ToString();

            ShoppingCart shoppingCart = new ShoppingCart();
            shoppingCart.ShoppingCartId = cartId;
            shoppingCart.CustomerId = cartDetails_Model.CustomerID;
            shoppingCart.ProductId = cartDetails_Model.ProductID;
            shoppingCart.Price = cartDetails_Model.Price;
            shoppingCart.Quantity = cartDetails_Model.Quantity;
            shoppingCart.DateAdded = dateTime;
            shoppingCart.CartStatus = "NotPurchased";
            _context.ShoppingCarts.Add(shoppingCart);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CartItemExists(shoppingCart.ShoppingCartId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCartItemWithId", new { id = shoppingCart.ShoppingCartId }, shoppingCart);
        }

        // DELETE: api/ShoppingCart/5
        [Authorize(Roles = "Customer")]
        [HttpDelete("{cartId}/{customerId}/{productId}")]
        public async Task<IActionResult> DeleteCartItem(string cartId, string customerId, string productId)
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId, customerId, productId);
            if (cart == null)
            {
                return NotFound();
            }

            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateCartID()
        {
            bool availability = true;
            string newCartID = "";
            while (availability)
            {
                Random random = new Random();
                int randomNum = random.Next(1, 999999);
                string cID = "Cart" + randomNum;

                availability = CartItemExists(cID);
                if (!availability)
                {
                    newCartID = cID;
                }

            }
            return newCartID;
        }

        private bool CartItemExists(string id)
        {
            return _context.ShoppingCarts.Any(sc => sc.ShoppingCartId == id);
        }

        [AllowAnonymous]
        [HttpPost("/sendInvoice")]
        public async Task<IActionResult> SendInvoice(InvoiceDetails_Model invoiceDetails_Model)
        {
            try
            {
                string email = invoiceDetails_Model.Email;
                string customerName = invoiceDetails_Model.Name;
                string address = invoiceDetails_Model.Address;
                string zip = invoiceDetails_Model.ZipCode;
                string total = invoiceDetails_Model.Total;

                sendInvoice(email, customerName, address, zip, total);
                return Ok();
            }
            catch (DbUpdateException)
            {
                return NoContent();
            }
        }

        private void sendInvoice(string email, string customerName, string address, string zip, string total)
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
            //var body = "<p>Hello Customer <b>Thanks for shopping!</b></p>";
            var body = "<html>" +
                "<body>" +
                "<h1>Thanks for your order</h1>" +
                "<div class='card'>" +
                "<div style='text-align: left;'> " +
                "<div style='display: inline-block;vertical-align: middle;padding: 1rem 1rem;border: 1px solid black;'>" +
                "<h3 class='bold'>SUMMARY" +
                "</h3>" +
                "<h4>Order #:   65006500" +
                "</h4>" +
                "<h4>Order Date :  " + dateTime +
                "</h4>" +
                "<h4>Order Total: Rs. " + total + ".00" +
                "</h4>" +
                "</div>" +
                "<div style='display: inline-block;vertical-align: middle;padding: 1rem 1rem;border: 1px solid black;'>" +
                "<h3 class='bold'>SHIPPING ADDRESS" +
                "</h3>" +
                "<h4>Customer Name : " + customerName +
                "</h4>" +
                "<h4>Address : " + address +
                "</h4>" +
                "<h4>Zip Code : " + zip +
                "</h4>" +
                "</div>" +
                "</div>" +
                "</div>" +
                "<hr>" +
                "</body>" +
                "</html>"; ;

            var mail = new MailMessage();
            mail.Subject = subject;
            mail.From = from;
            mail.To.Add(to);
            mail.Body = body;
            mail.IsBodyHtml = true;

            client.Send(mail);
            Console.WriteLine("Sent");*/

            var from = "snapbuy.shopping@gmail.com";
            var pass = "jglcadhuqpvggvls";
            var to = email;

            var subject = "Snap Buy";
            var body = "<html>" +
                "<body>" +
                "<h1>Thanks for your order</h1>" +
                "<div class='card'>" +
                "<div style='text-align: left;'> " +
                "<div style='display: inline-block;vertical-align: middle;padding: 1rem 1rem;border: 1px solid black;'>" +
                "<h3 class='bold'>SUMMARY" +
                "</h3>" +
                "<h4>Order #:   65006500" +
                "</h4>" +
                "<h4>Order Date :  " + dateTime +
                "</h4>" +
                "<h4>Order Total: Rs. " + total + ".00" +
                "</h4>" +
                "</div>" +
                "<div style='display: inline-block;vertical-align: middle;padding: 1rem 1rem;border: 1px solid black;'>" +
                "<h3 class='bold'>SHIPPING ADDRESS" +
                "</h3>" +
                "<h4>Customer Name : " + customerName +
                "</h4>" +
                "<h4>Address : " + address +
                "</h4>" +
                "<h4>Zip Code : " + zip +
                "</h4>" +
                "</div>" +
                "</div>" +
                "</div>" +
                "<hr>" +
                "</body>" +
                "</html>"; ;

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
        }
    }
}
