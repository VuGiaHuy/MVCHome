using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Blog.Controllers
{
    [Area("Blog")]
    public class PostViewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostViewController> _logger;

        public PostViewController(AppDbContext context, ILogger<PostViewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("/post/{categoryslug?}")]
        public IActionResult Index(string categoryslug, int page)
        {
            var categories = GetCategory();
            Category category = null;
            if(!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.Categories
                                .Where(c=>c.Slug==categoryslug)
                                .Include(c=>c.ChildrenCategory)
                                .FirstOrDefault();

                if(category == null)
                {
                    return NotFound();
                }
            }
            var post = _context.Posts.Include(p=>p.Author)
                                     .Include(p=>p.PostCategories)
                                     .ThenInclude(pc=>pc.Category)
                                     .AsQueryable();
            if(category != null)
            {
                List<int> ids = new List<int>();
                category.ChildCategoryIDs(ids,category.ChildrenCategory);        
                ids.Add(category.Id);
                post = post.Where(p=>p.PostCategories.Where(pc=>ids.Contains(pc.CategoryId)).Any());
            }
            
            post.OrderByDescending(p=>p.DateUpdate);

            ViewData["Categories"] = categories;
            ViewData["Slug"] = categoryslug;
            ViewData["Category"] = category;

            return View(post.ToList());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var post = _context.Posts.Where(p=>p.Slug==postslug)
                        .Include(p=>p.Author)
                        .Include(p=>p.PostCategories)
                        .ThenInclude(pc=>pc.Category)
                        .FirstOrDefault();
            if(post == null) return NotFound();
            var categories = GetCategory();

            Category category = post.PostCategories.FirstOrDefault()?.Category;
            List<Post> otherPost = _context.Posts.Include(p=>p.PostCategories).Where(p=>p.PostCategories.Where(pc=>pc.CategoryId == category.Id).Any())
                                                 .Where(p=>p.Slug!=postslug).Take(5).ToList();
            ViewData["OtherPost"] = otherPost;
            ViewData["Slug"] = post.Slug;
            ViewData["Categories"] = categories;
            ViewData["Category"] = category;
            return View(post);
        }
        private List<Category> GetCategory()
        {
            var categories = _context.Categories
                        .Include(c => c.ChildrenCategory)
                        .AsEnumerable()
                        .Where(c=>c.ParentCategory==null)
                        .ToList();
            return categories;
        }
    }
}