using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using MasterDetail.DataLayer;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterDetail.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        // Since we specified in WorkOrderConfiguration that the relationship
        // between ApplicationUser & WorkOrder will NOT cascade delete, we have to 
        // explicitly add a WorkOrder collection property in this model class.
        public List<WorkOrder> WorkOrders { get; set; }

        public string FullName => FirstName + " " + LastName;

        public string AddressBlock
        {
            get
            {
                string addressBlock = string.Format($"{Address}<br/>{City}, {State} {ZipCode}").Trim();

                return addressBlock == "<br/>," ? string.Empty : addressBlock;
            }
        }




        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}