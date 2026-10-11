using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1003.DataAccess.Repository.IRepository
{
    public interface IUnitOfWorkRepository
    {
        ICoverTypeRepository coverType { get; }
        ICategoryRepository category { get; }
        IProductRepository product { get; }
        ICompanyRepository company { get; }
        IApplicationRepository ApplicationUser { get; }
        IShoppingCartRepository shoppingCart { get; }
        IOrderDetailRepository orderDetail { get; }
        IOrderHeaderRepository orderHeader { get; }
        
        void save();
    }
}
