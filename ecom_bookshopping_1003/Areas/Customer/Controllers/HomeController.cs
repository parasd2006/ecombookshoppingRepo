using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Models.ViewModels;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Security.Claims;

namespace ecom_bookshopping_1003.Areas.Customer.Controllers
{
    [Area("Customer")]
    
    public class HomeController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;
        public HomeController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var cliamsIdentity = (ClaimsIdentity)User.Identity;
            var claims = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims!=null)
            {
                var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId ==
                claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
          
            var orderDetails = _unitOfWork.orderDetail.GetAll();
            var soldCounts = orderDetails.GroupBy(od => od.ProductId).ToDictionary(
                g => g.Key, g => g.Sum(od => od.Count));
                                                       
            var products = _unitOfWork.product.GetAll();
            
            var modelList = products.Select(p => new ProductVM
            {
                product = p,
                SoldCount = soldCounts.ContainsKey(p.Id) ? soldCounts[p.Id] : 0
            }).OrderByDescending(p => p.SoldCount).ToList();
                                                             
            return View(modelList);
        }
        public IActionResult Details(int id)
        {
            var cliamsIdentity = (ClaimsIdentity)User.Identity;
            var claims = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            var productInDb = _unitOfWork.product.FirstOrDefault(p => p.Id == id,
                includeproperties: "Category,CoverType");
            if (productInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                product = productInDb,
                ProductId = id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if(ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingCartInDb = _unitOfWork.shoppingCart.FirstOrDefault(sc => sc.ApplicationUserId == claims.Value
                && sc.ProductId == shoppingCart.ProductId);

                if (shoppingCartInDb == null)
                    _unitOfWork.shoppingCart.Add(shoppingCart);
                else
                    shoppingCartInDb.Count += shoppingCart.Count;
                _unitOfWork.save();
                return RedirectToAction("Index");
            }
            else
            {
                var productInDb = _unitOfWork.product.FirstOrDefault(p =>
                p.Id == shoppingCart.ProductId, includeproperties: "Category,CoverType");
                if (productInDb == null) return NotFound();
                var shoppingcartEdit = new ShoppingCart();
                return View(shoppingCart);
            }
            return View(shoppingCart);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
