using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Models.ViewModels;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.V2.Core;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ecom_bookshopping_1003.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;
        private static bool isEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
       
        public CartController(IUnitOfWorkRepository unitOfWork,
            IEmailSender emailSender, IConfiguration config,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _config = config;
            _userManager = userManager;
        }
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null)
            {
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(shoppingCartVM);
            }

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value,
                    includeproperties: "product"),
                orderHeader = new OrderHeader()
            };
            shoppingCartVM.orderHeader.OrderTotal = 0;

            // User fetch kiya
            var applicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            shoppingCartVM.orderHeader.ApplicationUser = applicationUser;

            foreach (var list in shoppingCartVM.ListCart)
            {
                list.price = SD.GetPriceBasedOnQuantity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (list.Count * list.price);

                if (list.product.Description.Length > 100)
                {
                    list.product.Description = list.product.Description.Substring(0, 99) + "...";
                }
            }

            // Email status message set karo
            if (applicationUser != null && !applicationUser.EmailConfirmed)
            {
                ViewBag.EmailMessage = "Email must be confirmed for authorized customers.";
                ViewBag.EmailCss = "text-danger";
            }

            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if (user == null)
                ModelState.AddModelError(string.Empty, "Email Empty!!");
            else
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code,  },
                    protocol: Request.Scheme)!;

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult plus(int id)
        {
            var cart = _unitOfWork.shoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult minus(int id)
        {
            var cart = _unitOfWork.shoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var deleteInDb = _unitOfWork.shoppingCart.Get(id);
            _unitOfWork.shoppingCart.Remove(deleteInDb);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        
        public IActionResult Summary(string selectedIds)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var previousOrders = _unitOfWork.orderHeader.GetAll(o => o.ApplicationUserId == claims.Value)
                .GroupBy(o => new { o.StreetAddress, o.City, o.PostalCode })
                .Select(g => g.First())
                .ToList();

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeproperties: "product"),
                orderHeader = new OrderHeader(),
                PreviousAddresses = previousOrders 
            };

            //select Ids
            if (!string.IsNullOrEmpty(selectedIds))
            {
                List<int> idsList = selectedIds.Split(',').Select(int.Parse).ToList();
                shoppingCartVM.ListCart = shoppingCartVM.ListCart.Where(u => idsList.Contains(u.Id)).ToList();
            }

            shoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);

            foreach (var list in shoppingCartVM.ListCart)
            {
                list.price = SD.GetPriceBasedOnQuantity(list.Count, list.product.Price,
                    list.product.Price50, list.product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (list.price * list.Count);
            }
            shoppingCartVM.orderHeader.Name = shoppingCartVM.orderHeader.ApplicationUser.Name;
            shoppingCartVM.orderHeader.StreetAddress = shoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.orderHeader.City = shoppingCartVM.orderHeader.ApplicationUser.City;
            shoppingCartVM.orderHeader.State = shoppingCartVM.orderHeader.ApplicationUser.State;
            shoppingCartVM.orderHeader.PostalCode = shoppingCartVM.orderHeader.ApplicationUser.PostalCode;
            shoppingCartVM.orderHeader.PhoneNumber = shoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string selectedIds, string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();

            shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(
                sc => sc.ApplicationUserId == claims.Value,
                includeproperties: "product"
            ).ToList();

            if (!string.IsNullOrEmpty(selectedIds))
            {
                List<int> idsList = selectedIds.Split(',').Select(int.Parse).ToList();
                shoppingCartVM.ListCart = shoppingCartVM.ListCart.Where(u => idsList.Contains(u.Id)).ToList();
            }

            shoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            shoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusPending;
            shoppingCartVM.orderHeader.PaymentStatus = SD.OrderStatusPending;
            shoppingCartVM.orderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.orderHeader.ApplicationUserId = claims.Value;
            shoppingCartVM.orderHeader.OrderTotal = 0;

            _unitOfWork.orderHeader.Add(shoppingCartVM.orderHeader);
            _unitOfWork.save();

            foreach (var list in shoppingCartVM.ListCart)
            {
                list.price = SD.GetPriceBasedOnQuantity(list.Count, list.product.Price,
                    list.product.Price50, list.product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (list.price * list.Count);

                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = shoppingCartVM.orderHeader.Id,
                    ProductId = list.ProductId,
                    Price = list.price,
                    Count = list.Count,
                };
                _unitOfWork.orderDetail.Add(orderDetail);
            }
            _unitOfWork.save();

            _unitOfWork.shoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _unitOfWork.save();

            var remainingCartCount = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).Count();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, remainingCartCount);

            if (string.IsNullOrEmpty(stripeToken))
            {
                shoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                shoppingCartVM.orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                shoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                long amountInCents = Convert.ToInt64(shoppingCartVM.orderHeader.OrderTotal * 100);

                var paymentMethodOptions = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Token = stripeToken
                    }
                };
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = paymentMethodService.Create(paymentMethodOptions);

                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "usd",
                    Description = "OrderId: " + shoppingCartVM.orderHeader.Id.ToString(),
                    PaymentMethod = paymentMethod.Id,
                    Confirm = true,
                    ReturnUrl = "https://localhost"
                };

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent intent = paymentIntentService.Create(paymentIntentOptions);

                if (intent.Status.ToLower() == "succeeded")
                {
                    shoppingCartVM.orderHeader.TransactionId = intent.Id;
                    shoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
                    shoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    shoppingCartVM.orderHeader.OrderDate = DateTime.Now;
                }
                else
                {
                    shoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
            }

            _unitOfWork.save();

            return RedirectToAction("orderConfirmation", "Cart", new { id = shoppingCartVM.orderHeader.Id });
        }
        public IActionResult orderConfirmation(int id)
        {
            
            var orderHeader = _unitOfWork.orderHeader.FirstOrDefault(
                u => u.Id == id,
                includeproperties: "ApplicationUser"
            );

            var orderDetails = _unitOfWork.orderDetail.GetAll(
                u => u.OrderHeaderId == id,
                includeproperties: "Product"
            ).ToList();

            
            if (orderHeader != null)
            {
                OrderNotificationHelper.SendOrderNotifications(orderHeader, orderDetails, _emailSender, _config);
            }

            return View(id);
        }
    }
}

