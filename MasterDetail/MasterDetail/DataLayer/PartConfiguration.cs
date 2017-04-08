using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterDetail.Models;

namespace MasterDetail.DataLayer
{
    public class PartConfiguration : EntityTypeConfiguration<Part>
    {
        public PartConfiguration()
        {
            Property(p => p.InventoryItemCode)
                .HasMaxLength(15)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    // To prevent duplicate parts on the same workorder, 
                    // we need the unique index over the combination of InventoryItemCode and the parent's workorder id.
                    // This is done by placing the IndexAnnotation on both columns and then telling EF which column
                    // comes first in the index and which column comes second. The most restrictive columns come first
                    // in the index structure. Hence why WorkOrderId column is indexed as 1 and this InventoryItemCode
                    // column is indexed as 2.
                    new IndexAnnotation(new IndexAttribute("AK_Part", 2) { IsUnique = true }));

            Property(p => p.WorkOrderId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("AK_Part", 1) { IsUnique = true }));


            Property(p => p.InventoryItemName)
                .HasMaxLength(80)
                .IsRequired();

            Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            // This is a computed column in the database so we must explicitly tell EF.
            Property(p => p.ExtendedPrice)
                .HasPrecision(18, 2)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            Property(p => p.Notes)
                .HasMaxLength(140)
                .IsOptional();
        }
    }
}
