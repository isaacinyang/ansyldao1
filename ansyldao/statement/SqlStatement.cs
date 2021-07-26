#if false
using System.IO;
using System.Linq;

namespace ansyl.dao
{
    public class SqlStatement<T> where T : DataObject
    {
        SqlStatement()
        {
            _statement = MySQLStatement<T>.Instance;
        }

        public static SqlStatement<T> Get() => new SqlStatement<T>();

        private readonly BaseStatement _statement;

        public string SelectOne => _statement.SelectOne;
        public string SelectAll => _statement.SelectAll;
        public string InsertOne => _statement.InsertOne;
        public string UpdateOne => _statement.UpdateOne;
        public string DeleteOne => _statement.DeleteOne;

        public string GetInsertSQL(object o = null)
            => o == null ? InsertOne : _statement.GetInsertSQL(o);

        public string GetUpdateSQL(object o = null)
            => o == null ? UpdateOne : _statement.GetUpdateSQL(o);

        public string GetListSql(object o = null, int limit = 1000)
        {
            var tt = typeof(T);
            var tableName = DOE.GetMySqlTableName<T>();

            var sw = new StringWriter();

            sw.WriteLine("SELECT * FROM {0}", tableName);

            if (o != null)
            {
                var conditions = (from p in o.GetType().GetProperties()
                                  select string.Format((string)"`{0}`=@{0}", (object)p.Name)).ToArray();

                if (conditions.Any())
                    sw.WriteLine("WHERE {0}", string.Join(" AND ", conditions));
            }

            sw.WriteLine("LIMIT {0}", limit);

            return sw.ToString();
        }
    }
} 
#endif
