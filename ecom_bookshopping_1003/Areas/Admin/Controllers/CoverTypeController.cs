using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1003.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWorkRepository _unitofwork;
        public CoverTypeController(IUnitOfWorkRepository unitofwork)
        {
            _unitofwork = unitofwork; 
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id== null) return View(coverType);
            coverType = _unitofwork.coverType.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
            return View(coverType);

        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return BadRequest();
            if (!ModelState.IsValid) return NotFound();
            if (coverType.Id == 0)
                _unitofwork.coverType.Add(coverType);
            else
                _unitofwork.coverType.Update(coverType);
            _unitofwork.save();
            return RedirectToAction("Index");
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.coverType.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var covertype = _unitofwork.coverType.Get(id);
            if (covertype == null)
                return Json(new { success = false, message = "Unable to Delete Data" });
            _unitofwork.coverType.Remove(covertype);
            _unitofwork.save();
            return Json(new { success = true, message = "Data Deleted Successfully" });
        }
        #endregion
    }
}
