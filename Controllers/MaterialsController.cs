using MaterialManagement.Data;
using MaterialManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MaterialManagement.Controllers
{
    [Authorize]
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Materials
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var materials = _context.Materials.Include(m => m.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                materials = materials.Where(m => m.Name.Contains(searchString) || m.SKU.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                materials = materials.Where(m => m.CategoryId == categoryId);
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentCategory = categoryId;

            return View(await materials.OrderBy(m => m.Name).ToListAsync());
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,SKU,CategoryId,Quantity,MinimumQuantity,UnitPrice")] Material material)
        {
            if (ModelState.IsValid)
            {
                material.CreatedDate = DateTime.Now;
                material.LastModifiedDate = DateTime.Now;
                _context.Add(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", material.CategoryId);
            return View(material);
        }

        // GET: Materials/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", material.CategoryId);
            return View(material);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,SKU,CategoryId,Quantity,MinimumQuantity,UnitPrice,CreatedDate")] Material material)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    material.LastModifiedDate = DateTime.Now;
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Material updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
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
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", material.CategoryId);
            return View(material);
        }

        // GET: Materials/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
