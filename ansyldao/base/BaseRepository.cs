using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ansyl.dao;
using ansyl.dao.expressions;
using Dapper;

namespace ansyl.dao
{
    /// <summary>
    /// Base Repository Class
    /// </summary>
    public class BaseRepository<T> : IRepository<T>, IBaseRepository where T : DataObject
    {
        public BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _statement = MySQLStatement<T>.Instance;
        }

        private readonly BaseStatement _statement;
        private readonly IUnitOfWork _unitOfWork;

        private IDbConnection Connection => _unitOfWork.Connection;
        private IDbTransaction Transaction => _unitOfWork.Transaction;

        public T Get(int id)
        {
            if (id == 0) return null;

            var primaryKeyId = "@" + typeof(T).Name + "Id";

            var parameter = new DynamicParameters();
            parameter.Add(primaryKeyId, id);

            return Connection.Query<T>(_statement.SelectOne, parameter, transaction: Transaction).SingleOrDefault();
        }

        public T Get(int? id)
        {
            return Get(id ?? 0);
        }

        public T Get(Expression<Func<T, bool>> expression)
        {
            return List(expression, 2).SingleOrDefault();
        }

        public IList<T> List()
        {
            var selectSQL = _statement.SelectAll;
            return Connection.Query<T>(selectSQL, transaction: Transaction).ToList();
        }

        //public IList<T> List(Expression<Func<T, bool>> predicate, int limit = 0)
        //{
        //    //  my parameter collection
        //    var collection = new Dictionary<string, object>();

        //    //  needed by Dapper
        //    var selectSQL = string.Empty;
        //    var parameters = new DynamicParameters();

        //    if (predicate == null)
        //    {
        //        selectSQL = _statement.SelectAll;
        //    }
        //    else
        //    {
        //        var exSql = new ExpressionSql<T>(predicate);

        //        if (string.IsNullOrWhiteSpace(exSql.SqlSelect))
        //            return new List<T>();

        //        collection = exSql.Data;
        //        selectSQL = exSql.SqlSelect;
        //        //parameters = new DynamicParameters(exSql.Data);

        //        foreach (var dp in exSql.Data)
        //        {
        //            object value = dp.Value;

        //            //  Dapper gives errors for byte array i.e. e.StudentId IN @p1 where @p1 is byte[] ... so I map to a short[]
        //            if (dp.Value is byte[] bytes)
        //            {
        //                value = bytes.Select(Convert.ToInt16).ToList();
        //            }
        //            parameters.Add(dp.Key, value);
        //        }
        //    }

        //    //  limits
        //    if (limit > 0)
        //        selectSQL += " LIMIT " + limit;

        //    if (Dns.GetHostName() == "soft")
        //    {
        //        Console.WriteLine(selectSQL);
        //        foreach (var dp in collection)
        //        {
        //            Console.WriteLine($"{dp.Key,10} = {dp.Value}");
        //        }
        //    }

        //    return Connection.Query<T>(selectSQL, parameters, transaction: Transaction).ToList();
        //}

        public IList<T> List(Expression<Func<T, bool>> predicate, int limit = 0)
        {
            var sql = string.Empty;
            var parameters = new DynamicParameters();

            if (predicate == null)
            {
                sql = _statement.SelectAll;
                //return Connection.Query<T>(selectSQL, transaction: Transaction).ToList();
            }

            if (predicate != null)
            {
                var exSql = new ExpressionSql<T>(predicate);

                if (string.IsNullOrWhiteSpace(exSql.SqlSelect))
                    return new List<T>();

                sql = exSql.SqlSelect;

                var collection = new Dictionary<string, object>();

                foreach (var dp in exSql.Data)
                {
                    object value = dp.Value; 
                    
                    ////  Dapper gives errors for byte array i.e. e.StudentId IN @p1 where @p1 is byte[] ... so I map to a short[]
                    //if (value is IEnumerable<byte> b)
                    //{
                    //    value = b.Select(Convert.ToInt16).ToList();
                    //}

                    parameters.Add(dp.Key, value);
                    collection.Add(dp.Key, value);
                }

                //if (Dns.GetHostName() == "soft")
                //{
                //    var sw = new StringWriter();
                //    sw.Write(selectSQL + "; ");

                //    foreach (var dp in collection)
                //    {
                //        sw.Write($"{dp.Key} = {dp.Value}; ");
                //    }

                //    sw.WriteLine();
                //    Console.WriteLine(sw.ToString());
                //}
            }

            //  limits
            if (limit > 0)
                sql += " LIMIT " + limit;

            return Connection.Query<T>(sql, parameters, transaction: Transaction).ToList();
        }

        public int Update(T item)
        {
            return item == null ? 0 : Connection.Execute(_statement.UpdateOne, item, transaction: Transaction);
        }

        public int Delete(T item)
        {
            return item == null ? 0 : Connection.Execute(_statement.DeleteOne, item, transaction: Transaction);
        }

        public int Delete(int entityId)
        {
            return entityId <= 0 ? 0 : Connection.Execute(_statement.DeleteOneId, new { entityId }, transaction: Transaction);
        }

        public int Insert(T item)
        {
            return item == null ? 0 : Connection.ExecuteScalar<int>(_statement.InsertOne, item, transaction: Transaction);
        }
    }
}
