using System.Data;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace Area.Blog.Controllers
{
    [Area("Product")]
    [Route("/admin/product/pcategory/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class PCategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PCategoryController> _logger;

        public PCategoryController(AppDbContext context, ILogger<PCategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PCategory
        public async Task<IActionResult> Index()
        {
            var qr =(from c in _context.PCategories
                    select c).Include(c=>c.ChildrenCategory).Include(c=>c.ParentCategory);
            List<PCategory> ListPCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            return View(ListPCategory);
        }

        // GET: PCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PCategories == null)
            {
                return NotFound();
            }

            var PCategory = await _context.PCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (PCategory == null)
            {
                return NotFound();
            }

            return View(PCategory);
        }
        
        private async Task CreateSelectItems(List<PCategory> source, List<PCategory> des, int level)
        {   
            string prefix = string.Concat(Enumerable.Repeat("-----",level));
            foreach(var item in source)
            {
                PCategory PCategory;
                if(level == 0)
                {
                    PCategory = new PCategory{
                        Title = prefix+item.Title,
                        Id = item.Id
                    };                
                }
                else
                {
                    PCategory = new PCategory{
                        Title = prefix+"|   "+item.Title,
                        Id = item.Id
                    };                  
                }
                des.Add(PCategory);
                if(item.ChildrenCategory?.Count > 0)
                {
                    await CreateSelectItems(item.ChildrenCategory.ToList(),des,level+1);
                }
            }
        }
        public async Task<IActionResult> Create()
        {
            var qr = (from c in _context.PCategories
                        select c).Include(c=>c.ChildrenCategory).Include(c=>c.ParentCategory);
            var listPCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            listPCategory.Insert(0, new PCategory()
            {
                Title = "Default",
                Id = -1,
            });
            var items = new List<PCategory>();
            await CreateSelectItems(listPCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items,"Id","Title",-1);
            return View();
        }

        [TempData]
        public string StatusMessage {get;set;}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParentCategoryId,Title,Content,Slug")] PCategory PCategory)
        {
            if (ModelState.IsValid)
            {
                if (PCategory.ParentCategoryId.Value == -1)
                {
                    PCategory.ParentCategoryId = null;
                }
                await _context.AddAsync(PCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var listPCategory = await _context.PCategories.ToListAsync();
            listPCategory.Insert(0, new PCategory()
            {
                Title = "Default",
                Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList(listPCategory, "Id", "Title", PCategory.ParentCategoryId);
            return View(PCategory);
        }
        public void RemoveChildren(List<PCategory> source, int id)
        {
            var qr = (from c in _context.PCategories
                    where c.Id == id
                    select c).Include(c=>c.ChildrenCategory).FirstOrDefault();
            if(qr==null)return;
            if(source==null)return;
            if(qr.ChildrenCategory?.Count > 0)
            {
                foreach(PCategory item in qr.ChildrenCategory)
                {
                    source.Remove(item);
                    RemoveChildren(source,item.Id);
                }
            }
        }
        // GET: PCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PCategories == null)
            {
                return NotFound();
            }

            var PCategory = await _context.PCategories.FindAsync(id);
            if (PCategory == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.PCategories
                        select c).Include(c=>c.ChildrenCategory).Include(c=>c.ParentCategory);

            List<PCategory> ListPCategory  = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            ListPCategory.Remove(PCategory);
            ListPCategory.Insert(0,new PCategory{
                Id=-1,
                Title = "Default",
            });
            List<PCategory> items = new List<PCategory>();
            await CreateSelectItems(ListPCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", PCategory.ParentCategoryId);
            return View(PCategory);
        }
        

        // POST: PCategory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParentCategoryId,Title,Content,Slug")] PCategory PCategory)
        {
            if (id != PCategory.Id)
            {
                return NotFound();
            }
            bool canUpdate = true;
            if(PCategory.Id == PCategory.ParentCategoryId)
            {
                ModelState.AddModelError(string.Empty,"k the chon thu muc ban dau");
                canUpdate = false;
            }
            if(canUpdate && PCategory.ParentCategoryId!=null)
            {
                var childrenCate = (from c in _context.PCategories
                                    select c).Include(c=>c.ChildrenCategory).ToList()
                                    .Where(c=>c.ParentCategoryId==PCategory.Id);

                Predicate<List<PCategory>> checkCateId = null;
                checkCateId = (cates)=>{
                    foreach(var cate in cates)
                    {
                        Console.WriteLine(cate.Title);
                        if(cate.Id==PCategory.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty,"k the chon thu muc con");
                            return true;
                        }
                        if(cate.ChildrenCategory!=null)
                        {
                            return checkCateId(cate.ChildrenCategory.ToList());
                        }
                    }
                    return false;
                };
                checkCateId(childrenCate.ToList());
            }
            
            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if(PCategory.ParentCategoryId==-1) 
                        PCategory.ParentCategoryId=null;
                    
                    var dtc = _context.PCategories.FirstOrDefault(c=>c.Id==id);
                    _context.Entry(dtc).State = EntityState.Detached;
                  
                    _context.Update(PCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PCategoryExists(PCategory.Id))
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
            var qr = (from c in _context.PCategories
                        select c).Include(c=>c.ChildrenCategory).Include(c=>c.ParentCategory);
            List<PCategory> ListPCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            List<PCategory> items = new List<PCategory>();
            ListPCategory.Insert(0,new PCategory{
                Id=-1,
                Title = "Default"
            });
            await CreateSelectItems(ListPCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", PCategory.ParentCategoryId);
            return View(PCategory);
        }

        // GET: PCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PCategories == null)
            {
                return NotFound();
            }

            var PCategory = await _context.PCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (PCategory == null)
            {
                return NotFound();
            }

            return View(PCategory);
        }

        // POST: PCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PCategories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var PCategory = (from c in _context.PCategories
                            where c.Id == id
                            select c).Include(c=>c.ChildrenCategory).FirstOrDefault();
            if (PCategory != null)
            {
                if(PCategory.ChildrenCategory?.Count > 0)
                {
                    foreach(PCategory children in PCategory.ChildrenCategory)
                    {
                        children.ParentCategoryId = PCategory.ParentCategoryId;
                    }
                }
                _context.PCategories.Remove(PCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PCategoryExists(int id)
        {
            return (_context.PCategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
