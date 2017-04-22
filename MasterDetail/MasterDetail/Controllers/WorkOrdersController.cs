using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using Microsoft.AspNet.Identity;

namespace MasterDetail.Controllers
{
    [Authorize]
    public class WorkOrdersController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();


        public async Task<ActionResult> Index()
        {
            var workOrders = _applicationDbContext.WorkOrders.Include(w => w.CurrentWorker).Include(w => w.Customer);

            return View(await workOrders.ToListAsync());
        }


        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }

            return View(workOrder);
        }


        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers, "CustomerId", "CompanyName");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                workOrder.CurrentWorkerId = User.Identity.GetUserId();
                _applicationDbContext.WorkOrders.Add(workOrder);
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Edit", new { controller = "WorkOrders", action = "Edit", Id = workOrder.WorkOrderId });
            }

            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers, "CustomerId", "AccountNumber", workOrder.CustomerId);

            return View(workOrder);
        }


        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);

            if (workOrder == null)
            {
                return HttpNotFound();
            }

            return View(workOrder);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId,ReworkNotes")] WorkOrder workOrder, string command)
        {
            if (ModelState.IsValid)
            {
                // Populate Parts and Labors.
                workOrder.Parts =
                    _applicationDbContext.Parts.Where(p => p.WorkOrderId == workOrder.WorkOrderId).ToList();
                workOrder.Labors =
                    _applicationDbContext.Labors.Where(l => l.WorkOrderId == workOrder.WorkOrderId).ToList();

                var promotionResult = new PromotionResult();

                if (command == "Save")
                {
                    promotionResult.Success = true;
                }
                else if (command == "Claim")
                {
                    promotionResult = workOrder.ClaimWorkOrder(User.Identity.GetUserId());
                }
                else
                {
                    promotionResult = workOrder.PromoteWorkOrder(command);
                }

                if (!promotionResult.Success)
                {
                    TempData["MessageToClient"] = promotionResult.Message;
                }

                _applicationDbContext.Entry(workOrder).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(workOrder);
        }


        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }

            return View(workOrder);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            _applicationDbContext.WorkOrders.Remove(workOrder);
            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
