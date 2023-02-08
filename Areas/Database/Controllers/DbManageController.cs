using System.Security.AccessControl;
using System.Net;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Bogus;
using App.Models.Blog;

namespace Area.DbMangae.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        [TempData]
        public string StatusMessage { get; set; }
        private readonly AppDbContext _dbContext;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext dbContext, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MigrateAsync()
        {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Migrate success";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult DeleteDB()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDBAsync()
        {
            bool success = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = (success) ? "Delete DB success" : "Can't Delete DB";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SeedDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var role in roleNames)
            {
                var roleName = (string)role.GetRawConstantValue();
                var roleFound = await _roleManager.FindByNameAsync(roleName);
                if (roleFound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            var admin = await _userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new AppUser
                {
                    Email = "admin@gmail.com",
                    UserName = "admin",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, "admin123");
                await _userManager.AddToRoleAsync(admin, RoleName.Administrator);
            }
            await SeedCategory();
            await _signInManager.SignInAsync(admin, true);
            StatusMessage = "Seed data success";
            return RedirectToAction(nameof(Index));
        }
        private async Task SeedCategory()
        {
            _dbContext.RemoveRange(_dbContext.Categories.Where(c=>c.Content.Contains("[FakeData]")));
           // _dbContext.RemoveRange(_dbContext.Posts.Where(c=>c.Content.Contains("[FakeData]")));
            var faker = new Faker<Category>();
            int cm = 1;
            faker.RuleFor(c => c.Title, faker => $"CM{cm++}" + faker.Lorem.Sentence(1, 2).Trim('.'))
                .RuleFor(c => c.Content, faker => faker.Lorem.Sentences(5) + "[FakeData]")
                .RuleFor(c => c.Slug, faker => faker.Lorem.Slug());


            var cate1 = faker.Generate();
            var cate11 = faker.Generate();
            var cate12 = faker.Generate();

            var cate2 = faker.Generate();
            var cate21 = faker.Generate();
            var cate211 = faker.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var cates = new Category[] { cate1, cate11, cate12, cate2, cate21, cate211 };
            _dbContext.Categories.AddRange(cates);
        


            var rCateIndex = new Random();
            int baiviet = 1;
            var user =  _userManager.GetUserAsync(this.User).Result;
            
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p=>p.AuthorId,faker=>user.Id);
            fakerPost.RuleFor(p=>p.Content,faker=>faker.Lorem.Paragraphs(5)+"[FakeData]");
            fakerPost.RuleFor(p=>p.DateCreate,faker=>faker.Date.Between(new DateTime(2000,1,1),new DateTime(2022,1,1)));
            fakerPost.RuleFor(p=>p.Description,faker=>faker.Lorem.Sentence(3));
            fakerPost.RuleFor(p=>p.Published,faker=>true);
            fakerPost.RuleFor(p=>p.Slug,faker=>faker.Lorem.Slug());
            fakerPost.RuleFor(p=>p.Title,faker=>$"Bai {baiviet++}"+faker.Lorem.Sentence(3,4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> post_category = new List<PostCategory>();

            for(int i=0; i<40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdate = post.DateCreate;
                posts.Add(post);
                post_category.Add(new PostCategory{
                    Post = post,
                    Category = cates[rCateIndex.Next(5)]
                });
            }
            _dbContext.AddRange(posts);
            _dbContext.AddRange(post_category);
            _dbContext.SaveChanges();

        }
    }
}