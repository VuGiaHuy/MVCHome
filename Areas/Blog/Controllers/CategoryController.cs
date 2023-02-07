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
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace Area.Blog.Controllers
{
    [Area("Blog")]
    [Route("/admin/blog/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var qr =(from c in _context.Categories
                    select c).Include(c=>c.CategoryChildren).Include(c=>c.ParentCategory);
            List<Category> ListCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            return View(ListCategory);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        
        private async Task CreateSelectItems(List<Category> source, List<Category> des, int level)
        {   
            string prefix = string.Concat(Enumerable.Repeat("-----",level));
            foreach(var item in source)
            {
                Category category;
                if(level == 0)
                {
                    category = new Category{
                        Title = prefix+item.Title,
                        Id = item.Id
                    };                
                }
                else
                {
                    category = new Category{
                        Title = prefix+"|   "+item.Title,
                        Id = item.Id
                    };                  
                }
                des.Add(category);
                if(item.CategoryChildren?.Count > 0)
                {
                    await CreateSelectItems(item.CategoryChildren.ToList(),des,level+1);
                }
            }
        }
        public async Task<IActionResult> Create()
        {
            var qr = (from c in _context.Categories
                        select c).Include(c=>c.CategoryChildren).Include(c=>c.ParentCategory);
            var listCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            listCategory.Insert(0, new Category()
            {
                Title = "Default",
                Id = -1,
            });
            var items = new List<Category>();
            await CreateSelectItems(listCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items,"Id","Title",-1);
            return View();
        }

        [TempData]
        public string StatusMessage {get;set;}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParentCategoryId,Title,Content,Slug")] Category category)
        {
            if (ModelState.IsValid)
            {
                StatusMessage = "vao dc";
                if (category.ParentCategoryId.Value == -1)
                {
                    category.ParentCategoryId = null;
                }
                await _context.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Default",
                Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList(listcategory, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }
        public void RemoveChildren(List<Category> source, int id)
        {
            var qr = (from c in _context.Categories
                    where c.Id == id
                    select c).Include(c=>c.CategoryChildren).FirstOrDefault();
            if(qr==null)return;
            if(source==null)return;
            if(qr.CategoryChildren?.Count > 0)
            {
                foreach(Category item in qr.CategoryChildren)
                {
                    source.Remove(item);
                    RemoveChildren(source,item.Id);
                }
            }
        }
        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.Categories
                        select c).Include(c=>c.CategoryChildren).Include(c=>c.ParentCategory);

            List<Category> ListCategory  = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            ListCategory.Remove(category);
            ListCategory.Insert(0,new Category{
                Id=-1,
                Title = "Default",
            });
            List<Category> items = new List<Category>();
            await CreateSelectItems(ListCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }
        

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Content,Slug")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            bool canUpdate = true;
            if(category.Id == category.ParentCategoryId)
            {
                ModelState.AddModelError(string.Empty,"k the chon thu muc ban dau");
                canUpdate = false;
            }
            if(canUpdate && category.ParentCategoryId!=null)
            {
                var childrenCate = (from c in _context.Categories
                                    select c).Include(c=>c.CategoryChildren).ToList()
                                    .Where(c=>c.ParentCategoryId==category.Id);

                Predicate<List<Category>> checkCateId = null;
                checkCateId = (cates)=>{
                    foreach(var cate in cates)
                    {
                        Console.WriteLine(cate.Title);
                        if(cate.Id==category.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty,"k the chon thu muc con");
                            return true;
                        }
                        if(cate.CategoryChildren!=null)
                        {
                            return checkCateId(cate.CategoryChildren.ToList());
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
                    if(category.ParentCategoryId==-1) 
                        category.ParentCategoryId=null;
                    
                    var dtc = _context.Categories.FirstOrDefault(c=>c.Id==id);
                    _context.Entry(dtc).State = EntityState.Detached;
                  
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            var qr = (from c in _context.Categories
                        select c).Include(c=>c.CategoryChildren).Include(c=>c.ParentCategory);
            List<Category> ListCategory = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            List<Category> items = new List<Category>();
            ListCategory.Insert(0,new Category{
                Id=-1,
                Title = "Default"
            });
            await CreateSelectItems(ListCategory,items,0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = (from c in _context.Categories
                            where c.Id == id
                            select c).Include(c=>c.CategoryChildren).FirstOrDefault();
            if (category != null)
            {
                if(category.CategoryChildren?.Count > 0)
                {
                    foreach(Category children in category.CategoryChildren)
                    {
                        children.ParentCategoryId = category.ParentCategoryId;
                    }
                }
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
