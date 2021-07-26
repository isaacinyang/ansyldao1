#if false
using System;
using System.Collections.Generic;
using System.Linq;

namespace ansyl_dao
{
    public static class DOE
    {
        #region ConnString
        public static T GetOne<T>(ConnString connString, int id) where T : DataObject
        {
            var parameter = new DbParameter(typeof(T).Name + "Id", id);
            var sql = SqlStatement.GetSelectSQL<T>();
            return DbX.ToEntities<T>(connString, sql, parameter).SingleOrDefault();
        }

        private static int ExecuteNonQuery(ConnString connString, string sql, params DbParameter[] parameters)
        {
            try
            {
                return DbX.ExecuteNonQuery(connString, sql, parameters);
            }
            catch (Exception ex)
            {
                DataObjectAudit.Insert(ex, sql, parameters);
                return -1;
            }
        }

        private static int ExecuteScalar(ConnString connString, string sql, params DbParameter[] parameters)
        {
            try
            {
                return DbX.ExecuteScalar<int>(connString, sql, parameters);
            }
            catch (Exception ex)
            {
                var sw = new StringWriter();

                sw.WriteLine(ex.ToString());

                var table = DbX.ToDataTable(connString, sql, parameters);

                sw.WriteLine("{0} records", table.Rows.Count);

                foreach (DataColumn column in table.Columns)
                {
                    sw.WriteLine("{0} => {1} => {2}", column.ColumnName, column.DataType, column.AllowDBNull ? "(null)" : "");
                }

                DataObjectAudit.Insert(new Exception(sw.ToString()), sql);
                return 0;
            }
        }
        #endregion

        #region mysql-db-core
        private static int ExecuteNonQuery(string sql, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(SqlX.ConnString, sql, parameters);
        }

        private static int ExecuteScalar(string sql, params DbParameter[] parameters)
        {
            return ExecuteScalar(SqlX.ConnString, sql, parameters);
        }
        #endregion

        #region Nullable Id
        private static T GetOne<T>(this int id) where T : DataObject
        {
            return GetOne<T>(SqlX.ConnString, id);
        }

        public static T Get<T>(this byte id) where T : DataObject
        {
            return GetOne<T>(id);
        }

        public static T Get<T>(this short id) where T : DataObject
        {
            return GetOne<T>(id);
        }

        public static T Get<T>(this int id) where T : DataObject
        {
            return GetOne<T>(id);
        }

        //private static T GetOne<T>(this int? id) where T : DataObject
        //{
        //    return id == null ? null : GetOne<T>(id ?? 0);
        //}

        public static T Get<T>(this byte? id) where T : DataObject
        {
            return GetOne<T>(id ?? 0);
        }

        public static T Get<T>(this short? id) where T : DataObject
        {
            return GetOne<T>(id ?? 0);
        }

        public static T Get<T>(this int? id) where T : DataObject
        {
            return GetOne<T>(id ?? 0);
        }
        #endregion

        private const StringComparison IgnoreCase = StringComparison.CurrentCultureIgnoreCase;

        internal static string GetTableName(this Type tt)
        {
            var database = tt.Namespace.Split('.').Last();
            return "`{0}`.`{1}`".Fmt(database, tt.Name).ToLower();
        }

        public static string GetMySqlTableName<T>()
            => GetTableName(typeof(T));

        private static DbParameter GetParameterPk<T>(T t)
        {
            var type = typeof(T);

            return (from pi in type.GetProperties()
                    where pi.Name.Equals(type.Name + "Id", IgnoreCase)
                    select new DbParameter(pi.Name, pi.GetValue(t, null))).SingleOrDefault();
        }

        public static DbParameter[] GetParameters<T>(object o) where T : DataObject
        {
            if (o == null)
                return new DbParameter[0];

            return (from pi in o.GetType().GetProperties()
                    let fieldType = SqlField.GetFieldType<T>(SqlX.ConnString, pi.Name)
                    let value = pi.GetValue(o, null)
                    let fieldValue = DataConverter.Read(value, pi.PropertyType, fieldType)
                    select new DbParameter(pi.Name, fieldValue)).ToArray();
        }

        public static DbParameter[] GetParameters<T>(T t, object o) where T : DataObject
        {
            if (o == null)
                return GetParameters<T>(t);

            var dic = GetParameters<T>(o).ToDictionary(i => i.Key, i => i);
            var pk = GetParameterPk(t);
            dic[pk.Key] = pk;

            return (from p in dic
                    select p.Value).ToArray();

            //return dic.Select(i => i.Value).ToArray();
        }

        #region Update Region
        public static int Update<T>(this T t, object o = null) where T : DataObject
        {
            DbParameter[] parameters;

            if (o == null)
            {
                parameters = GetParameters<T>(t);
            }
            else
            {
                var pk = GetParameterPk(t);
                var dic = GetParameters<T>(o).ToDictionary(i => i.Key, i => i);

                var oldKey = dic.Where(p => p.Key.Equals(pk.Key, IgnoreCase))
                                .Select(i => i.Key)
                                .SingleOrDefault();

                if (oldKey != null)
                    dic.Remove(oldKey);

                foreach (var p in dic)
                {
                    if (p.Key.Equals(pk.Key, IgnoreCase))
                        dic.Remove(p.Key);
                }
                dic[pk.Key] = pk;
                parameters = dic.Select(i => i.Value).ToArray();
            }

            var sql = SqlStatement.GetUpdateSQL<T>(o);
            return Update<T>(sql, parameters);
        }

        public static int Update<T>(object o) where T : DataObject
        {
            var parameters = GetParameters<T>(o);
            var sql = SqlStatement.GetUpdateSQL<T>(o);
            return Update<T>(sql, parameters);
        }

        private static int Update<T>(string sql, DbParameter[] parameters)
        {
            var rowCount = ExecuteNonQuery(sql, parameters);

            //  no row affected
            if (rowCount == 0) return -1;

            var pk = parameters.SingleOrDefault(i => i.Key == typeof(T).Name + "Id");

            //  no primary key found
            if (pk == null) return -2;

            //  pk value is NULL
            if (pk.Value == DBNull.Value) return -3;
            if (pk.Value == null) return -4;

            var givenId = Convert.ToInt32(pk.Value);

            //  pk value was already given
            if (givenId != 0) return givenId;

            //  return the last insert id
            return -100;
            return ExecuteScalar("SELECT LAST_INSERT_ID()");
        }
        #endregion

        #region Insert Region
        public static int Insert<T>(this T t, object o = null) where T : DataObject
        {
            var parameters = GetParameters<T>(o ?? t);
            var sql = SqlStatement.GetInsertSQL<T>(o);
            return Insert<T>(sql, parameters);
        }

        public static int Insert<T>(object o) where T : DataObject
        {
            var parameters = GetParameters<T>(o);
            var sql = SqlStatement.GetInsertSQL<T>(o);
            return Insert<T>(sql, parameters);
            //return ExecuteScalar(sql, parameters);
        }

        private static int Insert<T>(string sql, DbParameter[] parameters)
        {
            var rowCount = MySqlHelper.ExecuteNonQuery(SqlX.ConnString, sql, DbX.GetParameters(parameters));
            var insertId = ExecuteScalar("SELECT LAST_INSERT_ID()");

            //Console.WriteLine($"{rowCount} rows; {insertId} ID inserted");

            var pk = parameters.SingleOrDefault(i => i.Key == typeof(T).Name + "Id");

            //  no primary key found
            //if (pk == null) return -2;

            if (pk != null)
            {
                //  pk value is NULL
                if (pk.Value == null || pk.Value == DBNull.Value) return -3;

                //  pk value was already given
                var providedId = Convert.ToInt32(pk.Value);
                if (providedId != 0) return providedId;
            }

            return rowCount > 0 ? insertId : -1;
        }

        //private static int Insert1<T>(string sql, DbParameter[] parameters)
        //{
        //    var rowCount = ExecuteNonQuery(sql, parameters);
        //    return ExecuteScalar("SELECT LAST_INSERT_ID()");

        //    //  no row affected
        //    if (rowCount == 0) return -1;

        //    var pk = parameters.SingleOrDefault(i => i.Key == typeof (T).Name + "Id");

        //    //  no primary key found
        //    //if (pk == null) return -2;

        //    if (pk != null)
        //    {
        //        //  pk value is NULL
        //        if (pk.Value == DBNull.Value) return -3;
        //        if (pk.Value == null) return -4;

        //        var givenId = Convert.ToInt32(pk.Value);

        //        //  pk value was already given
        //        if (givenId != 0) return givenId;
        //    }

        //    //  return the last insert id
        //    return ExecuteScalar("SELECT LAST_INSERT_ID()");
        //}
        #endregion

        public static int Delete<T>(this T t) where T : DataObject
        {
            var parameters = GetParameters<T>(t);
            var sql = SqlStatement.GetDeleteSQL<T>();
            return ExecuteNonQuery(sql, parameters);
        }

        public static int Delete<T>(this int id) where T : DataObject
        {
            var parameter = new DbParameter(typeof(T).Name + "Id", id);
            var sql = SqlStatement.GetDeleteSQL<T>();
            return ExecuteNonQuery(sql, parameter);
        }

        public static string GetListSql<T>(object o = null, int limit = 1000) where T : DataObject
        {
            var tt = typeof(T);
            var tableName = DOE.GetTableName(tt);

            var sw = new StringWriter();

            sw.WriteLine("SELECT * ");
            sw.WriteLine("FROM {0}", tableName);

            if (o != null)
            {
                var conditions = (from p in o.GetType().GetProperties()
                                  select string.Format("`{0}`=@{0}", p.Name)).ToArray();

                if (conditions.Any())
                    sw.WriteLine("WHERE {0}", string.Join(" AND ", conditions));
            }

            sw.WriteLine("LIMIT {0}", limit);

            return sw.ToString();
        }

        public static IList<T> GetListMine<T>(object o = null, int limit = 1000) where T : DataObject
        {
            var sql = GetListSql<T>(o, limit);
            var parameters = GetParameters<T>(o);
            return DbX.ToEntities<T>(SqlX.ConnString, sql, parameters);
            //return sql.ToList<T>(parameters);
        }

        public static IList<T> GetListDapper<T>(object o = null, int limit = 10000) where T : DataObject
        {
            var sql = GetListSql<T>(o, limit);
            var parameters = GetParameters<T>(o);

            var dp = new DynamicParameters();

            foreach (var p in parameters)
            {
                dp.Add(p.Key, p.Value);
            }

            using (var conn = SqlX.ConnString.NewConnection())
            {
                return conn.Query<T>(sql, dp).ToList();
            }
        }

        public static IList<T> GetEntities<T>(object o = null, int limit = 10000) where T : DataObject
        {
            var sql = GetListSql<T>(o, limit);
            return DbX.ToEntities<T>(SqlX.ConnString, sql, o);
        }

        [Obsolete("Use GetEntities")]
        public static IList<T> GetList<T>(object o = null, int limit = 10000) where T : DataObject
        {
            return GetEntities<T>(o, limit);

            var sql = GetListSql<T>(o, limit);
            return DbX.ToEntities<T>(SqlX.ConnString, sql, o);

            //return sql.ToEntities<T>(o);

            //var parameters = GetParameters<T>(o);

            //var dp = new DynamicParameters();

            //foreach (var p in parameters)
            //{
            //    dp.Add(p.Key, p.Value);
            //}

            //return SqlMapper.Query<T>(SqlX.Connection, sql, dp).ToList();

            return GetListDapper<T>(o, limit);
        }

        public static T Get<T, U>(U o)
            where T : DataObject
            where U : new()
        {
            var sql = GetListSql<T>(o, 10);
            return DbX.ToEntities<T>(SqlX.ConnString, sql, o).SingleOrDefault();
            return GetList<T>(o, 10).SingleOrDefault();
        }

        public static T GetSingle<T>(object o) where T : DataObject
        {
            var sql = GetListSql<T>(o, 10);
            return DbX.ToEntities<T>(SqlX.ConnString, sql, o).SingleOrDefault();
            return GetList<T>(o, 10).SingleOrDefault();
        }
    }
}
#endif
