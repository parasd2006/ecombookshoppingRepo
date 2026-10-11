using ecom_bookshopping_1003.Data.DataAccess;
using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1003.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWorkRepository _unitofwork;
        public CategoryController(IUnitOfWorkRepository unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null) return View(category);
            category = _unitofwork.category.Get(id.GetValueOrDefault());

            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return BadRequest();
            if (!ModelState.IsValid)
                return View(category);
            if (category.Id == 0)
                _unitofwork.category.Add(category);
            else
                _unitofwork.category.Update(category);
            _unitofwork.save();
            return RedirectToAction("Index");

        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var categorylist = _unitofwork.category.GetAll();
            return Json(new { data = categorylist });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _unitofwork.category.Get(id);
            if (category == null)
                return Json(new { success = false, message = "unable to delete data!!!.." });
            _unitofwork.category.Remove(category);
            _unitofwork.save();
            return Json(new { success = true, message = "successfully delete data.." });
        }
        #endregion
    }
}
