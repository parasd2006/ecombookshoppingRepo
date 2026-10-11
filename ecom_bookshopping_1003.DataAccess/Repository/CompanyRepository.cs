using ecom_bookshopping_1003.Data.DataAccess;
using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1003.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
