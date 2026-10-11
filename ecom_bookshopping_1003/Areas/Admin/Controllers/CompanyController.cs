using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1003.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;
        public CompanyController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            company = _unitOfWork.company.Get(id.GetValueOrDefault());
            if (company == null)
                return NotFound();
            return View(company);
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return View(company);
            if (company.Id == 0)
                _unitOfWork.company.Add(company);
            else
                _unitOfWork.company.Update(company);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.company.GetAll() });
        }
        
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companylist = _unitOfWork.company.Get(id);
            if (companylist == null)
                return Json(new { success = false, message = "unable to delete data..." });
            _unitOfWork.company.Remove(companylist);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data deleted successfully..." });
        }
        #endregion
    }
}
