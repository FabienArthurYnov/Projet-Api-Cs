using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Projet_Api_Cs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandProductsController : ControllerBase
    {
        private readonly Context _context;

        public CommandProductsController(Context context)
        {
            _context = context;
        }

        // GET: api/CommandProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommandProduct>>> GetCommandProducts()
        {
          if (_context.CommandProducts == null)
          {
              return NotFound();
          }
            return await _context.CommandProducts.ToListAsync();
        }

        // GET: api/CommandProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommandProduct>> GetCommandProduct(int id)
        {
          if (_context.CommandProducts == null)
          {
              return NotFound();
          }
            var commandProduct = await _context.CommandProducts.FindAsync(id);

            if (commandProduct == null)
            {
                return NotFound();
            }

            return commandProduct;
        }

        // PUT: api/CommandProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommandProduct(int id, CommandProduct commandProduct)
        {
            if (id != commandProduct.CommandProductId)
            {
                return BadRequest();
            }

            _context.Entry(commandProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommandProductExists(id))
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

        // POST: api/CommandProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CommandProduct>> PostCommandProduct(CommandProduct commandProduct)
        {
          if (_context.CommandProducts == null)
          {
              return Problem("Entity set 'Context.CommandProducts'  is null.");
          }
            _context.CommandProducts.Add(commandProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCommandProduct", new { id = commandProduct.CommandProductId }, commandProduct);
        }

        // DELETE: api/CommandProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommandProduct(int id)
        {
            if (_context.CommandProducts == null)
            {
                return NotFound();
            }
            var commandProduct = await _context.CommandProducts.FindAsync(id);
            if (commandProduct == null)
            {
                return NotFound();
            }

            _context.CommandProducts.Remove(commandProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommandProductExists(int id)
        {
            return (_context.CommandProducts?.Any(e => e.CommandProductId == id)).GetValueOrDefault();
        }
    }
}
