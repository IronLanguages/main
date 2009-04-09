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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Dynamic;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting;
using System.Reflection;
using System.Diagnostics;

namespace IronPython.Runtime.Binding {
    
    class ConversionBinder : ConvertBinder, IPythonSite, IExpressionSerializable  {
        private readonly BinderState/*!*/ _state;
        private readonly ConversionResultKind/*!*/ _kind;

        public ConversionBinder(BinderState/*!*/ state, Type/*!*/ type, ConversionResultKind resultKind)
            : base(type, resultKind == ConversionResultKind.ExplicitCast || resultKind == ConversionResultKind.ExplicitTry) {
            Assert.NotNull(state, type);

            _state = state;
            _kind = resultKind;
        }

        public ConversionResultKind ResultKind {
            get {
                return _kind;
            }
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject self, DynamicMetaObject errorSuggestion) {
            if (self.NeedsDeferral()) {
                return Defer(self);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Convert " + Type.FullName + " " + self.LimitType);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "Conversion");

#if !SILVERLIGHT
            DynamicMetaObject comConvert;
            if (ComBinder.TryConvert(this, self, out comConvert)) {
                return comConvert;
            }
#endif

            Type type = Type;

            DynamicMetaObject res = null;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    res = MakeToBoolConversion(self);
                    break;
                case TypeCode.Char:
                    res = TryToCharConversion(self);
                    break;
                case TypeCode.Object:
                    // !!! Deferral?
                    if (type.IsArray && self.Value is PythonTuple && type.GetArrayRank() == 1) {
                        res = MakeToArrayConversion(self, type);
                    } else if (type.IsGenericType && !type.IsAssignableFrom(CompilerHelpers.GetType(self.Value))) {
                        Type genTo = type.GetGenericTypeDefinition();

                        // Interface conversion helpers...
                        if (genTo == typeof(IList<>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IList<object>), typeof(ListGenericWrapper<>));
                        } else if (genTo == typeof(IDictionary<,>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IDictionary<object, object>), typeof(DictionaryGenericWrapper<,>));
                        } else if (genTo == typeof(IEnumerable<>)) {
                            res = TryToGenericInterfaceConversion(self, type, typeof(IEnumerable), typeof(IEnumerableOfTWrapper<>));
                        }
                    } else if (type == typeof(IEnumerable)) {
                        if (self.GetLimitType() == typeof(string)) {
                            // replace strings normal enumeration with our own which returns strings instead of chars.
                            res = new DynamicMetaObject(
                                Ast.Call(
                                    typeof(StringOps).GetMethod("ConvertToIEnumerable"),
                                    AstUtils.Convert(self.Expression, typeof(string))
                                ),
                                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(string))
                            );
                        } else if (!typeof(IEnumerable).IsAssignableFrom(self.GetLimitType()) && IsIndexless(self)) {
                            res = PythonProtocol.ConvertToIEnumerable(this, self.Restrict(self.GetLimitType()));
                        }
                    } else if (type == typeof(IEnumerator) ) {
                        if (!typeof(IEnumerator).IsAssignableFrom(self.GetLimitType()) && 
                            !typeof(IEnumerable).IsAssignableFrom(self.GetLimitType()) &&
                            IsIndexless(self)) {
                            res = PythonProtocol.ConvertToIEnumerator(this, self.Restrict(self.GetLimitType()));
                        }
                    }
                    break;
            }

            if (type.IsEnum && Enum.GetUnderlyingType(type) == self.GetLimitType()) {
                // numeric type to enum, this is ok if the value is zero
                object value = Activator.CreateInstance(type);

                return new DynamicMetaObject(
                    Ast.Condition(
                        Ast.Equal(
                            AstUtils.Convert(self.Expression, Enum.GetUnderlyingType(type)),
                            AstUtils.Constant(Activator.CreateInstance(self.GetLimitType()))
                        ),
                        AstUtils.Constant(value),
                        Ast.Call(
                            typeof(PythonOps).GetMethod("TypeErrorForBadEnumConversion").MakeGenericMethod(type),
                            AstUtils.Convert(self.Expression, typeof(object))
                        )
                    ),
                    self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, self.GetLimitType())),
                    value
                );
            }

            return res ?? Binder.Binder.ConvertTo(Type, ResultKind, self);
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            //Debug.Assert(typeof(T).GetMethod("Invoke").ReturnType == Type);

            object target = args[0];
            T res = null;
            if (typeof(T) == typeof(Func<CallSite, object, string>) && target is string) {
                res = (T)(object)new Func<CallSite, object, string>(StringConversion);
            } else if (typeof(T) == typeof(Func<CallSite, object, int>) && target is int) {
                res = (T)(object)new Func<CallSite, object, int>(IntConversion);
            } else if (typeof(T) == typeof(Func<CallSite, object, bool>) && target is bool) {
                res = (T)(object)new Func<CallSite, object, bool>(BoolConversion);
            } else if (target != null) {
                if (target.GetType() == Type || Type.IsAssignableFrom(target.GetType())) {
                    if (typeof(T) == typeof(Func<CallSite, object, object>)) {
                        // called via a helper call site in the runtime (e.g. Converter.Convert)
                        res = (T)(object)new Func<CallSite, object, object>(new IdentityConversion(target.GetType()).Convert);
                    } else {
                        // called via an embedded call site
                        Debug.Assert(typeof(T).GetMethod("Invoke").ReturnType == Type);
                        if (typeof(T).GetMethod("Invoke").GetParameters()[1].ParameterType == typeof(object)) {
                            object identityConversion = Activator.CreateInstance(typeof(IdentityConversion<>).MakeGenericType(Type), target.GetType());
                            res = (T)(object)Delegate.CreateDelegate(typeof(T), identityConversion, identityConversion.GetType().GetMethod("Convert"));
                        }
                    }
                }
            }
            
            if (res != null) {
                CacheTarget(res);
                return res;
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Convert " + Type.FullName + " " + CompilerHelpers.GetType(args[0]) + " " + typeof(T));
            return base.BindDelegate(site, args);
        }

        public string StringConversion(CallSite site, object value) {
            string str = value as string;
            if (str != null) {
                return str;
            }

            return ((CallSite<Func<CallSite, object, string>>)site).Update(site, value);
        }

        public int IntConversion(CallSite site, object value) {
            if (value is int) {
                return (int)value;
            }

            return ((CallSite<Func<CallSite, object, int>>)site).Update(site, value);
        }

        public bool BoolConversion(CallSite site, object value) {
            if (value is bool) {
                return (bool)value;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
        }
        
        class IdentityConversion {
            private readonly Type _type;

            public IdentityConversion(Type type) {
                _type = type;
            }
            public object Convert(CallSite site, object value) {
                if (value != null && value.GetType() == _type) {
                    return value;
                }

                return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
            }
        }

        class IdentityConversion<T> {
            private readonly Type _type;

            public IdentityConversion(Type type) {
                _type = type;
            }

            public T Convert(CallSite site, object value) {
                if (value != null && value.GetType() == _type) {
                    return (T)value;
                }

                return ((CallSite<Func<CallSite, object, T>>)site).Update(site, value);
            }
        }

        internal static bool IsIndexless(DynamicMetaObject/*!*/ arg) {
            return arg.GetLimitType() != typeof(OldInstance) &&
                arg.GetLimitType() != typeof(BuiltinFunction) &&
                arg.GetLimitType() != typeof(BuiltinMethodDescriptor);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode() ^ _kind.GetHashCode();
        }

        public override bool Equals(object obj) {
            ConversionBinder ob = obj as ConversionBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && _kind == ob._kind && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        #region Conversion Logic

        private DynamicMetaObject TryToGenericInterfaceConversion(DynamicMetaObject/*!*/ self, Type/*!*/ toType, Type/*!*/ fromType, Type/*!*/ wrapperType) {
            if (fromType.IsAssignableFrom(CompilerHelpers.GetType(self.Value))) {
                Type making = wrapperType.MakeGenericType(toType.GetGenericArguments());

                self = self.Restrict(CompilerHelpers.GetType(self.Value));

                return new DynamicMetaObject(
                    Ast.New(
                        making.GetConstructor(new Type[] { fromType }),
                        AstUtils.Convert(
                            self.Expression,
                            fromType
                        )
                    ),
                    self.Restrictions
                );
            }
            return null;
        }

        private DynamicMetaObject/*!*/ MakeToArrayConversion(DynamicMetaObject/*!*/ self, Type/*!*/ toType) {
            self = self.Restrict(typeof(PythonTuple));

            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("ConvertTupleToArray").MakeGenericMethod(toType.GetElementType()),
                    self.Expression
                ),
                self.Restrictions
            );
        }

        private DynamicMetaObject TryToCharConversion(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject res;
            // we have an implicit conversion to char if the
            // string length == 1, but we can only represent
            // this is implicit via a rule.
            string strVal = self.Value as string;
            Expression strExpr = self.Expression;
            if (strVal == null) {
                Extensible<string> extstr = self.Value as Extensible<string>;
                if (extstr != null) {
                    strVal = extstr.Value;
                    strExpr =
                        Ast.Property(
                            AstUtils.Convert(
                                strExpr,
                                typeof(Extensible<string>)
                            ),
                            typeof(Extensible<string>).GetProperty("Value")
                        );
                }
            }

            // we can only produce a conversion if we have a string value...
            if (strVal != null) {
                self = self.Restrict(self.GetRuntimeType());

                Expression getLen = Ast.Property(
                    AstUtils.Convert(
                        strExpr,
                        typeof(string)
                    ),
                    typeof(string).GetProperty("Length")
                );

                if (strVal.Length == 1) {
                    res = new DynamicMetaObject(
                        Ast.Call(
                            AstUtils.Convert(strExpr, typeof(string)),
                            typeof(string).GetMethod("get_Chars"),
                            AstUtils.Constant(0)
                        ),
                        self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Ast.Equal(getLen, AstUtils.Constant(1))))
                    );
                } else {
                    res = new DynamicMetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TypeError"),
                                AstUtils.Constant("expected string of length 1 when converting to char, got '{0}'"),
                                Ast.NewArrayInit(typeof(object), self.Expression)
                            )
                        ),
                        self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Ast.NotEqual(getLen, AstUtils.Constant(1))))
                    );
                }
            } else {
                // let the base class produce the rule
                res = null;
            }

            return res;
        }

        private DynamicMetaObject/*!*/ MakeToBoolConversion(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject res = null;
            if (self.NeedsDeferral()) {
                res = Defer(self);
            } else {
                if (self.HasValue) {
                    self = self.Restrict(self.GetRuntimeType());
                }

                // Optimization: if we already boxed it to a bool, and now
                // we're unboxing it, remove the unnecessary box.
                if (self.Expression.NodeType == ExpressionType.Convert && self.Expression.Type == typeof(object)) {
                    var convert = (UnaryExpression)self.Expression;
                    if (convert.Operand.Type == typeof(bool)) {
                        return new DynamicMetaObject(convert.Operand, self.Restrictions);
                    }
                }

                if (self.GetLimitType() == typeof(DynamicNull)) {
                    // None has no __nonzero__ and no __len__ but it's always false
                    res = MakeNoneToBoolConversion(self);
                } else if (self.GetLimitType() == typeof(bool)) {
                    // nothing special to convert from bool to bool
                    res = self;
                } else if (typeof(IStrongBox).IsAssignableFrom(self.GetLimitType())) {
                    // Explictly block conversion of References to bool
                    res = MakeStrongBoxToBoolConversionError(self);
                } else if (self.GetLimitType().IsPrimitive || self.GetLimitType().IsEnum) {
                    // optimization - rather than doing a method call for primitives and enums generate
                    // the comparison to zero directly.
                    res = MakePrimitiveToBoolComparison(self);
                } else {
                    // anything non-null that doesn't fall under one of the above rules is true.  So we
                    // fallback to the base Python conversion which will check for __nonzero__ and
                    // __len__.  The fallback is handled by our ConvertTo site binder.
                    return
                        PythonProtocol.ConvertToBool(this, self) ??
                        new DynamicMetaObject(
                            AstUtils.Constant(true),
                            self.Restrictions
                        );
                }
            }

            return res;
        }

        private static DynamicMetaObject/*!*/ MakeNoneToBoolConversion(DynamicMetaObject/*!*/ self) {
            // null is never true
            return new DynamicMetaObject(
                AstUtils.Constant(false),
                self.Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakePrimitiveToBoolComparison(DynamicMetaObject/*!*/ self) {
            object zeroVal = Activator.CreateInstance(self.GetLimitType());

            return new DynamicMetaObject(
                Ast.NotEqual(
                    AstUtils.Constant(zeroVal),
                    self.Expression
                ),
                self.Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakeStrongBoxToBoolConversionError(DynamicMetaObject/*!*/ self) {
            return new DynamicMetaObject(
                Ast.Throw(
                    Ast.Call(
                        typeof(ScriptingRuntimeHelpers).GetMethod("SimpleTypeError"),
                        AstUtils.Constant("Can't convert a Reference<> instance to a bool")
                    )
                ),
                self.Restrictions
            );
        }

        #endregion

        public override string ToString() {
            return String.Format("Python Convert {0} {1}", Type, ResultKind);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeConversionAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Type),
                AstUtils.Constant(ResultKind)
            );
        }

        #endregion
    }
}
