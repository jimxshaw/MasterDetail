using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MasterDetail.Models
{
    public class WorkOrder
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


        // Claiming a work order does two things: First, it promotes the work order
        // to its next active status "ing status". Second, it relates the work order
        // to the authenticated user who claimed it.
        public PromotionResult ClaimWorkOrder(string userId)
        {
            var promotionResult = new PromotionResult();

            if (!promotionResult.Success)
            {
                return promotionResult;
            }

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


        public PromotionResult PromoteWorkOrder(string command)
        {
            var promotionResult = new PromotionResult();

            //switch (command)
            //{
            //    case "PromoteToCreated":
            //        promotionResult = PromoteToCreated();
            //        break;

            //    case "PromoteToProcessed":
            //        promotionResult = PromoteToProcessed();
            //        break;

            //    case "PromoteToCertified":
            //        promotionResult = PromoteToCertified();
            //        break;

            //    case "PromoteToApproved":
            //        promotionResult = PromoteToApproved();
            //        break;

            //    case "DemoteToCreated":
            //        promotionResult = DemoteToCreated();
            //        break;

            //    case "DemoteToRejected":
            //        promotionResult = DemoteToRejected();
            //        break;

            //    case "DemoteToCanceled":
            //        promotionResult = DemoteToCanceled();
            //        break;
            //}

            if (promotionResult.Success)
            {
                CurrentWorker = null;
                CurrentWorkerId = null;

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
