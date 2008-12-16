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

using System.Dynamic.Utils;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;

namespace System.Linq.Expressions {
    //CONFORMING
    /// <summary>
    /// Base class for specialized parameter expressions.  This version only holds onto the
    /// name which all subclasses need.  Specialized subclasses provide the type and by ref
    /// flags.
    /// </summary>
    public class ParameterExpression : Expression {
        private readonly string _name;

        internal ParameterExpression(string name) {
            _name = name;
        }

        internal static ParameterExpression Make(Type type, string name, bool isByRef) {
            if (isByRef) {
                return new ByRefParameterExpression(type, name);
            } else {
                if (!type.IsEnum) {
                    switch (Type.GetTypeCode(type)) {
                        case TypeCode.Boolean: return new PrimitiveParameterExpression<Boolean>(name);
                        case TypeCode.Byte: return new PrimitiveParameterExpression<Byte>(name);
                        case TypeCode.Char: return new PrimitiveParameterExpression<Char>(name);
                        case TypeCode.DateTime: return new PrimitiveParameterExpression<DateTime>(name);
                        case TypeCode.DBNull: return new PrimitiveParameterExpression<DBNull>(name);
                        case TypeCode.Decimal: return new PrimitiveParameterExpression<Decimal>(name);
                        case TypeCode.Double: return new PrimitiveParameterExpression<Double>(name);
                        case TypeCode.Int16: return new PrimitiveParameterExpression<Int16>(name);
                        case TypeCode.Int32: return new PrimitiveParameterExpression<Int32>(name);
                        case TypeCode.Int64: return new PrimitiveParameterExpression<Int64>(name);
                        case TypeCode.Object:
                            // common reference types which we optimize go here.  Of course object is in
                            // the list, the others are driven by profiling of various workloads.  This list
                            // should be kept short.
                            if (type == typeof(object)) {
                                return new ParameterExpression(name);
                            } else if (type == typeof(Exception)) {
                                return new PrimitiveParameterExpression<Exception>(name);
                            } else if (type == typeof(object[])) {
                                return new PrimitiveParameterExpression<object[]>(name);
                            }
                            break;
                        case TypeCode.SByte: return new PrimitiveParameterExpression<SByte>(name);
                        case TypeCode.Single: return new PrimitiveParameterExpression<Single>(name);
                        case TypeCode.String: return new PrimitiveParameterExpression<String>(name);
                        case TypeCode.UInt16: return new PrimitiveParameterExpression<UInt16>(name);
                        case TypeCode.UInt32: return new PrimitiveParameterExpression<UInt32>(name);
                        case TypeCode.UInt64: return new PrimitiveParameterExpression<UInt64>(name);
                    }
                }
            }

            return new TypedParameterExpression(type, name);            
        }

        protected override Type GetExpressionType() {
            return typeof(object);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Parameter;
        }

        public string Name {
            get { return _name; }
        }

        public bool IsByRef {
            get {
                return GetIsByRef();
            }
        }

        internal virtual bool GetIsByRef() {
            return false;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitParameter(this);
        }
    }

    /// <summary>
    /// Specialized subclass to avoid holding onto the byref flag in a 
    /// parameter expression.  This version always holds onto the expression
    /// type explicitly and therefore derives from TypedParameterExpression.
    /// </summary>
    internal sealed class ByRefParameterExpression : TypedParameterExpression {
        internal ByRefParameterExpression(Type type, string name)
            : base(type, name) {
        }

        internal override bool GetIsByRef() {
            return true;
        }
    }

    /// <summary>
    /// Specialized subclass which holds onto the type of the expression for
    /// uncommon types.
    /// </summary>
    internal class TypedParameterExpression : ParameterExpression {
        private readonly Type _paramType;

        internal TypedParameterExpression(Type type, string name)
            : base(name) {
            _paramType = type;
        }
        
        protected override Type GetExpressionType() {
            return _paramType;
        }
    }

    /// <summary>
    /// Generic type to avoid needing explicit storage for primitive data types
    /// which are commonly used.
    /// </summary>
    internal sealed class PrimitiveParameterExpression<T> : ParameterExpression {
        internal PrimitiveParameterExpression(string name)
            : base(name) {
        }

        protected override Type GetExpressionType() {
            return typeof(T);
        }        
    }

    public partial class Expression {
        //CONFORMING
        public static ParameterExpression Parameter(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            bool byref = type.IsByRef;
            if (byref) {
                type = type.GetElementType();
            }

            return ParameterExpression.Make(type, name, byref);
        }

        public static ParameterExpression Variable(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type != typeof(void), "type", Strings.ArgumentCannotBeOfTypeVoid);
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);
            return ParameterExpression.Make(type, name, false);
        }

        //Variables must not be ByRef.
        internal static void RequireVariableNotByRef(ParameterExpression v, string varName) {
            Debug.Assert(varName != null);
            if (v != null && v.IsByRef) {
                throw new ArgumentException(Strings.VariableMustNotBeByRef, varName);
            }
        }

        internal static void RequireVariablesNotByRef(ReadOnlyCollection<ParameterExpression> vs, string collectionName) {
            Debug.Assert(vs != null);
            Debug.Assert(collectionName != null);
            foreach (ParameterExpression v in vs) {
                RequireVariableNotByRef(v, collectionName);
            }
        }
    }
}
