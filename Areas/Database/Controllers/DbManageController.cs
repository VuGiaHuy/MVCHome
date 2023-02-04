using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Area.DbMangae.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        [TempData]
        public string StatusMessage {get;set;}
        private readonly AppDbContext _dbContext;

        public DbManageController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
    }
}