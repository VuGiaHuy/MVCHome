using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Contact;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace Area.Contact.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Contact")]
    [Route("/Contact/[action]")]
    public class ContactController : Controller
    {
        [TempData]
        public string StatusMessage {get;set;}
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Contact
        public async Task<IActionResult> Index()
        {
              return _context.ContactModels != null ? 
                          View(await _context.ContactModels.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.ContactModels'  is null.");
        }

        // GET: Contact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ContactModels == null)
            {
                return NotFound();
            }

            var ContactModels = await _context.ContactModels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ContactModels == null)
            {
                return NotFound();
            }

            return View(ContactModels);
        }

        // GET: Contact/Create
        [AllowAnonymous]
        public IActionResult SendContact()
        {
            return View();
        }

        // POST: Contact/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> SendContact([Bind("Name,Email,Message")] ContactModels ContactModels)
        {
            if (ModelState.IsValid)
            {
                ContactModels.DateSend = DateTime.Now;
                _context.Add(ContactModels);
                await _context.SaveChangesAsync();
                StatusMessage = "Send Success!";
                return LocalRedirect("/");
            }
            return View(ContactModels);
        }

        // GET: Contact/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ContactModels == null)
            {
                return NotFound();
            }

            var ContactModels = await _context.ContactModels.FindAsync(id);
            if (ContactModels == null)
            {
                return NotFound();
            }
            return View(ContactModels);
        }

        // POST: Contact/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,DateSend,Message")] ContactModels ContactModels)
        {
            if (id != ContactModels.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ContactModels);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactModelsExists(ContactModels.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ContactModels);
        }

        // GET: Contact/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ContactModels == null)
            {
                return NotFound();
            }

            var ContactModels = await _context.ContactModels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ContactModels == null)
            {
                return NotFound();
            }

            return View(ContactModels);
        }

        // POST: Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ContactModels == null)
            {
                return Problem("Entity set 'AppDbContext.ContactModels'  is null.");
            }
            var ContactModels = await _context.ContactModels.FindAsync(id);
            if (ContactModels != null)
            {
                _context.ContactModels.Remove(ContactModels);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactModelsExists(int id)
        {
          return (_context.ContactModels?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
