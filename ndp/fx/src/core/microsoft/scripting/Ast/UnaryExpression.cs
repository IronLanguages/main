/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class UnaryExpression : Expression {
        private readonly Expression _operand;
        private readonly MethodInfo _method;
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        internal UnaryExpression(ExpressionType nodeType, Expression expression, Type type, MethodInfo method) {
            _operand = expression;
            _method = method;
            _nodeType = nodeType;
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return _nodeType;
        }

        public Expression Operand {
            get { return _operand; }
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public bool IsLifted {
            get {
                if (NodeType == ExpressionType.TypeAs || NodeType == ExpressionType.Quote || NodeType == ExpressionType.Throw) {
                    return false;
                }
                bool operandIsNullable = TypeUtils.IsNullableType(_operand.Type);
                bool resultIsNullable = TypeUtils.IsNullableType(this.Type);
                if (_method != null) {
                    return (operandIsNullable && _method.GetParametersCached()[0].ParameterType != _operand.Type) ||
                           (resultIsNullable && _method.ReturnType != this.Type);
                }
                return operandIsNullable || resultIsNullable;
            }
        }

        public bool IsLiftedToNull {
            get {
                return IsLifted && TypeUtils.IsNullableType(this.Type);
            }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitUnary(this);
        }

        public override bool CanReduce {
            get {
                switch (_nodeType) {
                    case ExpressionType.PreIncrementAssign:
                    case ExpressionType.PreDecrementAssign:
                    case ExpressionType.PostIncrementAssign:
                    case ExpressionType.PostDecrementAssign:
                        return true;
                }
                return false;
            }
        }

        public override Expression Reduce() {
            if (CanReduce) {
                switch (_operand.NodeType) {
                    case ExpressionType.Index:
                        return ReduceIndex();
                    case ExpressionType.MemberAccess:
                        return ReduceMember();
                    default:
                        return ReduceVariable();
                }
            }
            return this;
        }

        private bool IsPrefix {
            get { return _nodeType == ExpressionType.PreIncrementAssign || _nodeType == ExpressionType.PreDecrementAssign; }
        }

        private UnaryExpression FunctionalOp(Expression operand) {
            ExpressionType functional;
            if (_nodeType == ExpressionType.PreIncrementAssign || _nodeType == ExpressionType.PostIncrementAssign) {
                functional = ExpressionType.Increment;
            } else {
                functional = ExpressionType.Decrement;
            }
            return new UnaryExpression(functional, operand, operand.Type, _method);
        }
        
        private Expression ReduceVariable() {
            if (IsPrefix) {
                // (op) var
                // ... is reduced into ...
                // var = op(var)
                return Assign(_operand, FunctionalOp(_operand));
            }
            // var (op)
            // ... is reduced into ...
            // temp = var
            // var = op(var)
            // temp
            var temp = Parameter(_operand.Type, null);
            return Block(
                new[] { temp },
                Assign(temp, _operand),
                Assign(_operand, FunctionalOp(temp)),
                temp
            );
        }

        private Expression ReduceMember() {
            var member = (MemberExpression)_operand;
            if (member.Expression == null) {
                //static member, reduce the same as variable
                return ReduceVariable();
            } else {
                var temp1 = Parameter(member.Expression.Type, null);
                var initTemp1 = Assign(temp1, member.Expression);
                member = MakeMemberAccess(temp1, member.Member);

                if (IsPrefix) {
                    // (op) value.member
                    // ... is reduced into ...
                    // temp1 = value
                    // temp1.member = op(temp1.member)
                    return Block(
                        new[] { temp1 },
                        initTemp1,
                        Assign(member, FunctionalOp(member))
                    );
                }

                // value.member (op)
                // ... is reduced into ...
                // temp1 = value
                // temp2 = temp1.member
                // temp1.member = op(temp2)
                // temp2
                var temp2 = Parameter(member.Type, null);
                return Block(
                    new[] { temp1, temp2 },
                    initTemp1,
                    Assign(temp2, member),
                    Assign(member, FunctionalOp(temp2)),
                    temp2
                );
            }
        }

        private Expression ReduceIndex() {
            // left[a0, a1, ... aN] (op)
            //
            // ... is reduced into ...
            //
            // tempObj = left
            // tempArg0 = a0
            // ...
            // tempArgN = aN
            // tempValue = tempObj[tempArg0, ... tempArgN]
            // tempObj[tempArg0, ... tempArgN] = op(tempValue)
            // tempValue

            bool prefix = IsPrefix;
            var index = (IndexExpression)_operand;
            int count =  index.Arguments.Count;
            var block = new Expression[count + (prefix ? 2 : 4)];
            var temps = new ParameterExpression[count + (prefix ? 1 : 2)];
            var args = new ParameterExpression[count];

            int i = 0;
            temps[i] = Parameter(index.Object.Type, null);
            block[i] = Assign(temps[i], index.Object);
            i++;
            while (i <= count) {
                var arg = index.Arguments[i - 1];
                args[i - 1] = temps[i] = Parameter(arg.Type, null);
                block[i] = Assign(temps[i], arg);
                i++;
            }
            index = MakeIndex(temps[0], index.Indexer, new ReadOnlyCollection<Expression>(args));
            if (!prefix) {
                var lastTemp = temps[i] = Parameter(index.Type, null);
                block[i] = Assign(temps[i], index);
                i++;
                Debug.Assert(i == temps.Length);
                block[i++] = Assign(index, FunctionalOp(lastTemp));
                block[i++] = lastTemp;
            } else {
                Debug.Assert(i == temps.Length);
                block[i++] = Assign(index, FunctionalOp(index));
            }
            Debug.Assert(i == block.Length);
            return Block(new ReadOnlyCollection<ParameterExpression>(temps), new ReadOnlyCollection<Expression>(block));
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) {
            return MakeUnary(unaryType, operand, type, null);
        }
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) {
            switch (unaryType) {
                case ExpressionType.Negate:
                    return Negate(operand, method);
                case ExpressionType.NegateChecked:
                    return NegateChecked(operand, method);
                case ExpressionType.Not:
                    return Not(operand, method);
                case ExpressionType.ArrayLength:
                    return ArrayLength(operand);
                case ExpressionType.Convert:
                    return Convert(operand, type, method);
                case ExpressionType.ConvertChecked:
                    return ConvertChecked(operand, type, method);
                case ExpressionType.Throw:
                    return Throw(operand, type);
                case ExpressionType.TypeAs:
                    return TypeAs(operand, type);
                case ExpressionType.Quote:
                    return Quote(operand);
                case ExpressionType.UnaryPlus:
                    return UnaryPlus(operand, method);
                case ExpressionType.Unbox:
                    return Unbox(operand, type);
                case ExpressionType.Increment:
                    return Increment(operand, method);
                case ExpressionType.Decrement:
                    return Decrement(operand, method);
                case ExpressionType.PreIncrementAssign:
                    return PreIncrementAssign(operand, method);
                case ExpressionType.PostIncrementAssign:
                    return PostIncrementAssign(operand, method);
                case ExpressionType.PreDecrementAssign:
                    return PreDecrementAssign(operand, method);
                case ExpressionType.PostDecrementAssign:
                    return PostDecrementAssign(operand, method);
                default:
                    throw Error.UnhandledUnary(unaryType);
            }
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(ExpressionType unaryType, string name, Expression operand) {
            UnaryExpression u = GetUserDefinedUnaryOperator(unaryType, name, operand);
            if (u != null) {
                ValidateParamswithOperandsOrThrow(u.Method.GetParametersCached()[0].ParameterType, operand.Type, unaryType, name);
                return u;
            }
            throw Error.UnaryOperatorNotDefined(unaryType, operand.Type);
        }
        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperator(ExpressionType unaryType, string name, Expression operand) {
            Type operandType = operand.Type;
            Type[] types = new Type[] { operandType };
            Type nnOperandType = TypeUtils.GetNonNullableType(operandType);
            MethodInfo method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            if (method != null) {
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            // try lifted call
            if (TypeUtils.IsNullableType(operandType)) {
                types[0] = nnOperandType;
                method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                if (method != null && method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                    return new UnaryExpression(unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
                }
            }
            return null;
        }
        //CONFORMING
        private static UnaryExpression GetMethodBasedUnaryOperator(ExpressionType unaryType, Expression operand, MethodInfo method) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type)) {
                ValidateParamswithOperandsOrThrow(pms[0].ParameterType, operand.Type, unaryType, method.Name);
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if (TypeUtils.IsNullableType(operand.Type) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                return new UnaryExpression(unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
            }

            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercionOrThrow(ExpressionType coercionType, Expression expression, Type convertToType) {
            UnaryExpression u = GetUserDefinedCoercion(coercionType, expression, convertToType);
            if (u != null) {
                return u;
            }
            throw Error.CoercionOperatorNotDefined(expression.Type, convertToType);
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercion(ExpressionType coercionType, Expression expression, Type convertToType) {
            MethodInfo method = TypeUtils.GetUserDefinedCoercionMethod(expression.Type, convertToType, false);
            if (method != null) {
                return new UnaryExpression(coercionType, expression, convertToType, method);
            } else {
                return null;
            }
        }

        //CONFORMING
        private static UnaryExpression GetMethodBasedCoercionOperator(ExpressionType unaryType, Expression operand, Type convertToType, MethodInfo method) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type) && method.ReturnType == convertToType) {
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if ((TypeUtils.IsNullableType(operand.Type) || TypeUtils.IsNullableType(convertToType)) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType == TypeUtils.GetNonNullableType(convertToType)) {
                return new UnaryExpression(unaryType, operand, convertToType, method);
            }
            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        public static UnaryExpression Negate(Expression expression) {
            return Negate(expression, null);
        }
        //CONFORMING
        public static UnaryExpression Negate(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                    return new UnaryExpression(ExpressionType.Negate, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method);
        }

        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression) {
            return UnaryPlus(expression, null);
        }
        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type)) {
                    return new UnaryExpression(ExpressionType.UnaryPlus, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method);
        }

        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression) {
            return NegateChecked(expression, null);
        }
        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                    return new UnaryExpression(ExpressionType.NegateChecked, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method);
        }

        //CONFORMING
        public static UnaryExpression Not(Expression expression) {
            return Not(expression, null);
        }
        //CONFORMING
        public static UnaryExpression Not(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsIntegerOrBool(expression.Type)) {
                    return new UnaryExpression(ExpressionType.Not, expression, expression.Type, null);
                }
                UnaryExpression u = GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression);
                if (u != null) {
                    return u;
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method);
        }

        //CONFORMING
        public static UnaryExpression TypeAs(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.IncorrectTypeForTypeAs(type);
            }
            return new UnaryExpression(ExpressionType.TypeAs, expression, type, null);
        }

        public static UnaryExpression Unbox(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(
                expression.Type.IsInterface || expression.Type == typeof(object),
                "expression", Strings.InvalidUnboxType
            );
            ContractUtils.Requires(type.IsValueType, "type", Strings.InvalidUnboxType);
            return new UnaryExpression(ExpressionType.Unbox, expression, type, null);
        }

        //CONFORMING
        public static UnaryExpression Convert(Expression expression, Type type) {
            return Convert(expression, type, null);
        }
        //CONFORMING
        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                ContractUtils.RequiresNotNull(type, "type");
                if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) ||
                    TypeUtils.HasReferenceConversion(expression.Type, type)) {
                    return new UnaryExpression(ExpressionType.Convert, expression, type, null);
                }
                return GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method);
        }

        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type) {
            return ConvertChecked(expression, type, null);
        }
        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                ContractUtils.RequiresNotNull(type, "type");
                if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type)) {
                    return new UnaryExpression(ExpressionType.ConvertChecked, expression, type, null);
                }
                if (TypeUtils.HasReferenceConversion(expression.Type, type)) {
                    return new UnaryExpression(ExpressionType.Convert, expression, type, null);
                }
                return GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method);
        }

        //CONFORMING
        public static UnaryExpression ArrayLength(Expression array) {
            ContractUtils.RequiresNotNull(array, "array");
            if (!array.Type.IsArray || !typeof(Array).IsAssignableFrom(array.Type)) {
                throw Error.ArgumentMustBeArray();
            }
            if (array.Type.GetArrayRank() != 1) {
                throw Error.ArgumentMustBeSingleDimensionalArrayType();
            }
            return new UnaryExpression(ExpressionType.ArrayLength, array, typeof(int), null);
        }

        //CONFORMING
        public static UnaryExpression Quote(Expression expression) {
            RequiresCanRead(expression, "expression");
            return new UnaryExpression(ExpressionType.Quote, expression, expression.GetType(), null);
        }

        // TODO: should we just always wrap it in a convert?
        // Do we need this factory at all?
        public static Expression Void(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (expression.Type == typeof(void)) {
                return expression;
            }
            return Expression.Convert(expression, typeof(void));
        }

        public static UnaryExpression Rethrow() {
            return Throw(null);
        }

        public static UnaryExpression Throw(Expression value) {
            return Throw(value, typeof(void));
        }

        public static UnaryExpression Throw(Expression value, Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (value != null) {
                RequiresCanRead(value, "value");
                ContractUtils.Requires(
                    TypeUtils.AreReferenceAssignable(typeof(Exception), value.Type),
                    "value",
                    Strings.ArgumentMustBeException
                );
            }
            return new UnaryExpression(ExpressionType.Throw, value, type, null);
        }

        public static UnaryExpression Increment(Expression expression) {
            return Increment(expression, null);
        }
        public static UnaryExpression Increment(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type)) {
                    return new UnaryExpression(ExpressionType.Increment, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Increment, "op_Increment", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Increment, expression, method);
        }

        public static UnaryExpression Decrement(Expression expression) {
            return Decrement(expression, null);
        }
        public static UnaryExpression Decrement(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type)) {
                    return new UnaryExpression(ExpressionType.Decrement, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Decrement, "op_Decrement", expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Decrement, expression, method);
        }


        public static UnaryExpression PreIncrementAssign(Expression expression) {
            return MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, null);
        }
        public static UnaryExpression PreIncrementAssign(Expression expression, MethodInfo method) {
            return MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, method);
        }
        public static UnaryExpression PreDecrementAssign(Expression expression) {
            return MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, null);
        }
        public static UnaryExpression PreDecrementAssign(Expression expression, MethodInfo method) {
            return MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, method);
        }
        public static UnaryExpression PostIncrementAssign(Expression expression) {
            return MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, null);
        }
        public static UnaryExpression PostIncrementAssign(Expression expression, MethodInfo method) {
            return MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, method);
        }
        public static UnaryExpression PostDecrementAssign(Expression expression) {
            return MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, null);
        }
        public static UnaryExpression PostDecrementAssign(Expression expression, MethodInfo method) {
            return MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, method);
        }
        private static UnaryExpression MakeOpAssignUnary(ExpressionType kind, Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            RequiresCanWrite(expression, "expression");

            UnaryExpression result;
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type)) {
                    return new UnaryExpression(kind, expression, expression.Type, null);
                }
                string name;
                if (kind == ExpressionType.PreIncrementAssign || kind == ExpressionType.PostIncrementAssign) {
                    name = "op_Increment";
                } else {
                    name = "op_Decrement";
                }
                result = GetUserDefinedUnaryOperatorOrThrow(kind, name, expression);
            } else {
                result = GetMethodBasedUnaryOperator(kind, expression, method);
            }
            // return type must be assignable back to the operand type
            if (!TypeUtils.AreReferenceAssignable(expression.Type, result.Type)) {
                throw Error.UserDefinedOpMustHaveValidReturnType(kind, method.Name);
            }
            return result;
        }
    }
}
