using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace ansyl.dao
{
    public abstract class BaseOneTaskOption2<TEntity> : IDisposable, IRepository<TEntity> where TEntity : DataObject
    {
        protected BaseOneTaskOption2(IUnitOfWork uow)
        {
            this.UnitOfWork = uow;
        }

        IUnitOfWork UnitOfWork { get; }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnitOfWork?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region MyRegion

        int Execute(Func<TEntity, int> action, TEntity entity)
        {
            try
            {
                UnitOfWork.Begin();
                var returnValue = action.Invoke(entity);
                UnitOfWork.Commit();
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UnitOfWork.Rollback();
                return -1;
            }
            finally
            {
                UnitOfWork.Connection?.Close();
                UnitOfWork.Connection?.Dispose();
                UnitOfWork.Dispose();
            }
        }

        public int Insert(TEntity entity) => Execute(UnitOfWork.Insert, entity);

        public int Update(TEntity entity) => Execute(UnitOfWork.Update, entity);

        public int Delete(TEntity entity) => Execute(UnitOfWork.Delete, entity);

        public int Delete(int entityId) => Execute(UnitOfWork.Delete, Get(entityId));
        //public int Delete

        public BaseRepository<TEntity> Repository() => UnitOfWork.Repository<TEntity>();

        public TEntity Get(int id)
        {
            return RunGetTask(() => Repository().Get(id));
        }

        public TEntity Get(int? id)
        {
            return RunGetTask(() => Repository().Get(id));
        }

        public TEntity Get(Expression<Func<TEntity, bool>> expression)
        {
            return RunGetTask(() => Repository().Get(expression));
        }

        public IList<TEntity> List(Expression<Func<TEntity, bool>> predicate = null, int limit = 0)
        {
            return RunGetTask(() => Repository().List(predicate));
        }

        //void CleanUp()
        //{
        //    UnitOfWork.Connection?.Close();
        //    UnitOfWork.Connection?.Dispose();
        //    UnitOfWork.Dispose();
        //    Console.WriteLine("cleaning up");
        //}

        T RunGetTask<T>(Func<T> action)
        {
            try
            {
                return action.Invoke();
            }
            finally
            {
                UnitOfWork?.Dispose();
            }
        }

        #endregion
    }
}
