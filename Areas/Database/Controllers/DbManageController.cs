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

namespace Area.DbMangae.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        [TempData]
        public string StatusMessage {get;set;}
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
            await  _dbContext.Database.MigrateAsync();
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
            StatusMessage = (success)?"Delete DB success":"Can't Delete DB";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> SeedDataAsync()
        { 
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach(var role in roleNames)
            {
                var roleName = (string)role.GetRawConstantValue();
                var roleFound = await _roleManager.FindByNameAsync(roleName);
                if(roleFound==null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            var admin = await _userManager.FindByNameAsync("admin");
            if(admin==null)
            {
                admin = new AppUser{
                    Email = "admin@gmail.com",
                    UserName = "admin",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin,"admin123");
                await _userManager.AddToRoleAsync(admin,RoleName.Administrator);
            }
            await _signInManager.SignInAsync(admin,true);
            StatusMessage = "Seed data success";
            return RedirectToAction(nameof(Index));
        }
    }
}