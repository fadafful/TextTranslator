using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TextTranslator.Models;
using TextTranslator.TextTranslatorLib.Helpers;

namespace TextTranslator.Controllers
{
    [Authorize]
    public class TranslationsController : Controller
    {
        private readonly TextTranslatorDBContext _context;
        private readonly HttpClient _httpClient;


        public TranslationsController(TextTranslatorDBContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // GET: Translations
        public async Task<IActionResult> Index()
        {
            var textTranslatorDBContext = _context.Translations.Include(t => t.Translator);
            return View(await textTranslatorDBContext.ToListAsync());
        }

        // GET: Translations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Translations == null)
            {
                return NotFound();
            }

            var translation = await _context.Translations
                .Include(t => t.Translator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (translation == null)
            {
                return NotFound();
            }

            return View(translation);
        }

        // GET: Translations/Create
        public IActionResult Create()
        {
            ViewData["TranslatorId"] = new SelectList(_context.Translators, "Id", "Id");
            return View();
        }

        // POST: Translations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Text,Result,TranslatorId,DateCreated")] Translation translation)
        {
            if (ModelState.IsValid)
            {
                translation.Id = Guid.NewGuid();
                _context.Add(translation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TranslatorId"] = new SelectList(_context.Translators, "Id", "Id", translation.TranslatorId);
            return View(translation);
        }

        // GET: Translations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Translations == null)
            {
                return NotFound();
            }

            var translation = await _context.Translations.FindAsync(id);
            if (translation == null)
            {
                return NotFound();
            }
            ViewData["TranslatorId"] = new SelectList(_context.Translators, "Id", "Id", translation.TranslatorId);
            return View(translation);
        }

        // POST: Translations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Text,Result,TranslatorId,DateCreated")] Translation translation)
        {
            if (id != translation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(translation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TranslationExists(translation.Id))
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
            ViewData["TranslatorId"] = new SelectList(_context.Translators, "Id", "Id", translation.TranslatorId);
            return View(translation);
        }

        // GET: Translations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Translations == null)
            {
                return NotFound();
            }

            var translation = await _context.Translations
                .Include(t => t.Translator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (translation == null)
            {
                return NotFound();
            }

            return View(translation);
        }

        // POST: Translations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Translations == null)
            {
                return Problem("Entity set 'TextTranslatorDBContext.Translations'  is null.");
            }
            var translation = await _context.Translations.FindAsync(id);
            if (translation != null)
            {
                _context.Translations.Remove(translation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TranslationExists(Guid id)
        {
            return (_context.Translations?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// A call to this method will retrieve the associated list of translations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetTranslations()
        {
            var textTranslatorDBContext = _context.Translations.Include(t => t.Translator);
            return Json(new { data = await textTranslatorDBContext.Select(t => new { t.Id, t.Text, t.Result, t.Translator.Name, DateCreated = t.DateCreated.Value.ToShortDateString() + " " + t.DateCreated.Value.ToShortTimeString() }).ToListAsync() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Translate([Bind("Id,Text,Result,TranslatorId,DateCreated")] Translation translation)
        {
            JsonResult json = new JsonResult("");
            JsonData jsonData = new JsonData();

            TranslationsHelper translationsHelper = new TranslationsHelper(_context);
            string url = translationsHelper.TranslatorUrlMapperById(translation.TranslatorId);

            string apiUrl = $"{url}?text={translation.Text}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            // Check if the request was successful (status code 200-299)
            if (response.IsSuccessStatusCode)
            {

                string apiResponse = await response.Content.ReadAsStringAsync();
   
                JsonDocument jsonDocument = JsonDocument.Parse(apiResponse);
                JsonElement root = jsonDocument.RootElement;
                string translatedText = root.GetProperty("contents").GetProperty("translated").GetString();

                // Record the call
                translation.Id = Guid.NewGuid();
                translation.Result = translatedText;
                translation.DateCreated = DateTime.UtcNow;
                _context.Add(translation);
                var dbResponse = await _context.SaveChangesAsync();
                bool result = dbResponse > 0;
                if (result)
                {
                    jsonData = new JsonData
                    {
                        Success = true,
                        Message = "Translation successful",
                        Data = translatedText,

                    };
                    json = new JsonResult(jsonData);
                } 
                
                else
                {
                    jsonData = new JsonData
                    {
                        Success = false,
                        Message = "Translation failed to save",
                        Data = { }

                    };
                    json = new JsonResult(jsonData);
                }

            }
            else
            {
                jsonData = new JsonData
                {
                    Success = false,
                    Message = "Translation failed",
                    Data = StatusCode((int)response.StatusCode, response.ReasonPhrase)

                };
                json = new JsonResult(jsonData);
            }

            return json;
        }
    }
}
