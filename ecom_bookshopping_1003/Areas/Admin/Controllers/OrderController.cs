using Microsoft.AspNetCore.Mvc;
using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;

namespace ecom_bookshopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;

        public OrderController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<OrderHeader> orderList;

            if (startDate.HasValue && endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);

                orderList = _unitOfWork.orderHeader.GetAll(
                    filter: o => o.OrderDate >= startDate.Value.Date && o.OrderDate < end,
                    includeproperties: "ApplicationUser"
                ).OrderByDescending(u => u.OrderDate);
            }
            else
            {
                orderList = _unitOfWork.orderHeader.GetAll(
                    includeproperties: "ApplicationUser"
                ).OrderByDescending(u => u.OrderDate);
            }

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(orderList);
        }

        
        public IActionResult Details(int id)
        {
            var orderHeader = _unitOfWork.orderHeader.FirstOrDefault(
                filter: u => u.Id == id,
                includeproperties: "ApplicationUser"
            );

            if (orderHeader == null)
            {
                return NotFound();
            }

            ViewBag.OrderDetails = _unitOfWork.orderDetail.GetAll(
                filter: u => u.OrderHeaderId == id,
                includeproperties: "Product"
            );

            return View(orderHeader);
        }
    }
}