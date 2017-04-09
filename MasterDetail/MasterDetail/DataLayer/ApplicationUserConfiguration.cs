using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterDetail.Models;

namespace MasterDetail.DataLayer
{
    public class ApplicationUserConfiguration : EntityTypeConfiguration<ApplicationUser>
    {
        public ApplicationUserConfiguration()
        {
            Property(au => au.FirstName)
                .HasMaxLength(15)
                .IsOptional();
        }
    }
}
