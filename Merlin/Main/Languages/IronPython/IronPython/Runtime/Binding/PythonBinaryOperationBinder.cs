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

using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    
    class PythonBinaryOperationBinder : BinaryOperationBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonBinaryOperationBinder(BinderState/*!*/ state, ExpressionType operation)
            : base(operation) {
            _state = state;
        }
       
        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            return PythonProtocol.Operation(this, target, arg, errorSuggestion);
        }

        //private static Func<CallSite, object, object, object> DoubleAddSite = new Func<CallSite, object, object, object>(DoubleAdd);

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {    
            if (args[0] != null &&
                CompilerHelpers.GetType(args[0]) == CompilerHelpers.GetType(args[1])) {
                switch (Operation) {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        return BindAdd<T>(site, args);
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        return BindSubtract<T>(site, args);
                    case ExpressionType.Equal:
                        return BindEqual<T>(site, args);
                    case ExpressionType.NotEqual:
                        return BindNotEqual<T>(site, args);
                }
            }

            return base.BindDelegate<T>(site, args);
        }

        private T BindAdd<T>(CallSite<T> site, object[] args) where T : class {
            Type t = args[0].GetType();
            if (!t.IsEnum) {                
                switch (Type.GetTypeCode(args[0].GetType())) {
                    case TypeCode.Double:
                        if(typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object, object>(DoubleAdd);
                        }
                        break;
                    case TypeCode.Int32:
                        if (typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object, object>(IntAdd);
                        } else if (typeof(T) == typeof(Func<CallSite, object, int, object>)) {
                            return (T)(object)new Func<CallSite, object, int, object>(IntAdd);
                        } else if (typeof(T) == typeof(Func<CallSite, int, object, object>)) {
                            return (T)(object)new Func<CallSite, int, object, object>(IntAdd);
                        }
                        break;
                    case TypeCode.String:
                        if (typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object, object>(StringAdd);
                        }
                        break;

                }
            }
            return base.BindDelegate(site, args);
        }

        private T BindSubtract<T>(CallSite<T> site, object[] args) where T : class {
            Type t = args[0].GetType();
            if (!t.IsEnum) {
                switch (Type.GetTypeCode(args[0].GetType())) {
                    case TypeCode.Double:
                        if (typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object, object>(DoubleSubtract);
                        }
                        break;
                    case TypeCode.Int32:
                        if (typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object, object>(IntSubtract);
                        } else if (typeof(T) == typeof(Func<CallSite, object, int, object>)) {
                            return (T)(object)new Func<CallSite, object, int, object>(IntSubtract);
                        } else if (typeof(T) == typeof(Func<CallSite, int, object, object>)) {
                            return (T)(object)new Func<CallSite, int, object, object>(IntSubtract);
                        }
                        break;
                }
            }
            return base.BindDelegate(site, args);
        }

        private T BindEqual<T>(CallSite<T> site, object[] args) where T : class {
            Type t = args[0].GetType();
            if (!t.IsEnum && typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                switch (Type.GetTypeCode(args[0].GetType())) {
                    case TypeCode.Double:
                        return (T)(object)new Func<CallSite, object, object, object>(DoubleEqual);
                    case TypeCode.Int32:
                        return (T)(object)new Func<CallSite, object, object, object>(IntEqual);
                    case TypeCode.String:
                        return (T)(object)new Func<CallSite, object, object, object>(StringEqual);

                }
            }
            return base.BindDelegate(site, args);
        }

        private T BindNotEqual<T>(CallSite<T> site, object[] args) where T : class {
            Type t = args[0].GetType();
            if (!t.IsEnum && typeof(T) == typeof(Func<CallSite, object, object, object>)) {
                switch (Type.GetTypeCode(args[0].GetType())) {
                    case TypeCode.Double:
                        return (T)(object)new Func<CallSite, object, object, object>(DoubleNotEqual);
                    case TypeCode.Int32:
                        return (T)(object)new Func<CallSite, object, object, object>(IntNotEqual);
                    case TypeCode.String:
                        return (T)(object)new Func<CallSite, object, object, object>(StringNotEqual);

                }
            }
            return base.BindDelegate(site, args);
        }

        private object DoubleAdd(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(double) && 
                other != null && other.GetType() == typeof(double)) {
                return (double)self + (double)other;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntAdd(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(int) && 
                other != null && other.GetType() == typeof(int)) {
                return Int32Ops.Add((int)self, (int)other);
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntAdd(CallSite site, object self, int other) {
            if (self != null && self.GetType() == typeof(int)) {
                return Int32Ops.Add((int)self, other);
            }

            return ((CallSite<Func<CallSite, object, int, object>>)site).Update(site, self, other);
        }

        private object IntAdd(CallSite site, int self, object other) {
            if (other != null && other.GetType() == typeof(int)) {
                return Int32Ops.Add(self, (int)other);
            }

            return ((CallSite<Func<CallSite, int, object, object>>)site).Update(site, self, other);
        }

        private object StringAdd(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(string) && 
                other != null && other.GetType() == typeof(string)) {
                return StringOps.Add((string)self, (string)other);
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object DoubleSubtract(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(double) &&
                other != null && other.GetType() == typeof(double)) {
                return (double)self - (double)other;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntSubtract(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(int) &&
                other != null && other.GetType() == typeof(int)) {
                return Int32Ops.Subtract((int)self, (int)other);
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntSubtract(CallSite site, object self, int other) {
            if (self != null && self.GetType() == typeof(int)) {
                return Int32Ops.Subtract((int)self, other);
            }

            return ((CallSite<Func<CallSite, object, int, object>>)site).Update(site, self, other);
        }

        private object IntSubtract(CallSite site, int self, object other) {
            if (other != null && other.GetType() == typeof(int)) {
                return Int32Ops.Subtract(self, (int)other);
            }

            return ((CallSite<Func<CallSite, int, object, object>>)site).Update(site, self, other);
        }

        private object DoubleEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(double) &&
                other != null && other.GetType() == typeof(double)) {
                return DoubleOps.Equals((double)self, (double)other) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(int) &&
                other != null && other.GetType() == typeof(int)) {
                return (int)self == (int)other ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object StringEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(string) &&
                other != null && other.GetType() == typeof(string)) {
                return StringOps.Equals((string)self, (string)other) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object DoubleNotEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(double) &&
                other != null && other.GetType() == typeof(double)) {
                return DoubleOps.NotEquals((double)self, (double)other) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object IntNotEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(int) &&
                other != null && other.GetType() == typeof(int)) {
                return (int)self != (int)other ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        private object StringNotEqual(CallSite site, object self, object other) {
            if (self != null && self.GetType() == typeof(string) &&
                other != null && other.GetType() == typeof(string)) {
                return StringOps.NotEquals((string)self, (string)other) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, self, other);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonBinaryOperationBinder ob = obj as PythonBinaryOperationBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override string ToString() {
            return "PythonBinary " + Operation;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeBinaryOperationAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Operation)
            );
        }

        #endregion
    }
}
