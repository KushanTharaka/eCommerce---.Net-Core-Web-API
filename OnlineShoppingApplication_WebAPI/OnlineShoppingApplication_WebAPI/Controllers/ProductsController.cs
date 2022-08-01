using Microsoft.AspNetCore.Authorization;
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
    public class ProductsController : ControllerBase
    {
        private readonly NewOnlineShoppingApplicationDBContext _context;

        public ProductsController(NewOnlineShoppingApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.Where(prod => prod.ProductStatus == "Available" && prod.Quantity > 0).ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet ("/getProductsAdmin")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsAdmin()
        {
            return _context.Products.Where(prod => prod.ProductStatus == "Available" && prod.Quantity > 0)
                .Include(cat => cat.Category).ToList();
        }

        // GET: api/Products/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [AllowAnonymous]
        [HttpGet("/getCategoryProducts/{id}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetCategoryProducts(string id)
        {
            return await _context.Products.Where(prod => prod.CategoryId.Equals(id)).ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet("/getAdminCategoryProducts/{id}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAdminCategoryProducts(string id)
        {
            return _context.Products.Where(prod => prod.CategoryId.Equals(id))
                .Include(cat => cat.Category).ToList();
        }

        // PUT: api/Products/5
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, ProductDetails_Model productDetails_Model)
        {
            Product product = new Product();
            product.ProductId = id;
            product.Name = productDetails_Model.Name;
            product.CategoryId = productDetails_Model.CategoryId;
            product.Price = productDetails_Model.Price;
            product.Details = productDetails_Model.Details;
            product.Images = productDetails_Model.Images;
            product.Quantity = productDetails_Model.Quantity;
            product.ProductStatus = productDetails_Model.ProductStatus;

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        [AllowAnonymous]
        [HttpPut("/deleteProduct/{productId}")]
        public async Task<IActionResult> deleteProduct(string productId)
        {
            Product product = new Product();
            product.ProductId = productId;
            product.ProductStatus = "Unavailable";

            if (productId != product.ProductId)
            {
                return BadRequest();
            }

            //_context.Entry(category).State = EntityState.Modified;
            _context.Products.Attach(product);
            _context.Entry(product).Property(p => p.ProductStatus).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productId))
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

        // POST: api/Products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDetails_Model productDetails_Model)
        {
            string prodId = GenerateProductID();

            Product product = new Product();
            product.ProductId = prodId;
            product.Name = productDetails_Model.Name;
            product.CategoryId = productDetails_Model.CategoryId;
            product.Price = productDetails_Model.Price;
            product.Details = productDetails_Model.Details;
            product.Images = productDetails_Model.Images;
            product.Quantity = productDetails_Model.Quantity;
            product.ProductStatus = productDetails_Model.ProductStatus;
            _context.Products.Add(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.ProductId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateProductID()
        {
            bool availability = true;
            string newProductID = "";
            while (availability)
            {
                Random random = new Random();
                int randomNum = random.Next(1, 999999);
                string pID = "Product" + randomNum;

                availability = ProductExists(pID);
                if (!availability)
                {
                    newProductID = pID;
                }

            }
            return newProductID;
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
