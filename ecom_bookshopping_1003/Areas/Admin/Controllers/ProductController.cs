using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Models.ViewModels;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ecom_bookshopping_1003.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWorkRepository _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWorkRepository unitofwork, IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                product = new Product(),
                CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value=cl.Id.ToString()
                }),
                CoverTypeList=_unitofwork.coverType.GetAll().Select(ct=> new SelectListItem()
                {
                    Text=ct.Name,
                    Value=ct.Id.ToString()
                })
            };
            if (id == null) return View(productVM);
            productVM.product = _unitofwork.product.Get(id.GetValueOrDefault());
            if (productVM.product == null) return NotFound();
            return View(productVM);
            

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    var filename = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var uploads = Path.Combine(webRootPath, @"Images/Products");
                    if (productVM.product.Id != 0)
                    {
                        var iamgexist = _unitofwork.product.Get(productVM.product.Id).Imageurl;
                        productVM.product.Imageurl = iamgexist;

                    }
                    if (productVM.product.Imageurl != null)
                    {
                        var imagepath = Path.Combine(webRootPath, productVM.product.Imageurl.Trim('\\'));
                        if (System.IO.File.Exists(imagepath))
                        {
                            System.IO.File.Delete(imagepath);
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    productVM.product.Imageurl = @"\Images\Products\" + filename + extension;

                }
                else
                {
                    if (productVM.product.Id != 0)
                    {
                        var imageexist = _unitofwork.product.Get(productVM.product.Id).Imageurl;
                        productVM.product.Imageurl = imageexist;
                    }
                }
                if (productVM.product.Id == 0)
                    _unitofwork.product.Add(productVM.product);
                else
                    _unitofwork.product.Update(productVM.product);
                _unitofwork.save();
                return RedirectToAction("Index");
            }
            else
            {
                productVM = new ProductVM()
                {
                    product = new Product(),

                    CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitofwork.coverType.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })


                };
                if (productVM.product.Id != 0)
                {
                    productVM.product = _unitofwork.product.Get(productVM.product.Id);
                    if (productVM.product == null) return NotFound();


                }
                return View(productVM);
            }
        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.product.GetAll() });
        }
        #endregion

    }
}
