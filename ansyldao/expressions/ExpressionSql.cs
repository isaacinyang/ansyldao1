using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ansyl.dao.expressions
{
    /// <summary>
    /// Generates SQL statements based on a given Expression statement
    /// </summary>
    public class ExpressionSql<T> where T : class
    {
        public ExpressionSql(Expression<Func<T, bool>> predicate)
        {
            var sqlWhere = GetSqlCore(predicate.Body);

            if (string.IsNullOrWhiteSpace(sqlWhere))
                throw new Exception(predicate.ToString());

            var tableAlias = predicate.Parameters[0].Name;
            //var tableName = typeof(T).Name.ToLower();
            //SqlSelect = $"SELECT * FROM `{tableName}` {tableAlias} WHERE {sqlWhere}";

            var tableName = MySQLStatement<T>.Instance.TableName;
            SqlSelect = $"SELECT * FROM {tableName} {tableAlias} WHERE {sqlWhere}";
        }

        public string                     SqlSelect { get; }
        public Dictionary<string, object> Data      { get; } = new Dictionary<string, object>();

        /// <summary>
        /// generates a DB parameter OR NULL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string GetParameterizedValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            var key = "@p" + (Data.Count + 1);
            Data.Add(key, value);
            return key;
        }

        /// <summary>
        /// extracts the RAW value for Constant Expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        string GetValueRaw(Expression expression)
        {
            //  get value object
            var unaryExpression = Expression.Convert(expression, typeof(object));
            var constantValue = Expression.Lambda<Func<object>>(unaryExpression).Compile().Invoke();

            //  e.g. e.Id == null
            if (constantValue == null)
                return GetParameterizedValue(null);

            //  extract the actual primitive value
            var type = constantValue.GetType();
            var isPrimitiveType = type.IsPrimitive || type.IsValueType || type == typeof(string);

            object finalValue = constantValue;

            if (isPrimitiveType == false)
            {
                var fields = type.GetFields();

                if (fields.Length == 1)
                {
                    finalValue = fields[0].GetValue(constantValue);
                }
                else
                {
                    foreach (var field in fields)
                    {
                        Console.WriteLine("orphaned value: " + field.GetValue(constantValue));
                    }
                }
            }

            //Console.WriteLine($"{constantValue} => {finalValue}");

            if (finalValue == null)
                throw new Exception("No Raw Value!!");

            return GetParameterizedValue(finalValue);
        }

        #region GetValue

        /// <summary>
        /// Analyse and return Raw Value of various expression types
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        string GetValue(Expression expression)
        {
            if (expression is ConstantExpression ce)
                return GetValueRaw(ce);

            if (expression is MemberExpression me)
                return GetValueRaw(me);

            if (expression is NewExpression ne)
                return GetValueRaw(ne);

            throw new Exception("unhandled getValue: " + expression);
        }

        #endregion

        #region GetSQL

        /// <summary>
        /// Generate SQL statements for BinaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(BinaryExpression expression)
        {
            if (expression == null)
                return null;

            var left = expression.Left;
            var right = expression.Right;
            var nodeType = expression.NodeType;

            var lft = GetSqlCore(left);
            var rgt = GetSqlCore(right);
            var mid = ExpressionNodeType.GetNodeType(nodeType);

            if (lft == null || rgt == null || mid == null)
            {
                //var log = new StringWriter();

                //log.WriteLine("{0,10} : {1,-20} // {2}", "LFT", left, lft);
                //log.WriteLine("{0,10} : {1,-20} // {2}", "MID", nodeType, mid);
                //log.WriteLine("{0,10} : {1,-20} // {2}", "RGT", right, rgt);

                //throw new Exception(log.ToString());
                throw new Exception("LEFT, RIGHT or MID is equal to NULL");
            }

            if (lft == "NULL" || rgt == "NULL")
            {
                if (nodeType == ExpressionType.Equal)
                    mid = "IS";
                else if (nodeType == ExpressionType.NotEqual)
                    mid = "IS NOT";
            }

            return $"({lft} {mid} {rgt})";
        }

        /// <summary>
        /// Generate SQL statements for ConstantExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(ConstantExpression expression)
        {
            return expression == null ? null : GetValueRaw(expression);
        }

        /// <summary>
        /// Generate SQL statements for NewExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(NewExpression expression)
        {
            return expression == null ? null : GetValue(expression);
        }

        /// <summary>
        /// Generate SQL statements - this coordinates the various GetSQL methods
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns></returns>
        string GetSqlCore(Expression expression)
        {
            if (expression == null)
                return null;

            if (expression is BinaryExpression be)
                return GetSql(be);

            if (expression is ConstantExpression ce)
                return GetSql(ce);

            if (expression is MemberExpression me)
                return GetSql(me);

            if (expression is UnaryExpression ue)
                return GetSql(ue);

            if (expression is MethodCallExpression mce)
                return GetSql(mce);

            if (expression is NewExpression ne)
                return GetSql(ne);

            return null;
        }

        /// <summary>
        /// Check if the value passed is either a string, value type, enum or array of values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static bool IsValidValue(object value)
        {
            if (value == null)
                return true;

            var type = value.GetType();
            return type == typeof(string) || type.IsValueType || type.IsEnum || type.IsArray;
        }

        /// <summary>
        /// Generate SQL statements for MemberExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(MemberExpression expression)
        {
            //  e.StudentID
            if (expression.Expression.NodeType == ExpressionType.Parameter)
                return $"{expression}";

            //  attempt to pull raw value
            var unaryExpression = Expression.Convert(expression, typeof(object));
            var constantValue = Expression.Lambda<Func<object>>(unaryExpression).Compile().Invoke();

            if (IsValidValue(constantValue))
                return GetParameterizedValue(constantValue);

            foreach (var ex in new[] {expression, expression.Expression})
            {
                if (ex is MemberExpression me && me.Expression is ConstantExpression ce)
                {
                    var value1 = ce.Value;
                    var type1 = value1.GetType();

                    if (me.Member is FieldInfo fi)
                    {
                        if (type1.IsValueType)
                        {
                            var value2 = fi.GetValue(value1);
                            return GetParameterizedValue(value2);
                        }

                        //{
                        //    Console.WriteLine(type1);
                        //}
                    }

                    if (me.Member is PropertyInfo pi)
                    {
                        if (type1.IsValueType)
                        {
                            var value2 = pi.GetValue(value1);
                            return GetParameterizedValue(value2);
                        }

                        //{
                        //    Console.WriteLine(type1);
                        //}
                    }
                }
            }

            return GetSqlCore(expression.Expression);
        }

        /// <summary>
        /// Generate SQL statements for UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(UnaryExpression expression)
            => GetSqlCore(expression.Operand);

        /// <summary>
        /// Generate SQL statements for MethodCallExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetSql(MethodCallExpression expression)
        {
            if (expression == null)
                return null;

            //  e.StudentName.Contains('isaac')
            var methodName = expression.Method.Name;

            if (expression.Arguments.Count == 1)
            {
                //  value
                var pValue = GetValue(expression.Arguments[0]);

                switch (methodName)
                {
                    //  e.Surname.StartsWith('Inya')
                    case "StartsWith":
                        pValue = $"{pValue}, '%'";
                        break;
                    //  e.Surname.EndsWith('ang')
                    case "EndsWith":
                        pValue = $"'%', {pValue}";
                        break;
                    //  e.Surname.Contains('yan')
                    case "Contains":
                        pValue = $"'%', {pValue}, '%'";
                        break;
                    default:
                        return null;
                }

                return $"{expression.Object} LIKE CONCAT({pValue})";
            }

            if (expression.Arguments.Count == 2)
            {
                var l = expression.Arguments[0];
                var r = expression.Arguments[1];

                var lft = GetSqlCore(l);
                var rgt = GetSqlCore(r);

                switch (methodName)
                {
                    //  new[]{1,2,3}.Contains(e.StudentId)
                    case "Contains":
                        return $"{rgt} IN {lft}";
                    //  e.StudentId.In(new[]{1,2,3})
                    case "In":
                        return $"{lft} IN {rgt}";
                }

                throw new Exception($"({lft} {methodName} {rgt}");
            }

            return null;
        }

        #endregion
    }
}
