using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MasterDetail.Controllers;

namespace MasterDetail.Models
{
    public class WorkOrder : IWorkListItem
    {
        public int WorkOrderId { get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "You must choose a customer.")]
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDateTime { get; set; }

        [Display(Name = "Target Date")]
        public DateTime? TargetDateTime { get; set; }

        [Display(Name = "Drop Dead Date")]
        public DateTime? DropDeadDateTime { get; set; }

        [StringLength(256, ErrorMessage = "The description must be 256 characters or shorter.")]
        public string Description { get; set; }

        [Display(Name = "Status")]
        public WorkOrderStatus WorkOrderStatus { get; set; }

        public decimal Total { get; set; }

        [Display(Name = "Certifications Needed")]
        [StringLength(120, ErrorMessage = "The certification requirements must be 120 characters or shorter.")]
        public string CertificationRequirements { get; set; }

        public virtual ApplicationUser CurrentWorker { get; set; }

        public string CurrentWorkerId { get; set; }

        public List<Part> Parts { get; set; }

        public List<Labor> Labors { get; set; }

        [Display(Name = "Rework Notes")]
        [StringLength(256, ErrorMessage = "Rework notes must be 256 characters or fewer.")]
        public string ReworkNotes { get; set; }


        public WorkOrder()
        {
            WorkOrderStatus = WorkOrderStatus.Creating;
            Parts = new List<Part>();
            Labors = new List<Labor>();
        }

        // Claiming a work order does two things: First, it promotes the work order
        // to its next active status "ing status". Second, it relates the work order
        // to the authenticated user who claimed it.
        public PromotionResult ClaimWorkListItem(string userId)
        {
            var promotionResult = new PromotionResult();

            //if (!promotionResult.Success)
            //{
            //    return promotionResult;
            //}

            // A work order could be claimed in Rejected, Created, Processed or Certified
            // status. Use a switch statement to handle those scenarios.
            switch (WorkOrderStatus)
            {
                case WorkOrderStatus.Rejected:
                    promotionResult = PromoteToProcessing();
                    break;
                case WorkOrderStatus.Created:
                    promotionResult = PromoteToProcessing();
                    break;
                case WorkOrderStatus.Processed:
                    promotionResult = PromoteToCertifying();
                    break;
                case WorkOrderStatus.Certified:
                    promotionResult = PromoteToApproving();
                    break;
            }

            // After the switch statement, we assign userId to CurrentWorkerId
            // if the promotion succeeded. E.g. When the current status is Rejected and
            // the work order is claimed, it's status is promoted to Processing.
            if (promotionResult.Success)
            {
                CurrentWorkerId = userId;
            }

            return promotionResult;
        }


        public PromotionResult PromoteWorkListItem(string command)
        {
            var promotionResult = new PromotionResult();

            switch (command)
            {
                case "PromoteToCreated":
                    promotionResult = PromoteToCreated();
                    break;

                case "PromoteToProcessed":
                    promotionResult = PromoteToProcessed();
                    break;

                case "PromoteToCertified":
                    promotionResult = PromoteToCertified();
                    break;

                case "PromoteToApproved":
                    promotionResult = PromoteToApproved();
                    break;

                case "DemoteToCreated":
                    promotionResult = DemoteToCreated();
                    break;

                case "DemoteToRejected":
                    promotionResult = DemoteToRejected();
                    break;

                case "DemoteToCanceled":
                    promotionResult = DemoteToCancelled();
                    break;
            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, "WorkOrders", WorkOrderId, HttpContext.Current.User.Identity.Name, null);

            if (promotionResult.Success)
            {
                CurrentWorker = null;
                CurrentWorkerId = null;

                // Attempt auto-promotion from Certified to Approved if the total value of Parts & Labors
                // is less than $5000.
                if (WorkOrderStatus == WorkOrderStatus.Certified && Parts.Sum(p => p.ExtendedPrice) + Labors.Sum(l => l.ExtendedPrice) < 5000)
                {
                    PromotionResult autoPromotionResult = PromoteToApproved();

                    if (autoPromotionResult.Success)
                    {
                        promotionResult = autoPromotionResult;
                        promotionResult.Message = "AUTOMATIC PROMOTION: " + promotionResult.Message;
                    }
                }
            }

            return promotionResult;
        }


        private PromotionResult PromoteToProcessing()
        {
            if (WorkOrderStatus == WorkOrderStatus.Created || WorkOrderStatus == WorkOrderStatus.Rejected)
            {
                WorkOrderStatus = WorkOrderStatus.Processing;
            }

            var promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Processing;

            if (promotionResult.Success)
                promotionResult.Message =
                    $"Work order {WorkOrderId} successfully claimed by {HttpContext.Current.User.Identity.Name} and promoted to status {WorkOrderStatus}.";
            else
                promotionResult.Message = "Failed to promote the work order to Processing status because its current status prevented it.";

            return promotionResult;
        }


        private PromotionResult PromoteToCertifying()
        {
            if (WorkOrderStatus == WorkOrderStatus.Processed)
            {
                WorkOrderStatus = WorkOrderStatus.Certifying;
            }

            var promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Certifying;

            if (promotionResult.Success)
                promotionResult.Message =
                    $"Work order {WorkOrderId} successfully claimed by {HttpContext.Current.User.Identity.Name} and promoted to status {WorkOrderStatus}.";
            else
                promotionResult.Message = "Failed to promote the work order to Certifying status because its current status prevented it.";

            return promotionResult;
        }

        private PromotionResult PromoteToApproving()
        {
            if (WorkOrderStatus == WorkOrderStatus.Certified)
            {
                WorkOrderStatus = WorkOrderStatus.Approving;
            }

            var promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Approving;

            if (promotionResult.Success)
                promotionResult.Message =
                    $"Work order {WorkOrderId} successfully claimed by {HttpContext.Current.User.Identity.Name} and promoted to status {WorkOrderStatus}.";
            else
                promotionResult.Message = "Failed to promote the work order to Approving status because its current status prevented it.";

            return promotionResult;
        }

        private PromotionResult PromoteToCreated()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Creating)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because its current status prevented it.";
            }
            else if (String.IsNullOrWhiteSpace(TargetDateTime.ToString()) ||
                String.IsNullOrWhiteSpace(DropDeadDateTime.ToString()) ||
                String.IsNullOrWhiteSpace(Description))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because it requires a Target Date, Drop Dead Date, and Description.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Created;
                promotionResult.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        private PromotionResult PromoteToProcessed()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Processing)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because its current status prevented it.";
            }
            else if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because it did not contain at least one part and at least one labor item.";
            }
            else if (String.IsNullOrWhiteSpace(Description))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because it requires a Description.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Processed;
                promotionResult.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        private PromotionResult PromoteToCertified()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Certifying)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because its current status prevented it.";
            }

            if (string.IsNullOrWhiteSpace(CertificationRequirements))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because Certification Requirements were not present.";
            }
            else if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because it did not contain at least one part and at least one labor item.";
            }
            else if (Parts.Count(p => p.IsInstalled == false) > 0 || Labors.Count(l => l.PercentComplete < 100) > 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because not all parts have been installed and labor completed.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Certified;
                promotionResult.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        // Approved is the last status a work order will ever achieve.
        private PromotionResult PromoteToApproved()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving && WorkOrderStatus != WorkOrderStatus.Certified)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Approved status because its current status prevented it.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Approved;
                promotionResult.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        // Created status can be achieved through promotion from Creating or demotion from Approving.
        private PromotionResult DemoteToCreated()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Created status because its current status prevented it.";
            }

            // When a work order is rejected, the admin needs to provide information on why it's demoted.
            if (String.IsNullOrWhiteSpace(ReworkNotes))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Created status because Rework Notes must be present.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Created;
                promotionResult.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        private PromotionResult DemoteToRejected()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Rejected status because its current status prevented it.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Rejected;
                promotionResult.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }


        private PromotionResult DemoteToCancelled()
        {
            var promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Cancelled status because its current status prevented it.";
            }

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Cancelled;
                promotionResult.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}.";
            }

            return promotionResult;
        }

        public int Id => WorkOrderId;

        public string Status => WorkOrderStatus.ToString();

        public string CurrentWorkerName
        {
            get
            {
                if (CurrentWorker == null)
                {
                    return string.Empty;
                }

                return CurrentWorker.FullName;
            }
        }

        public string EntityFamiliarName => "Worker Order";

        public string EntityFamiliarNamePlural => "Worker Orders";

        public string EntityFormalName => "Worker Order";

        public string EntityFormalNamePlural => "Worker Orders";

        public int PriorityScore
        {
            get { return 0; }
        }

        public IEnumerable<string> RolesWhichCanClaim
        {
            get
            {
                List<string> rolesWhichCanClaim = new List<string>();

                switch (WorkOrderStatus)
                {
                    case WorkOrderStatus.Created:
                        rolesWhichCanClaim.Add("Clerk");
                        rolesWhichCanClaim.Add("Manager");
                        break;

                    case WorkOrderStatus.Processed:
                        rolesWhichCanClaim.Add("Manager");
                        rolesWhichCanClaim.Add("Admin");
                        break;

                    case WorkOrderStatus.Certified:
                        rolesWhichCanClaim.Add("Admin");
                        break;

                    case WorkOrderStatus.Rejected:
                        rolesWhichCanClaim.Add("Manager");
                        rolesWhichCanClaim.Add("Admin");
                        break;
                }

                return rolesWhichCanClaim;
            }
        }

        public PromotionResult RelinquishWorkListItem()
        {
            throw new NotImplementedException();
        }
    }

    public enum WorkOrderStatus
    {
        Creating = 5,
        Created = 10,
        Processing = 15,
        Processed = 20,
        Certifying = 25,
        Certified = 30,
        Approving = 35,
        Approved = 40,
        Rejected = -10,
        Cancelled = -20
    }
}
