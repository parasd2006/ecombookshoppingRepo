using ecom_bookshopping_1003.Data.DataAccess;
using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1003.DataAccess.Repository
{
    public class UnitOfRepository : IUnitOfWorkRepository
    {
        private readonly ApplicationDbContext _context;
        public UnitOfRepository(ApplicationDbContext context)
        {
            _context = context;
            coverType = new CoverTypeRepository(context);
            category = new CategoryRepository(context);
            product = new ProductRepository(context);
            company = new CompanyRepository(context);
            ApplicationUser = new ApplicationRepository(context);
            shoppingCart = new ShoppingCartRepository(context);
            orderDetail = new OrderDetailRepository(context);
            orderHeader = new OrderHeaderRepository(context);
          
        }
        public ICoverTypeRepository coverType { private set; get; }

       public  ICategoryRepository category { private set; get; }
       public  IProductRepository product { private set; get; }

        public ICompanyRepository company { private set; get; }

        public IApplicationRepository ApplicationUser { private set; get; }

        public IShoppingCartRepository shoppingCart { private set; get; }

        public IOrderDetailRepository orderDetail { private set; get; }

        public IOrderHeaderRepository orderHeader { private set; get; }

        public void save()
        {
            _context.SaveChanges();
        }

       
    }
}
