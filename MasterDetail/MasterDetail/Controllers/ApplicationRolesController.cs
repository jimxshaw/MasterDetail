using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using Microsoft.AspNet.Identity.Owin;

namespace MasterDetail.Controllers
{
    public class ApplicationRolesController : Controller
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            }
            set { _userManager = value; }
        }

        private ApplicationRoleManager _roleManager;

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            set { _roleManager = value; }
        }

        public ApplicationRolesController()
        {

        }

        public ApplicationRolesController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public async Task<ActionResult> Index()
        {
            return View(await RoleManager.Roles.ToListAsync());
        }

        //private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ApplicationRoles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationRole applicationRole = await RoleManager.FindByIdAsync(id);
            if (applicationRole == null)
            {
                return HttpNotFound();
            }
            return View(applicationRole);
        }

        // GET: ApplicationRoles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ApplicationRoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name")] ApplicationRole applicationRole)
        {
            if (ModelState.IsValid)
            {
                var roleResult = await RoleManager.CreateAsync(applicationRole);

                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("", roleResult.Errors.First());

                    return View();
                }

                return RedirectToAction("Index");
            }

            return View(applicationRole);
        }

        // GET: ApplicationRoles/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationRole applicationRole = await RoleManager.FindByIdAsync(id);
            if (applicationRole == null)
            {
                return HttpNotFound();
            }
            return View(applicationRole);
        }

        // POST: ApplicationRoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] ApplicationRole applicationRole)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole retrievedApplicationRole = await RoleManager.FindByIdAsync(applicationRole.Id);
                string originalName = retrievedApplicationRole.Name;

                if (originalName == "Admin" && applicationRole.Name != "Admin")
                {
                    ModelState.AddModelError("", "You cannot change the name of the Admin role.");

                    return View(applicationRole);
                }

                if (originalName != "Admin" && applicationRole.Name == "Admin")
                {
                    ModelState.AddModelError("", "You cannot change the name of a role to Admin.");

                    return View(applicationRole);
                }

                retrievedApplicationRole.Name = applicationRole.Name;

                await RoleManager.UpdateAsync(retrievedApplicationRole);

                return RedirectToAction("Index");
            }
            return View(applicationRole);
        }

        //// GET: ApplicationRoles/Delete/5
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ApplicationRole applicationRole = await db.IdentityRoles.FindAsync(id);
        //    if (applicationRole == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(applicationRole);
        //}

        //// POST: ApplicationRoles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(string id)
        //{
        //    ApplicationRole applicationRole = await db.IdentityRoles.FindAsync(id);
        //    db.IdentityRoles.Remove(applicationRole);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RoleManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
