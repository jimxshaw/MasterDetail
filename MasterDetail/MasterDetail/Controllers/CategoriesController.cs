using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using MasterDetail.ViewModels;
using TreeUtility;

namespace MasterDetail.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            // Start the outermost list
            string fullString = "<ul>";

            IList<Category> listOfNodes = GetListOfNodes();
            IList<Category> topLevelCategories = TreeHelper.ConvertToForest(listOfNodes);

            foreach (var category in topLevelCategories)
            {
                fullString += EnumerateNodes(category);
            }

            // End the outermost list
            fullString += "</ul>";

            return View((object)fullString);
        }


        public ActionResult Create()
        {
            ViewBag.ParentCategoryId = PopulateParentCategorySelectList(null);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ParentCategoryId,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ValidateParentsAreParentless(category);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.ParentCategoryId = PopulateParentCategorySelectList(null);

                    return View(category);
                }


                _applicationDbContext.Categories.Add(category);
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.ParentCategoryId = PopulateParentCategorySelectList(category.ParentCategoryId);

            return View(category);
        }


        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = await _applicationDbContext.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Wind up a Category view model.
            var categoryViewModel = new CategoryViewModel();
            categoryViewModel.Id = category.Id;
            categoryViewModel.ParentCategoryId = category.ParentCategoryId;
            categoryViewModel.CategoryName = category.CategoryName;

            ViewBag.ParentCategoryId = PopulateParentCategorySelectList(category.ParentCategoryId);

            return View(categoryViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentCategoryId,CategoryName")] CategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                // Unwind back to a Category.
                var editedCategory = new Category();

                try
                {
                    editedCategory.Id = categoryViewModel.Id;
                    editedCategory.ParentCategoryId = categoryViewModel.ParentCategoryId;
                    editedCategory.CategoryName = categoryViewModel.CategoryName;

                    ValidateParentsAreParentless(editedCategory);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.ParentCategoryId = PopulateParentCategorySelectList(categoryViewModel.ParentCategoryId);

                    return View("Edit", categoryViewModel);
                }

                _applicationDbContext.Entry(editedCategory).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.ParentCategoryId = PopulateParentCategorySelectList(categoryViewModel.ParentCategoryId);

            return View(categoryViewModel);
        }


        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _applicationDbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Category category = await _applicationDbContext.Categories.FindAsync(id);

            try
            {
                _applicationDbContext.Categories.Remove(category);
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "You attempted to delete a category that has child categories.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

            }

            return View("Delete", category);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext.Dispose();
            }
            base.Dispose(disposing);
        }





        private List<Category> GetListOfNodes()
        {
            List<Category> sourceCategories = _applicationDbContext.Categories.ToList();
            List<Category> categories = new List<Category>();

            foreach (var sourceCategory in sourceCategories)
            {
                var c = new Category();
                c.Id = sourceCategory.Id;
                c.CategoryName = sourceCategory.CategoryName;

                if (sourceCategory.ParentCategoryId != null)
                {
                    c.Parent = new Category();
                    c.Parent.Id = (int)sourceCategory.ParentCategoryId;
                }

                categories.Add(c);
            }

            return categories;
        }

        private string EnumerateNodes(Category parent)
        {
            // Initialize empty string. 
            string content = string.Empty;

            // Add <li> category name.
            content += "<li class=\"treenode\">";
            content += parent.CategoryName;
            content += string.Format($"<a href=\"/Categories/Edit/{parent.Id}\" class=\"btn btn-primary btn-xs treenodeeditbutton\">Edit</a>");
            content += string.Format($"<a href=\"/Categories/Delete/{parent.Id}\" class=\"btn btn-danger btn-xs treenodedeletebutton\">Delete</a>");


            // If there are no children, end the </li>.
            if (parent.Children.Count == 0)
            {
                content += "</li>";
            }
            else
            {
                content += "<ul>";
            }

            // Loop one past the number of children.
            int numberOfChildren = parent.Children.Count;
            for (int i = 0; i <= numberOfChildren; i++)
            {
                // If this iteration's index points to a child,
                // call this function recursively.
                if (numberOfChildren > 0 && i < numberOfChildren)
                {
                    Category child = parent.Children[i];
                    content += EnumerateNodes(child);
                }

                // If this iteration's index points past the children, end the </ul>
                if (numberOfChildren > 0 && i == numberOfChildren)
                {
                    content += "</ul>";
                }
            }

            return content;
        }

        private void ValidateParentsAreParentless(Category category)
        {
            // If there is no parent then the category is the root and
            // nothing more needs to be done.
            if (category.ParentCategoryId == null)
            {
                return;
            }

            // The parent has a parent.
            Category parentCategory = _applicationDbContext.Categories.Find(category.ParentCategoryId);

            if (parentCategory.ParentCategoryId != null)
            {
                throw new InvalidOperationException("You cannot nest this category more than two levels deep.");
            }

            // The parent does NOT have a parent but the category being nested has children.
            int numberOfChildren = _applicationDbContext.Categories.Count(c => c.ParentCategoryId == category.Id);

            if (numberOfChildren > 0)
            {
                throw new InvalidOperationException("You cannot nest this category's children more than two levels deep.");
            }
        }

        private SelectList PopulateParentCategorySelectList(int? id)
        {
            SelectList selectedList;

            if (id == null)
            {
                selectedList = new SelectList(
                    _applicationDbContext
                        .Categories
                        .Where(c => c.ParentCategoryId == null), "Id", "CategoryName"
                );
            }
            else if (_applicationDbContext.Categories.Count(c => c.ParentCategoryId == id) == 0)
            {
                selectedList = new SelectList(
                    _applicationDbContext
                        .Categories
                        .Where(c => c.ParentCategoryId == null && c.Id != id), "Id", "CategoryName"
                );
            }
            else
            {
                selectedList = new SelectList(
                    _applicationDbContext
                        .Categories
                        .Where(c => false), "Id", "CategoryName"
                );
            }

            return selectedList;
        }
    }
}
