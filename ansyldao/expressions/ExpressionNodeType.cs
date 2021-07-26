using System;
using System.Linq.Expressions;

namespace ansyl.dao.expressions
{
    /// <summary>
    /// Determine what SQL operator to map various expression node types to
    /// </summary>
    class ExpressionNodeType
    {
        public static string GetNodeType(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";

                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";

                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";

                case ExpressionType.Divide:
                    return "/";

                case ExpressionType.Multiply:
                    return "*";

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";

                case ExpressionType.Modulo:
                    return "%";

                //case ExpressionType.ArrayLength:
                //    break;
                //case ExpressionType.ArrayIndex:
                //    break;
                //case ExpressionType.Call:
                //    break;
                //case ExpressionType.Coalesce:
                //    break;
                //case ExpressionType.Conditional:
                //    break;
                //case ExpressionType.Constant:
                //    break;
                //case ExpressionType.Convert:
                //    break;
                //case ExpressionType.ConvertChecked:
                //    break;
                //    break;
                //case ExpressionType.ExclusiveOr:
                //    break;
                //case ExpressionType.Invoke:
                //    break;
                //case ExpressionType.Lambda:
                //    break;
                //case ExpressionType.LeftShift:
                //    break;
                //case ExpressionType.ListInit:
                //    break;
                //case ExpressionType.MemberAccess:
                //    break;
                //case ExpressionType.MemberInit:
                //    break;

                //    break;
                //case ExpressionType.MultiplyChecked:
                //    break;
                //case ExpressionType.Negate:
                //    break;
                //case ExpressionType.UnaryPlus:
                //    break;
                //case ExpressionType.NegateChecked:
                //    break;
                //case ExpressionType.New:
                //    break;
                //case ExpressionType.NewArrayInit:
                //    break;
                //case ExpressionType.NewArrayBounds:
                //    break;
                //case ExpressionType.Not:
                //    break;

                //    break;
                //    break;
                //case ExpressionType.Parameter:
                //    break;
                //case ExpressionType.Power:
                //    break;
                //case ExpressionType.Quote:
                //    break;
                //case ExpressionType.RightShift:
                //    break;
                //case ExpressionType.TypeAs:
                //    break;
                //case ExpressionType.TypeIs:
                //    break;
                //case ExpressionType.Assign:
                //    break;
                //case ExpressionType.Block:
                //    break;
                //case ExpressionType.DebugInfo:
                //    break;
                //case ExpressionType.Decrement:
                //    break;
                //case ExpressionType.Dynamic:
                //    break;
                //case ExpressionType.Default:
                //    break;
                //case ExpressionType.Extension:
                //    break;
                //case ExpressionType.Goto:
                //    break;
                //case ExpressionType.Increment:
                //    break;
                //case ExpressionType.Index:
                //    break;
                //case ExpressionType.Label:
                //    break;
                //case ExpressionType.RuntimeVariables:
                //    break;
                //case ExpressionType.Loop:
                //    break;
                //case ExpressionType.Switch:
                //    break;
                //case ExpressionType.Throw:
                //    break;
                //case ExpressionType.Try:
                //    break;
                //case ExpressionType.Unbox:
                //    break;
                //case ExpressionType.AddAssign:
                //    break;
                //case ExpressionType.AndAssign:
                //    break;
                //case ExpressionType.DivideAssign:
                //    break;
                //case ExpressionType.ExclusiveOrAssign:
                //    break;
                //case ExpressionType.LeftShiftAssign:
                //    break;
                //case ExpressionType.ModuloAssign:
                //    break;
                //case ExpressionType.MultiplyAssign:
                //    break;
                //case ExpressionType.OrAssign:
                //    break;
                //case ExpressionType.PowerAssign:
                //    break;
                //case ExpressionType.RightShiftAssign:
                //    break;
                //case ExpressionType.SubtractAssign:
                //    break;
                //case ExpressionType.AddAssignChecked:
                //    break;
                //case ExpressionType.MultiplyAssignChecked:
                //    break;
                //case ExpressionType.SubtractAssignChecked:
                //    break;
                //case ExpressionType.PreIncrementAssign:
                //    break;
                //case ExpressionType.PreDecrementAssign:
                //    break;
                //case ExpressionType.PostIncrementAssign:
                //    break;
                //case ExpressionType.PostDecrementAssign:
                //    break;
                //case ExpressionType.TypeEqual:
                //    break;
                //case ExpressionType.OnesComplement:
                //    break;
                //case ExpressionType.IsTrue:
                //    break;
                //case ExpressionType.IsFalse:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }
        }
    }
}