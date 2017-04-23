using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MasterDetail.Controllers
{
    [Authorize]
    public class WorkListController : Controller
    {
        ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

        public ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }


        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            List<string> userRolesList = UserManager.GetRoles(userId).ToList();

            IEnumerable<IWorkListItem> workListItemsToDisplay = new List<IWorkListItem>();
            workListItemsToDisplay = workListItemsToDisplay.Concat(GetWorkOrders(userId, userRolesList));


            return View(workListItemsToDisplay.OrderByDescending(wl => wl.PriorityScore));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext.Dispose();
            }

            base.Dispose(disposing);
        }


        private IEnumerable<IWorkListItem> GetWorkOrders(string userId, List<string> userRolesList)
        {
            // We start with the work orders from the db context. We filter that list down to the
            // work orders who have the status other than approved and then convert them to 
            // a list. We then ask if any of the user roles for those work orders is contained 
            // in the RolesWhichCanClaim list for each work order in that list.
            IEnumerable<IWorkListItem> claimableWorkOrders =
                _applicationDbContext.WorkOrders.Where(wo => wo.WorkOrderStatus != WorkOrderStatus.Approved)
                                                .ToList()
                                                .Where(wo => userRolesList.Any(ur => wo.RolesWhichCanClaim.Contains(ur)));

            IEnumerable<IWorkListItem> workOrdersIAmWorkingOn =
                _applicationDbContext.WorkOrders
                                     .Where(wo => wo.CurrentWorkerId == userId);

            return claimableWorkOrders.Concat(workOrdersIAmWorkingOn);
        }
    }
}