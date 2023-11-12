using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TextTranslator.Models;

namespace TextTranslator.Controllers
{
    [Authorize]
    public class TranslatorsController : Controller
    {
        private readonly TextTranslatorDBContext _context;

        public TranslatorsController(TextTranslatorDBContext context)
        {
            _context = context;
        }

        // GET: Translators
        public async Task<IActionResult> Index()
        {
              return _context.Translators != null ? 
                          View(await _context.Translators.ToListAsync()) :
                          Problem("Entity set 'TextTranslatorDBContext.Translators'  is null.");
        }

        // Stored procedure to get the most used translator
        public JsonResult GetMostUsedTranslator()
        {
            var result = _context.ExecuteGetMostUsedTranslator();
            return Json(result);
        }

        // GET: Translators/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Translators == null)
            {
                return NotFound();
            }

            var translator = await _context.Translators
                .FirstOrDefaultAsync(m => m.Id == id);
            if (translator == null)
            {
                return NotFound();
            }

            return View(translator);
        }

        // GET: Translators/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Translators/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Url,DateCreated,DateModified")] Translator translator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(translator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(translator);
        }

        // GET: Translators/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Translators == null)
            {
                return NotFound();
            }

            var translator = await _context.Translators.FindAsync(id);
            if (translator == null)
            {
                return NotFound();
            }
            return View(translator);
        }

        // POST: Translators/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Url,DateCreated,DateModified")] Translator translator)
        {
            if (id != translator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(translator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TranslatorExists(translator.Id))
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
            return View(translator);
        }

        // GET: Translators/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Translators == null)
            {
                return NotFound();
            }

            var translator = await _context.Translators
                .FirstOrDefaultAsync(m => m.Id == id);
            if (translator == null)
            {
                return NotFound();
            }

            return View(translator);
        }

        // POST: Translators/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Translators == null)
            {
                return Problem("Entity set 'TextTranslatorDBContext.Translators'  is null.");
            }
            var translator = await _context.Translators.FindAsync(id);
            if (translator != null)
            {
                _context.Translators.Remove(translator);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TranslatorExists(int id)
        {
          return (_context.Translators?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// A call to this method will retrieve the associated select list of translators
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetTranslators()
        {

            SelectList translatorSel = new SelectList(await _context.Translators.ToListAsync(), "Id", "Name");

            List<SelectListItem> l = translatorSel.ToList();

            // Add empty Element
            //l.Insert(0, new SelectListItem() { Text = "Select a translator", Value = "", Selected = true });
            translatorSel = new SelectList(l, "Value", "Text");

            return (Json(translatorSel));
        }

        /// <summary>
        /// A call to this method will retrieve the associated list of translators
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetTranslatorList()
        {
            var textTranslatorDBContext = _context.Translators;
            return Json(new { data = await textTranslatorDBContext.Select(t => new { t.Id, t.Name, t.Url, DateCreated = t.DateCreated.Value.ToShortDateString() + " " + t.DateCreated.Value.ToShortTimeString(), DateModified = t.DateModified.Value.ToShortDateString() + " " + t.DateModified.Value.ToShortTimeString() }).ToListAsync() });
        }
    }
}
