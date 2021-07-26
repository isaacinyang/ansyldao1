using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ansyl.dao
{
    /// <summary>
    /// Base Transaction Interface
    /// </summary>
    public interface IBaseTransaction : IDisposable
    {
        bool SaveChanges();

        BaseRepository<T> Repository<T>() where T : DataObject;

        int Insert<T>(T entity) where T : DataObject;
        int Update<T>(T entity) where T : DataObject;
        int Delete<T>(T entity) where T : DataObject;
        int Delete<T>(int entityId) where T : DataObject;

        T Get<T>(int id) where T : DataObject;
        T Get<T>(int? id) where T : DataObject;
        T Get<T>(Expression<Func<T, bool>> expression) where T : DataObject;
        IList<T> List<T>(Expression<Func<T, bool>> expression = null, int limit = 0) where T : DataObject;
    }

    /// <summary>
    /// Base Transaction Abstract Class
    /// </summary>
    public abstract class BaseTransaction<TUnitOfWork> : IBaseTransaction
        where TUnitOfWork : BaseUnitOfWork, new()
    {
        protected BaseTransaction()
        {
            _uow = new TUnitOfWork();
            _uow.Begin();
        }

        TUnitOfWork _uow;

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (_uow?.Connection != null)
                //    SaveChanges();

                _uow?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        //public void Begin()
        //{
        //    //_uow.Begin();
        //}

        //public void Commit()
        //{
        //    _uow.Commit();
        //    Console.WriteLine("committed 1 - " + (_uow.Transaction != null));
        //}

        //public void Rollback()
        //{
        //    _uow.Rollback();
        //}

        public bool SaveChanges()
        {
            using (_uow.Connection)
            {
                try
                {
                    _uow.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SaveChanges Error: " + ex.Message);
                    _uow.Rollback();
                    return false;
                }
                finally
                {
                    _uow.Dispose();
                }
            }
        }

        public BaseRepository<T> Repository<T>() where T : DataObject
        {
            return _uow.Repository<T>();
        }

        public int Insert<T>(T entity) where T : DataObject
        {
            return _uow.Insert(entity);
        }

        public int Update<T>(T entity) where T : DataObject
        {
            return _uow.Update(entity);
        }

        public int Delete<T>(T entity) where T : DataObject
        {
            return _uow.Delete(entity);
        }

        public int Delete<T>(int entityId) where T : DataObject
        {
            return _uow.Delete<T>(entityId);
        }

        public T Get<T>(int id) where T : DataObject
        {
            return _uow.Get<T>(id);
        }

        public T Get<T>(int? id) where T : DataObject
        {
            return _uow.Get<T>(id);
        }

        public T Get<T>(Expression<Func<T, bool>> expression) where T : DataObject
        {
            return _uow.Get(expression);
        }

        public IList<T> List<T>(Expression<Func<T, bool>> expression = null, int limit = 0) where T : DataObject
        {
            return _uow.List(expression, limit);
        }
    }
}
