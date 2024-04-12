using Food_Web.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.Web;
using Microsoft.AspNet.Identity;
using static Food_Web.Models.ManageController;

namespace Food_Web.Areas.Admin.Controllers
{
    public class MemberController : Controller
    {
        // GET: Admin/Member
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public MemberController()
        {
            _context = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        // GET: Member
        public async Task<ActionResult> Index()
        {
            var role = await _roleManager.FindByNameAsync("User");
            var members = await _userManager.Users
                .Where(u => u.Roles.Any(r => r.RoleId == role.Id))
                .ToListAsync();
            return View(members);
        }

        public async Task<ActionResult> User()
        {
            var role = await _roleManager.FindByNameAsync("Member");
            var members = await _userManager.Users
                .Where(u => u.Roles.Any(r => r.RoleId == role.Id))
                .ToListAsync();
            return View(members);
        }


        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        public ActionResult Create()
        {
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user.Id, "User");
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            return View(model);
        }

        // GET: Member/Delete/{id}
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Member/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to delete user.");
                return View(user);
            }

            return RedirectToAction("Index");
        }

        // GET: Member/Approve/{id}
        public async Task<ActionResult> Approve(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
        // GET: Member/Block/{id}
        public async Task<ActionResult> Block(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.IsApproved == true)
            {
                user.IsApproved = false;
            }
            else
            {
                user.IsApproved = true;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

       
    }
}
