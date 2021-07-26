using System;
using System.Data;
using ansyl.dao;
using testapp.schoolapp;

namespace ansyldao_app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using var ot = new OneTransaction();

            using var ot = new OneTransaction();

            var schoolTermIds = ot.List<SchoolTerm>(e => e.SchoolId == schoolId)
                                  .Select(i => i.TermId)
                //.ToArray();
                ;

            //var ids = new List<byte>();

            //ids.AddRange(schoolTermIds);

            //var ids = schoolTermIds;

            var items = ot.List<Term>(e => e.TermId.In(schoolTermIds));

            var years = items.Select(i => i.YearName).ToList();

            Console.WriteLine(years.Count);

            return years;

        }
    }
}

namespace testapp.schoolapp
{
    public class Term : DataObject
    {
        public byte     TermId     { get; set; }
        public byte     TermNo     { get; set; }
        public string   TermName   { get; set; }
        public string   YearName   { get; set; }
        public bool     IsActive   { get; set; }
        public DateTime InsertDate { get; set; }
    }

    public class SchoolTerm : DataObject
    {
        public int       SchoolTermId { get; set; }
        public short     SchoolId     { get; set; }
        public byte      TermId       { get; set; }
        public DateTime? FirstDate    { get; set; }
        public DateTime? FinalDate    { get; set; }
        public bool      IsActive     { get; set; }
        public DateTime  InsertDate   { get; set; }
    }
}

namespace testapp.dao
{
    public sealed class OneTransaction : BaseTransaction<UnitOfWork>
    {
    }

    public sealed class UnitOfWork : BaseUnitOfWork
    {
        public UnitOfWork() : base(DaoFactory.Connection())
        {
        }
    }

    public sealed class DaoFactory
    {
        private static bool IsLive = false;

        public static DataServer Server => IsLive ? LiveDataServer.Instance : DemoDataServer.Instance;
        public static IDbConnection Connection() => Server.GetDefaultConnection();
    }

    public class DemoDataServer : DataServer
    {
        private static readonly Lazy<DemoDataServer> Lazy = new(() => new DemoDataServer());

        public static DemoDataServer Instance => Lazy.Value;

        DemoDataServer()
        {
            const string dataSource = "localhost";
            DefaultConnString = CreateConnectionString(dataSource, 40044, "root", "tp0506r1892", DatabaseName);
            OneTaskConnString = CreateConnectionString(dataSource, 40044, "root", "tp0506r1892", DatabaseName);
        }
    }

    public class LiveDataServer : DataServer
    {
        private static readonly Lazy<LiveDataServer> Lazy = new(() => new LiveDataServer());

        public static LiveDataServer Instance => Lazy.Value;

        LiveDataServer()
        {
            const string dataSource = "db.oaunetque.com";
            DefaultConnString = CreateConnectionString(dataSource, 3306, "liveuser", "m3I!Bxy8tNDy", DatabaseName);
        }
    }

    public abstract class DataServer
    {
        protected static string DatabaseName => "demoapp";

        public string DefaultConnString { get; set; }
        //public string OneTaskConnString { get; set; }

        public IDbConnection GetDefaultConnection() => GetConnection(DefaultConnString);
        //public IDbConnection GetOneTaskConnection() => GetConnection(OneTaskConnString);
        static IDbConnection GetConnection(string connString)
        {
            var connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
        }

        protected static string CreateConnectionString(string dataSource, int port, string username, string password, string database)
        {
            return new MySqlConnectionString2
            {
                DataSource = dataSource,
                Database = database,
                UserId = username,
                Password = password,
                Port = port
            }.ToString();
        }
    }
}