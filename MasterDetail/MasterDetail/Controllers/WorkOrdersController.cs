﻿using System;
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
            //ViewBag.CurrentWorkerId = new SelectList(_applicationDbContext.ApplicationUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers, "CustomerId", "AccountNumber");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,Total,CertificationRequirements,CurrentWorkerId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                workOrder.CurrentWorkerId = User.Identity.GetUserId();

                _applicationDbContext.WorkOrders.Add(workOrder);
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Edit", new { controller = "WorkOrders", action = "Edit", Id = workOrder.WorkOrderId });
            }

            //ViewBag.CurrentWorkerId = new SelectList(_applicationDbContext.ApplicationUsers, "Id", "FirstName", workOrder.CurrentWorkerId);
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
        public async Task<ActionResult> Edit([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,Total,CertificationRequirements,CurrentWorkerId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Entry(workOrder).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            //ViewBag.CurrentWorkerId = new SelectList(_applicationDbContext.ApplicationUsers, "Id", "FirstName", workOrder.CurrentWorkerId);
            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers, "CustomerId", "AccountNumber", workOrder.CustomerId);

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
