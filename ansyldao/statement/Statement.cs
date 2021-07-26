#if false
using System;
using System.Collections.Generic;
using System.Linq;

namespace ansyl.dao
{
    abstract class Statement
    {
        Statement() { }

        [Obsolete]
        public string Select { get; }

        [Obsolete]
        public string Delete { get; }
        [Obsolete]
        public string Insert { get; }
        [Obsolete]
        public string Update { get; }

        public string DeleteOne { get; }
        public string InsertOne { get; }
        public string UpdateOne { get; }
        public string SelectOne { get; }
        public string SelectAll { get; }


        private const StringComparison IgnoreCase = StringComparison.CurrentCultureIgnoreCase;

        public string ClassName { get; }
        public string TableName { get; }

        internal string[] Properties { get; }
        internal string[] OtherProperties { get; }
        public string PkProperty { get; }

        internal Statement(Type type)
        {
            ClassName = type.Name;
            TableName = DOE.GetTableName(type);

            var properties = type.GetProperties().Select(i => i.Name).ToArray();
            Properties = properties;

            OtherProperties = (from pi in properties
                               where pi.Equals(type.Name + "Id", IgnoreCase) == false
                               select pi).ToArray();

            PkProperty = (from pi in properties
                          where pi.Equals(type.Name + "Id", IgnoreCase)
                          select pi).Single();

            Select = GetSelectSQL();
            Update = GetUpdateSQL(OtherProperties);
            Delete = GetDeleteSQL();
            Insert = GetInsertSQL(Properties);


            SelectAll = GetSelectAllSQL();
            SelectOne = GetSelectOneSQL();
            UpdateOne = GetUpdateSQL(OtherProperties);
            DeleteOne = GetDeleteSQL();
            InsertOne = GetInsertSQL(Properties);
        }

        public string GetUpdateSQL(object o)
            => GetUpdateSQL(o == null ? Properties : o.GetType().GetProperties().Select(i => i.Name).ToArray());

        private string GetDeleteSQL()
            => string.Format("DELETE FROM {0} WHERE `{1}Id`=@{1}Id", TableName, ClassName);

        private string GetSelectSQL()
            => string.Format("SELECT * FROM {0} WHERE `{1}Id`=@{1}Id", TableName, ClassName);

        private string GetSelectOneSQL()
            => string.Format("SELECT * FROM {0} WHERE `{1}Id`=@{1}Id", TableName, ClassName);

        private string GetSelectAllSQL()
            => string.Format("SELECT * FROM {0} ORDER BY 1", TableName, ClassName);

        public string GetInsertSQL(object o)
            => GetInsertSQL(o == null ? Properties : o.GetType().GetProperties().Select(i => i.Name).ToArray());

        private string GetInsertSQL(string[] properties)
        {
            var fields = properties.Select(i => "`" + i + "`").ToArray();
            var values = properties.Select(i => "@" + i).ToArray();

            var joinFields = string.Join(", ", fields);
            var joinValues = string.Join(", ", values);

            return $"INSERT INTO {TableName}({joinFields}) VALUES({joinValues});";
        }
        private string GetUpdateSQL(string[] properties)
        {
            var parameters = string.Join(", ", properties.Select(p => $"`{p}`=@{p}").ToArray());
            var primaryKey = $"{ClassName}ID";
            return $"UPDATE {TableName} SET {parameters} WHERE `{primaryKey}`=@{primaryKey}";
        }

        private static readonly IDictionary<string, BaseStatement> Statements = new Dictionary<string, BaseStatement>();

        public static BaseStatement Of<T>()
        {
            var typename = typeof(T).FullName;
            if (typename == null)
                return null;

            if (Statements.ContainsKey(typename) == false)
                Statements[typename] = MySQLStatement<T>.CreateInstance();

            return Statements[typename];
        }
    }
} 
#endif