using System.Net;
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
using Microsoft.AspNetCore.Identity;
using App.Utilities;
namespace Area.Blog.Controllers
{
    [Area("Product")]
    [Route("/admin/product/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator +","+ RoleName.Editor)]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ProductController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }



        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var posts = (from p in _context.Products
                        select p).Include(p=>p.Author).OrderByDescending(p=>p.DateUpdate);

            if(pagesize<=0) pagesize=10;

            int totalPost = await posts.CountAsync();
            int countPage =(int)Math.Ceiling((decimal) totalPost/pagesize);

            if(currentPage < 1) currentPage = 1;
            if(currentPage > countPage) currentPage = countPage;

            var PagingModel = new PagingModel{
                countpages = countPage,
                currentpage = currentPage,
                generateUrl = (pageNumber)=>Url.Action("Index",new { p=pageNumber, pagesize=pagesize})
            };

            ViewBag.PagingModel = PagingModel;
            ViewBag.totalPost = totalPost;
            ViewBag.Stt = (currentPage-1)*pagesize;
            var view = await posts.Skip((currentPage-1)*pagesize)
                                    .Take(pagesize)
                                    .Include(p=>p.ProductPCategories)
                                    .ThenInclude(pc=>pc.Category).ToListAsync();
            return View(view);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var Categories = await _context.Categories.ToListAsync();
            ViewData["Categories"]= new MultiSelectList(Categories,"Id","Title");
            return View();
        }

        [TempData]
        public string StatusMessage {get;set;}

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,PCategoryIds,Price")] CreateProductModel post)
        {
            if(post.Slug==null) post.Slug = AppUtilities.GenerateSlug(post.Title);
            if(await _context.Products.AnyAsync(p=>p.Slug==post.Slug))
            {
                ModelState.AddModelError("Slug","Slug da ton tai");
                return View(post);
            }
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.GetUserAsync(this.User);
               
                post.AuthorId = user.Id;
                post.DateCreate = DateTime.Now;
                post.DateUpdate = DateTime.Now;
                _context.Add(post);
                if(post.PCategoryIds !=null)
                {
                    foreach(var PCategoryId in post.PCategoryIds)
                    {
                        _context.Add(new ProductPCategory{
                            PCategoryId = PCategoryId,
                            Post = post
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage = "Vua tao bai viet moi";
                return RedirectToAction(nameof(Index));
            }
            var Categories = await _context.Categories.ToListAsync();
            ViewData["Categories"]= new MultiSelectList(Categories,"Id","Title");
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await (from p in _context.Products
                        where p.ProductId == id
                        select p).Include(p=>p.ProductPCategories).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound();
            }
            var postEdit = new CreateProductModel{
                ProductId = post.ProductId,
                Title = post.Title,
                Description = post.Description,
                Slug = post.Slug,
                Content = post.Content,
                Published = post.Published,
                PCategoryIds = post.ProductPCategories.Select(pc=>pc.PCategoryId).ToArray(),
                Price = post.Price
            };
            var Categories = await _context.Categories.ToListAsync();
            ViewData["Categories"]= new MultiSelectList(Categories,"Id","Title");            
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,PCategoryIds,Price")] CreateProductModel post)
        {
            if(id!= post.ProductId)
            {
                return NotFound();
            }
            
            if(post.Slug==null) post.Slug = AppUtilities.GenerateSlug(post.Title);
            var postSlug = _context.Products.Where(p=>p.ProductId==id).Select(p=>p.Slug).FirstOrDefault();
            if(post.Slug != postSlug)
            {
                if(await _context.Products.AnyAsync(p=>p.Slug==post.Slug))
                {
                    ModelState.AddModelError("Slug","Slug da ton tai");
                    return View(post);
                }
            }
           
            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Products.Include(p=>p.ProductPCategories).FirstOrDefaultAsync(p=>p.ProductId==id);
                    if(postUpdate == null) return NotFound();

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Slug = post.Slug;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.DateUpdate = DateTime.Now;
                    postUpdate.Price = post.Price;
                    
                    if(post.PCategoryIds==null) post.PCategoryIds = new int[]{};
                    var oldCateIds = postUpdate.ProductPCategories.Select(pc=>pc.PCategoryId).ToArray();
                    var newCateIds = post.PCategoryIds;
                    var removeCateIds = from postCate in postUpdate.ProductPCategories
                                        where (!newCateIds.Contains(postCate.PCategoryId))
                                        select postCate;
                    _context.ProductPCategories.RemoveRange(removeCateIds);
                    var addCateIds = from CateId in newCateIds
                                    where !oldCateIds.Contains(CateId)
                                    select CateId;
                    foreach(var CateId in addCateIds)
                    {
                        _context.ProductPCategories.Add(new ProductPCategory{
                            ProductId = id,
                            PCategoryId = CateId
                        });
                    }
                     var dtc = await (from p in _context.Products
                        where p.ProductId==id
                        select p).FirstOrDefaultAsync();
                    _context.Entry(dtc).State = EntityState.Detached;

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.ProductId))
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
             var Categories = await _context.Categories.ToListAsync();
            ViewData["Categories"]= new MultiSelectList(Categories,"Id","Title");   
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Products.FindAsync(id);
            if (post != null)
            {
                _context.Products.Remove(post);
            }
            StatusMessage=$"Ban vua xoa bai viet: {post.Title}";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
