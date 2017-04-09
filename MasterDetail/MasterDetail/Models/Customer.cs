using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }

        // Since we specified in WorkOrderConfiguration that the relationship
        // between Customer & WorkOrder will NOT cascade delete, we have to 
        // explicitly add a property of WorkOrder collection in this model class.
        public List<WorkOrder> WorkOrders { get; set; }
    }
}