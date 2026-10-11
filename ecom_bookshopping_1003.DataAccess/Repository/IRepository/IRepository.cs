using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ecom_bookshopping_1003.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T: class 
    {
        void Add(T entity);
        void Update(T entity);
        T Get(int id);

        IEnumerable<T> GetAll(Expression<Func<T,bool>> filter=null,
            Func<IQueryable<T>,IOrderedQueryable<T>> orderBy=null,
            string includeproperties=null);

        T FirstOrDefault(Expression<Func<T,bool>> filter=null,
            string includeproperties=null);

        void Remove(int id);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T>entities);
    }
}
