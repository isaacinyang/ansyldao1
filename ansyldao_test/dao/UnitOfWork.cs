using ansyl.dao;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;

namespace ansyldao_test.dao
{
    public sealed class UnitOfWork : BaseUnitOfWork
    {
        public UnitOfWork() : base(DaoFactory.Connection())
        {
        }
    }

    public sealed class OneTransaction : BaseTransaction<UnitOfWork>
    {
    }

    public sealed class DaoFactory
    {
        static DaoFactory()
        {
            var commandTimeOutSeconds = 120;
            var connectionTimeOutSeconds = 120;

            //connection parameters - change to what you'd like
            var dataSource = "localhost";
            var port = 40044;
            var username = "root";
            var password = "tp0506r1892";
            var database = "schoolapp";

            connString = new MySqlConnectionString(database, dataSource, username, password, port, connectionTimeOutSeconds, commandTimeOutSeconds);
            Console.WriteLine(connString);
        }

        private static string connString = null; //  PLACE YOUR CONNECTION STRING HERE

        public static IDbConnection Connection()
        {
            var connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
        }
    }

    public class MySqlConnectionString
    {
        public MySqlConnectionString(string database, string dataSource = null, string userId = null, string password = null, int? port = null, int connectionTimeOut = 600, int commandTimeOut = 600)
        {
            _map["Data Source"] = dataSource ?? "localhost";
            _map["User Id"] = userId ?? "root";
            _map["Password"] = password ?? "";
            _map["Port"] = port ?? 40044;
            _map["Database"] = database;
            _map["Connection Timeout"] = connectionTimeOut;
            _map["Default Command Timeout"] = commandTimeOut;

            _map["Use Compression"] = true;
            _map["Convert Zero Datetime"] = true;
            _map["Allow User Variables"] = true;

            _str = string.Join(";", _map.Select(p => $"{p.Key}={p.Value}"));
        }

        private readonly string _str = null;
        private readonly IDictionary<string, object> _map = new Dictionary<string, object>();

        public override string ToString()
        {
            return _str;
        }

        public static implicit operator string(MySqlConnectionString ts)
        {
            return ts?.ToString() ?? "";
        }
    }
}
