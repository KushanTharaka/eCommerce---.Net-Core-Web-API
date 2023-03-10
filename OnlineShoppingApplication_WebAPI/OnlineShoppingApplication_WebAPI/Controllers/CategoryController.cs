using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShoppingApplication_WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineShoppingApplication_WebAPI.Custom_Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShoppingApplication_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly NewOnlineShoppingApplicationDBContext _context;

        public CategoryController(NewOnlineShoppingApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.Where(cat => cat.CategoryStatus == "Available").ToListAsync();
        }

        // GET: api/Category/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Category/5
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [HttpPut("/updateCategory/{categoryId}/{name}")]
        public async Task<IActionResult> PutCategory(string categoryId, string name)
        {
            Category category = new Category();
            category.CategoryId = categoryId;
            category.Name = name;
            //category.CategoryStatus = categoryDetails_Model.CategoryStatus;

            if (categoryId != category.CategoryId)
            {
                return BadRequest();
            }

            //_context.Entry(category).State = EntityState.Modified;
            _context.Categories.Attach(category);
            _context.Entry(category).Property(c => c.Name).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(categoryId))
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
        [HttpPut("/deleteCategory/{categoryId}")]
        public async Task<IActionResult> deleteCategory(string categoryId)
        {
            Category category = new Category();
            category.CategoryId = categoryId;
            category.CategoryStatus = "Unavailable";

            if (categoryId != category.CategoryId)
            {
                return BadRequest();
            }

            //_context.Entry(category).State = EntityState.Modified;
            _context.Categories.Attach(category);
            _context.Entry(category).Property(c => c.CategoryStatus).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();

                var res = await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
                foreach (var item in res)
                {
                    item.ProductStatus = "Unavailable";
                }

                //Product product = new Product();
                //product.ProductId = productId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryId))//!CartItemExists(customerId)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(categoryId))
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

        // POST: api/Category
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(CategoryDetails_Model categoryDetails_Model)
        {
            string catId = GenerateCategoryID();

            Category category = new Category();
            category.CategoryId = catId;
            category.Name = categoryDetails_Model.Name;
            category.CategoryStatus = "Available";
            _context.Categories.Add(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CategoryExists(category.CategoryId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }

        // DELETE: api/Category/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateCategoryID()
        {
            bool availability = true;
            string newCategoryID = "";
            while (availability)
            {
                Random random = new Random();
                int randomNum = random.Next(1, 999999);
                string cID = "Category" + randomNum;

                availability = CategoryExists(cID);
                if (!availability)
                {
                    newCategoryID = cID;
                }

            }
            return newCategoryID;
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        private string getUserRole()
        {
            var identity = HttpContext.User.Identities as ClaimsIdentity;
            string role = "";
            if (identity != null)
            {
                var userClaims = identity.Claims;
                role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;
            }
            return role;
        }

    }
}
