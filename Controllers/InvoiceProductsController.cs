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
    public class InvoiceProductsController : ControllerBase
    {
        private readonly Context _context;

        public InvoiceProductsController(Context context)
        {
            _context = context;
        }

        // GET: api/InvoiceProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceProduct>>> GetInvoiceProducts()
        {
          if (_context.InvoiceProducts == null)
          {
              return NotFound();
          }
            return await _context.InvoiceProducts.ToListAsync();
        }

        // GET: api/InvoiceProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceProduct>> GetInvoiceProduct(int id)
        {
          if (_context.InvoiceProducts == null)
          {
              return NotFound();
          }
            var invoiceProduct = await _context.InvoiceProducts.FindAsync(id);

            if (invoiceProduct == null)
            {
                return NotFound();
            }

            return invoiceProduct;
        }

        // PUT: api/InvoiceProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceProduct(int id, InvoiceProduct invoiceProduct)
        {
            if (id != invoiceProduct.InvoiceProductId)
            {
                return BadRequest();
            }

            _context.Entry(invoiceProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceProductExists(id))
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

        // POST: api/InvoiceProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InvoiceProduct>> PostInvoiceProduct(InvoiceProduct invoiceProduct)
        {
          if (_context.InvoiceProducts == null)
          {
              return Problem("Entity set 'Context.InvoiceProducts'  is null.");
          }
            _context.InvoiceProducts.Add(invoiceProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceProduct", new { id = invoiceProduct.InvoiceProductId }, invoiceProduct);
        }

        // DELETE: api/InvoiceProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceProduct(int id)
        {
            if (_context.InvoiceProducts == null)
            {
                return NotFound();
            }
            var invoiceProduct = await _context.InvoiceProducts.FindAsync(id);
            if (invoiceProduct == null)
            {
                return NotFound();
            }

            _context.InvoiceProducts.Remove(invoiceProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceProductExists(int id)
        {
            return (_context.InvoiceProducts?.Any(e => e.InvoiceProductId == id)).GetValueOrDefault();
        }
    }
}
