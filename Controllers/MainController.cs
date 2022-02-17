using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Task3.Areas.Identity.Pages.Account;
using Task3.Data;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace Task3.Controllers
{
    public class MainController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Task3.Data.ApplicationDbContext db;
        UserManager<ApplicationUser> _userManager;

        public MainController(SignInManager<ApplicationUser> signInManager, 
                              Task3.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            db = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null ^ user.Status == "Blocked")
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(db.Users.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> MultiBlock(List<string> Group)
        {
            bool BlockedUser = false;
            foreach (string id in Group)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user.UserName == User.Identity.Name)
                {
                    user.Status = "Blocked";
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                    BlockedUser = true;
                }

                user.Status = "Blocked";
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
            if (BlockedUser)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MultiDelete(List<string> Group)
        {
            foreach(string id in Group)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    if (user.UserName == User.Identity.Name)
                    {
                        await _signInManager.SignOutAsync();
                    }
                    IdentityResult result = await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MultiUnblock(List<string> Group)
        {

            foreach (string id in Group)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                user.Status = "Not Blocked";
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
