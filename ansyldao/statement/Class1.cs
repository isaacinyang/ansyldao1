using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ansyl.dao
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DatabaseType
    {
        MySQL = 1,
        SQLServer = 2,
        SQLite = 3
    }

    /// <summary>
    /// Base Abstract Class for SQL Statement classes
    /// </summary>
    public abstract class BaseStatement
    {
        protected BaseStatement(Type type)
        {
        }
        public string TableName { get; protected set; }

        public string DeleteOne { get; protected set; }
        public string DeleteOneId { get; protected set; }
        public string InsertOne { get; protected set; }
        public string UpdateOne { get; protected set; }
        public string SelectOne { get; protected set; }
        public string SelectAll { get; protected set; }
        public abstract string GetInsertSQL(object o);
        public abstract string GetUpdateSQL(object o);
    }

    /// <summary>
    /// Implementation of BaseStatement for MySQL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MySQLStatement<T> : BaseStatement
    {
        public static BaseStatement Instance { get; } = new MySQLStatement<T>();

        private readonly string _className;
        private readonly string _tableName;
        private readonly string[] _properties;
        private readonly string[] _otherProperties;

        public string PkProperty { get; }

        private MySQLStatement() : base(typeof(T))
        {
            var type     = typeof(T);
            var database = type.Namespace?.Split('.').Last();
            _className  = type.Name;
            _tableName  = $"`{database}`.`{type.Name}`".ToLower();
            _properties = type.GetProperties().Select(i => i.Name).ToArray();

            var ignoreCase = StringComparison.CurrentCultureIgnoreCase;

            _otherProperties = (from p in _properties
                                where p.Equals(type.Name + "Id", ignoreCase) == false
                                select p).ToArray();

            PkProperty = _properties.Single(pi => pi.Equals(type.Name + "Id", ignoreCase));

            TableName   = _tableName;
            SelectAll   = string.Format("SELECT * FROM {0} ORDER BY 1",            _tableName, _className);
            SelectOne   = string.Format("SELECT * FROM {0} WHERE `{1}Id`=@{1}Id",  _tableName, _className);
            DeleteOne   = string.Format("DELETE FROM {0} WHERE `{1}Id`=@{1}Id",    _tableName, _className);
            DeleteOneId = string.Format("DELETE FROM {0} WHERE `{1}Id`=@EntityId", _tableName, _className);
            UpdateOne   = GetUpdateSQL(_otherProperties);
            InsertOne   = GetInsertSQL(_properties);
        }

        private string GetInsertSQL(string[] properties)
        {
            var fields = properties.Select(i => "`" + i + "`").ToArray();
            var values = properties.Select(i => "@" + i).ToArray();

            var joinFields = string.Join(", ", fields);
            var joinValues = string.Join(", ", values);

            return $"INSERT INTO {_tableName}({joinFields}) VALUES({joinValues}); SELECT LAST_INSERT_ID();";
        }

        private string GetUpdateSQL(string[] properties)
        {
            var parameters = string.Join(", ", properties.Select(p => $"`{p}`=@{p}").ToArray());
            var primaryKey = $"{_className}ID";

            return $"UPDATE {_tableName} SET {parameters} WHERE `{primaryKey}`=@{primaryKey}";
        }

        public override string GetUpdateSQL(object o)
            => GetUpdateSQL(o == null ? _properties : o.GetType().GetProperties().Select(i => i.Name).ToArray());

        public override string GetInsertSQL(object o)
            => GetInsertSQL(o == null ? _properties : o.GetType().GetProperties().Select(i => i.Name).ToArray());
    }

    /// <summary>
    /// Implementation of BaseStatement for SQLite (future)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLiteStatement<T> : BaseStatement
    {
        public static BaseStatement Instance { get; } = new SQLiteStatement<T>();

        private SQLiteStatement() : base(typeof(T))
        {
        }

        public override string GetInsertSQL(object o)
        {
            throw new NotImplementedException();
        }

        public override string GetUpdateSQL(object o)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Base DAO Object interface
    /// </summary>
    public interface IDaoObject { }

    /// <summary>
    /// Base DAO Object Class
    /// </summary>
    public abstract class DataObject : IDaoObject
    {
    }
}
