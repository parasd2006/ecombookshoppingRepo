using ecom_bookshopping_1003.Data.DataAccess;
using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1003.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWorkRepository _unitofwork;
        public UserController(ApplicationDbContext context, IUnitOfWorkRepository unitOfWork)
        {
            _context = context;
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        public IActionResult GetAll()
        {
            var userList = _context.ApplicationUsers.ToList();// asp net user
            var roleList = _context.Roles.ToList();//asp net roles
            var userRoles = _context.UserRoles.ToList();// aspnet userroles

            foreach(var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roleList.FirstOrDefault(r => r.Id == roleId).Name;
                if(user.CompanyId==null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
                if(user.CompanyId!=null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitofwork.company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            //remove admin role user
            var adminuser = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userList.Remove(adminuser);
            return Json(new {data=userList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool isLocked = false;
            var userIndb = _unitofwork.ApplicationUser.FirstOrDefault(u => u.Id == id);
            if(userIndb==null)
            {
                return Json(new
                {
                    success = false,
                    message = "Something went wrong whilw lock and unlock user"
                });
            }
            if(userIndb!=null && userIndb.LockoutEnd> DateTime.Now)
            {
                userIndb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }else
            {
                userIndb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, 
            message=isLocked==true?
            "User Successfully locked":" user successfully unlocked"});
             
        }
        #endregion
    }
}
