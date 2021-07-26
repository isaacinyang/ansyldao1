using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ansyl.dao
{
    /// <summary>
    /// Base Repository Interface
    /// </summary>
    public interface IRepository<T> where T : DataObject
    {
        T Get(int id);
        T Get(int? id);
        T Get(Expression<Func<T, bool>> predicate);
        IList<T> List(Expression<Func<T, bool>> predicate = null, int limit = 0);
        int Insert(T item);
        int Update(T item);
        int Delete(T item);
        int Delete(int id);
    }

    /// <summary>
    /// Base Unit of Work Interface
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        //Guid Id { get; }
        IDbConnection  Connection  { get; }
        IDbTransaction Transaction { get; }

        //  transaction management
        void Begin();
        void Commit();
        void Rollback();

        //  repository management
        BaseRepository<T> Repository<T>() where T : DataObject;

        T Get<T>(int id) where T : DataObject;
        T Get<T>(int? id) where T : DataObject;
        T Get<T>(Expression<Func<T, bool>> expression) where T : DataObject;
        IList<T> List<T>(Expression<Func<T, bool>> expression = null, int limit = 0) where T : DataObject;

        int Insert<T>(T entity) where T : DataObject;
        int Update<T>(T entity) where T : DataObject;
        int Delete<T>(T entity) where T : DataObject;
    }

    public interface IBaseRepository
    {
    }

    /// <summary>
    /// Base Unit of Work Interface
    /// </summary>
    public interface IBaseUnitOfWork : IDisposable
    {
        void Begin();
        void Commit();
        void Rollback();

        BaseRepository<T> Repository<T>() where T : DataObject;

        int Insert<T>(T entity) where T : DataObject;
        int Update<T>(T entity) where T : DataObject;
        int Delete<T>(T entity) where T : DataObject;
        int Delete<T>(int entityId) where T : DataObject;

        T Get<T>(int id) where T : DataObject;
        T Get<T>(Expression<Func<T, bool>> expression) where T : DataObject;
        IList<T> List<T>(Expression<Func<T, bool>> expression = null, int limit = 0) where T : DataObject;
    }
}