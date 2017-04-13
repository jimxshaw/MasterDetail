using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;
using MasterDetail.Models;

namespace MasterDetail.DataLayer
{
    public class CategoryConfiguration : EntityTypeConfiguration<Category>
    {
        public CategoryConfiguration()
        {
            Property(c => c.Id).HasColumnName("CategoryId");

            Property(c => c.CategoryName)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("AK_Category_CategoryName") { IsUnique = true }));

            // A Category has an optional relationship with the Parent, which can have
            // many Children related to it, through a foreign key named ParentCategoryId
            // and the relationship will not cascade delete from Parent to Children.
            // If we attempt to delete a Category that's parent to one or more children,
            // the db don't automatically delete the children along with the parent. It
            // will instead throw an exception.
            HasOptional(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .WillCascadeOnDelete(false);
        }
    }
}