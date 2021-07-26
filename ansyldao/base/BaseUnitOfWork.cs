using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;


namespace ansyl.dao
{
    /// <summary>
    /// Base Unit of Work Class
    /// </summary>
    public abstract class BaseUnitOfWork : IUnitOfWork, IBaseUnitOfWork
    {
        private IDbConnection _connection = null;

        public IDbConnection Connection => Transaction?.Connection ?? _connection;
        public IDbTransaction Transaction { get; private set; }

        protected BaseUnitOfWork(IDbConnection connection)
        {
            Debug.WriteLine("unit of work: started");
            _connection = connection;
        }

        public virtual void Dispose()
        {
            if (Transaction != null)
                Commit();

            Debug.WriteLine("unit of work: dispose");

            if (Transaction != null)
            {
                Transaction?.Dispose();
                Transaction = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }

        #region Transaction

        public void Begin()
        {
            if (Transaction != null) return;

            Debug.WriteLine("transaction: started");
            Transaction = _connection.BeginTransaction();
            //Connection = Transaction.Connection;
        }

        public void Commit()
        {
            if (Transaction == null) return;

            Debug.WriteLine("transaction: commit");
            Transaction.Commit();
            Transaction = null;

            //Connection = _connection;
        }

        public void Rollback()
        {
            if (Transaction == null) return;

            Debug.WriteLine("transaction: rollback");
            Transaction.Rollback();
            Transaction = null;

            //Connection = _connection;
        }

        #endregion

        private readonly IDictionary<string, IBaseRepository> _repositories = new Dictionary<string, IBaseRepository>();

        #region Repositories

        protected BaseRepository<T> GetRepository<T>() where T : DataObject
        {
            var typeName = typeof(T).FullName;

            if (_repositories.ContainsKey(typeName) == false)
            {
                _repositories[typeName] = new BaseRepository<T>(this);
                Debug.WriteLine($"added Repo: {_repositories.Count,3} => " + typeName);
            }

            return _repositories[typeName] as BaseRepository<T>;
        }

        public BaseRepository<T> Repository<T>() where T : DataObject => GetRepository<T>();

        public int Insert<T>(T entity) where T : DataObject => Repository<T>().Insert(entity);
        public int Update<T>(T entity) where T : DataObject => Repository<T>().Update(entity);
        public int Delete<T>(T entity) where T : DataObject => Repository<T>().Delete(entity);
        public int Delete<T>(int entityId) where T : DataObject => Repository<T>().Delete(entityId);

        public T Get<T>(int id) where T : DataObject => Repository<T>().Get(id);
        public T Get<T>(int? id) where T : DataObject => Repository<T>().Get(id);
        public T Get<T>(Expression<Func<T, bool>> expression) where T : DataObject => Repository<T>().Get(expression);
        public IList<T> List<T>(Expression<Func<T, bool>> expression = null, int limit = 0) where T : DataObject => Repository<T>().List(expression, limit);

        #endregion
    }
}
