using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ansyl.dao;

namespace ansyl.dao
{
    public class BaseOneTask<TUnitOfWork, TEntity> where TUnitOfWork : BaseUnitOfWork, new()
                                                   where TEntity : DataObject
    {
        public static TEntity Get(int? id)
        {
            if (id == null) return default(TEntity);

            using (var uow = new TUnitOfWork())
            {
                return uow.Repository<TEntity>().Get(id);
            }
        }

        public static TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            using (var uow = new TUnitOfWork())
            {
                return uow.Repository<TEntity>().Get(predicate);
            }
        }

        public static IList<TEntity> List(Expression<Func<TEntity, bool>> predicate = null, int limit = 0)
        {
            using (var uow = new TUnitOfWork())
            {
                return uow.Repository<TEntity>().List(predicate);
            }
        }

        public static int Insert(TEntity entity) => Execute(ActionType.InsertAction, entity);

        public static int Update(TEntity entity) => Execute(ActionType.UpdateAction, entity);

        public static int Delete(TEntity entity) => Execute(ActionType.DeleteAction, entity);
        public static int Delete(int entityId) => Execute(ActionType.DeleteAction,   Get(entityId));

        enum ActionType
        {
            InsertAction = 1,
            UpdateAction = 2,
            DeleteAction = 3
        }

        private static int Execute(ActionType actionType, TEntity entity)
        {
            using (var uow = new TUnitOfWork())
            {
                Func<TEntity, int> action = actionType switch
                                            {
                                                ActionType.InsertAction => uow.Insert,
                                                ActionType.UpdateAction => uow.Update,
                                                ActionType.DeleteAction => uow.Delete,
                                                _                       => null
                                            };

                if (action == null)
                    return -1;

                using (uow.Connection)
                {
                    try
                    {
                        uow.Begin();
                        var returnValue = action.Invoke(entity);
                        uow.Commit();
                        return returnValue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        uow.Rollback();
                        return -1;
                    }
                    finally
                    {
                        uow.Connection?.Close();
                        uow.Connection?.Dispose();
                        uow.Dispose();
                    }
                }
            }
        }
    }
}