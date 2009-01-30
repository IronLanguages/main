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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

namespace Microsoft.Scripting.Generation {
    // TODO: keep this?
    public delegate void ActionRef<T0, T1>(ref T0 arg0, ref T1 arg1);

    public static class CompilerHelpers {
        public static readonly MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;
        private static readonly MethodInfo _CreateInstanceMethod = typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance");

        public static string[] GetArgumentNames(ParameterInfo[] parameterInfos) {
            string[] ret = new string[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) ret[i] = parameterInfos[i].Name;
            return ret;
        }

        public static Type[] GetTypesWithThis(MethodBase mi) {
            Type[] types = ReflectionUtils.GetParameterTypes(mi.GetParameters());
            if (IsStatic(mi)) {
                return types;
            }

            // TODO (Spec#): do not specify <Type> type arg
            return ArrayUtils.Insert<Type>(mi.DeclaringType, types);
        }


        public static Type GetReturnType(MethodBase mi) {
            if (mi.IsConstructor) return mi.DeclaringType;
            else return ((MethodInfo)mi).ReturnType;
        }

        public static int GetStaticNumberOfArgs(MethodBase method) {
            if (IsStatic(method)) return method.GetParameters().Length;

            return method.GetParameters().Length + 1;
        }

        public static bool IsParamArray(ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static bool IsOutParameter(ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            return (pi.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out;
        }

        public static int GetOutAndByRefParameterCount(MethodBase method) {
            int res = 0;
            ParameterInfo[] pis = method.GetParameters();
            for (int i = 0; i < pis.Length; i++) {
                if (IsByRefParameter(pis[i])) res++;
            }
            return res;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified parameter is mandatory, i.e. is not optional and doesn't have a default value.
        /// </summary>
        public static bool IsMandatoryParameter(ParameterInfo pi) {
            return (pi.Attributes & (ParameterAttributes.Optional | ParameterAttributes.HasDefault)) == 0;
        }

        public static bool HasDefaultValue(ParameterInfo pi) {
            return (pi.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        public static bool IsByRefParameter(ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            if (pi.ParameterType.IsByRef) return true;

            return (pi.Attributes & (ParameterAttributes.Out)) == ParameterAttributes.Out;
        }

        public static bool ProhibitsNull(ParameterInfo parameter) {
            return parameter.IsDefined(typeof(NotNullAttribute), false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static object GetMissingValue(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsByRef) type = type.GetElementType();
            if (type.IsEnum) return Activator.CreateInstance(type);

            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    // struct
                    if (type.IsSealed && type.IsValueType) {
                        return Activator.CreateInstance(type);
                    } else if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        return Missing.Value;
                    } else if (!type.IsValueType) {
                        return null;
                    } else {
                        throw Error.CantCreateDefaultTypeFor(type);
                    }
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.String:
                    return null;

                case TypeCode.Boolean: return false;
                case TypeCode.Char: return '\0';
                case TypeCode.SByte: return (sbyte)0;
                case TypeCode.Byte: return (byte)0;
                case TypeCode.Int16: return (short)0;
                case TypeCode.UInt16: return (ushort)0;
                case TypeCode.Int32: return (int)0;
                case TypeCode.UInt32: return (uint)0;
                case TypeCode.Int64: return 0L;
                case TypeCode.UInt64: return 0UL;
                case TypeCode.Single: return 0.0f;
                case TypeCode.Double: return 0.0D;
                case TypeCode.Decimal: return (decimal)0;
                case TypeCode.DateTime: return DateTime.MinValue;
            }
        }

        public static bool IsStatic(MethodBase mi) {
            return mi.IsConstructor || mi.IsStatic;
        }

        /// <summary>
        /// True if the MethodBase is method which is going to construct an object
        /// </summary>
        public static bool IsConstructor(MethodBase mb) {
            if (mb.IsConstructor) {
                return true;
            }

            if (mb.IsGenericMethod) {
                MethodInfo mi = mb as MethodInfo;

                if (mi.GetGenericMethodDefinition() == _CreateInstanceMethod) {
                    return true;
                }
            }

            return false;
        }

        public static T[] MakeRepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }

        /// <summary>
        /// A helper routine to check if a type can be treated as sealed - i.e. there
        /// can never be a subtype of this given type.  This corresponds to a type
        /// that is either declared "Sealed" or is a ValueType and thus unable to be
        /// extended.
        /// </summary>
        public static bool IsSealed(Type type) {
            return type.IsSealed || type.IsValueType;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static Operators OperatorToReverseOperator(Operators op) {
            switch (op) {
                case Operators.LessThan: return Operators.GreaterThan;
                case Operators.LessThanOrEqual: return Operators.GreaterThanOrEqual;
                case Operators.GreaterThan: return Operators.LessThan;
                case Operators.GreaterThanOrEqual: return Operators.LessThanOrEqual;
                case Operators.Equals: return Operators.Equals;
                case Operators.NotEquals: return Operators.NotEquals;
                case Operators.DivMod: return Operators.ReverseDivMod;
                case Operators.ReverseDivMod: return Operators.DivMod;
                #region Generated Operator Reversal

                // *** BEGIN GENERATED CODE ***
                // generated by function: operator_reversal from: generate_ops.py

                case Operators.Add: return Operators.ReverseAdd;
                case Operators.ReverseAdd: return Operators.Add;
                case Operators.Subtract: return Operators.ReverseSubtract;
                case Operators.ReverseSubtract: return Operators.Subtract;
                case Operators.Power: return Operators.ReversePower;
                case Operators.ReversePower: return Operators.Power;
                case Operators.Multiply: return Operators.ReverseMultiply;
                case Operators.ReverseMultiply: return Operators.Multiply;
                case Operators.FloorDivide: return Operators.ReverseFloorDivide;
                case Operators.ReverseFloorDivide: return Operators.FloorDivide;
                case Operators.Divide: return Operators.ReverseDivide;
                case Operators.ReverseDivide: return Operators.Divide;
                case Operators.TrueDivide: return Operators.ReverseTrueDivide;
                case Operators.ReverseTrueDivide: return Operators.TrueDivide;
                case Operators.Mod: return Operators.ReverseMod;
                case Operators.ReverseMod: return Operators.Mod;
                case Operators.LeftShift: return Operators.ReverseLeftShift;
                case Operators.ReverseLeftShift: return Operators.LeftShift;
                case Operators.RightShift: return Operators.ReverseRightShift;
                case Operators.ReverseRightShift: return Operators.RightShift;
                case Operators.BitwiseAnd: return Operators.ReverseBitwiseAnd;
                case Operators.ReverseBitwiseAnd: return Operators.BitwiseAnd;
                case Operators.BitwiseOr: return Operators.ReverseBitwiseOr;
                case Operators.ReverseBitwiseOr: return Operators.BitwiseOr;
                case Operators.ExclusiveOr: return Operators.ReverseExclusiveOr;
                case Operators.ReverseExclusiveOr: return Operators.ExclusiveOr;

                // *** END GENERATED CODE ***

                #endregion
            }
            return Operators.None;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static string OperatorToReverseOperator(string op) {
            switch (op) {
                case StandardOperators.LessThan: return StandardOperators.GreaterThan;
                case StandardOperators.LessThanOrEqual: return StandardOperators.GreaterThanOrEqual;
                case StandardOperators.GreaterThan: return StandardOperators.LessThan;
                case StandardOperators.GreaterThanOrEqual: return StandardOperators.LessThanOrEqual;
                case StandardOperators.Equal: return StandardOperators.Equal;
                case StandardOperators.NotEqual: return StandardOperators.NotEqual;
            }
            return StandardOperators.None;
        }

        public static string InPlaceOperatorToOperator(string op) {
            if (op.StartsWith("InPlace")) {
                return op.Substring(7);
            }

            return StandardOperators.None;
        }

        public static Operators InPlaceOperatorToOperator(Operators op) {
            switch (op) {
                case Operators.InPlaceAdd: return Operators.Add;
                case Operators.InPlaceBitwiseAnd: return Operators.BitwiseAnd;
                case Operators.InPlaceBitwiseOr: return Operators.BitwiseOr;
                case Operators.InPlaceDivide: return Operators.Divide;
                case Operators.InPlaceFloorDivide: return Operators.FloorDivide;
                case Operators.InPlaceLeftShift: return Operators.LeftShift;
                case Operators.InPlaceMod: return Operators.Mod;
                case Operators.InPlaceMultiply: return Operators.Multiply;
                case Operators.InPlacePower: return Operators.Power;
                case Operators.InPlaceRightShift: return Operators.RightShift;
                case Operators.InPlaceSubtract: return Operators.Subtract;
                case Operators.InPlaceTrueDivide: return Operators.TrueDivide;
                case Operators.InPlaceExclusiveOr: return Operators.ExclusiveOr;
                case Operators.InPlaceRightShiftUnsigned: return Operators.RightShiftUnsigned;
                default: return Operators.None;
            }

        }
        public static bool IsComparisonOperator(Operators op) {
            switch (op) {
                case Operators.LessThan: return true;
                case Operators.LessThanOrEqual: return true;
                case Operators.GreaterThan: return true;
                case Operators.GreaterThanOrEqual: return true;
                case Operators.Equals: return true;
                case Operators.NotEquals: return true;
                case Operators.Compare: return true;
            }
            return false;
        }

        public static bool IsComparisonOperator(string op) {
            switch (op) {
                case StandardOperators.LessThan: return true;
                case StandardOperators.LessThanOrEqual: return true;
                case StandardOperators.GreaterThan: return true;
                case StandardOperators.GreaterThanOrEqual: return true;
                case StandardOperators.Equal: return true;
                case StandardOperators.NotEqual: return true;
                case StandardOperators.Compare: return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the System.Type for any object, including null.  The type of null
        /// is represented by None.Type and all other objects just return the 
        /// result of Object.GetType
        /// </summary>
        public static Type GetType(object obj) {
            if (obj == null) {
                return DynamicNull.Type;
            }

            return obj.GetType();
        }

        /// <summary>
        /// Simply returns a Type[] from calling GetType on each element of args.
        /// </summary>
        public static Type[] GetTypes(object[] args) {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = GetType(args[i]);
            }
            return types;
        }

        internal static Type[] GetTypes(IList<Expression> args) {
            Type[] types = new Type[args.Count];
            for (int i = 0, n = types.Length; i < n; i++) {
                types[i] = args[i].Type;
            }
            return types;
        }

        public static bool CanOptimizeMethod(MethodBase method) {
            if (method.ContainsGenericParameters ||
                method.IsFamily ||
                method.IsPrivate ||
                method.IsFamilyOrAssembly ||
                !method.DeclaringType.IsVisible) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Given a MethodInfo which may be declared on a non-public type this attempts to
        /// return a MethodInfo which will dispatch to the original MethodInfo but is declared
        /// on a public type.
        /// 
        /// Returns the original method if the method if a public version cannot be found.
        /// </summary>
        public static MethodInfo TryGetCallableMethod(MethodInfo method) {
            if (method.DeclaringType.IsVisible) return method;
            // first try and get it from the base type we're overriding...
            method = method.GetBaseDefinition();

            if (method.DeclaringType.IsVisible) return method;
            if (method.DeclaringType.IsInterface) return method;
            // maybe we can get it from an interface...
            Type[] interfaces = method.DeclaringType.GetInterfaces();
            foreach (Type iface in interfaces) {
                InterfaceMapping mapping = method.DeclaringType.GetInterfaceMap(iface);
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    if (mapping.TargetMethods[i] == method) {
                        return mapping.InterfaceMethods[i];
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Non-public types can have public members that we find when calling type.GetMember(...).  This
        /// filters out the non-visible members by attempting to resolve them to the correct visible type.
        /// 
        /// If no correct visible type can be found then the member is not visible and we won't call it.
        /// </summary>
        public static MemberInfo[] FilterNonVisibleMembers(Type type, MemberInfo[] foundMembers) {
            if (!type.IsVisible && foundMembers.Length > 0) {
                // need to remove any members that we can't get through other means
                List<MemberInfo> foundVisible = null;
                MemberInfo visible;
                MethodInfo mi;
                for (int i = 0; i < foundMembers.Length; i++) {
                    visible = null;
                    switch (foundMembers[i].MemberType) {
                        case MemberTypes.Method:
                            visible = TryGetCallableMethod((MethodInfo)foundMembers[i]);
                            if (!visible.DeclaringType.IsVisible) {
                                visible = null;
                            }
                            break;
                        case MemberTypes.Property:
                            PropertyInfo pi = (PropertyInfo)foundMembers[i];
                            mi = pi.GetGetMethod() ?? pi.GetSetMethod();
                            visible = TryGetCallableMethod(mi);
                            if (visible.DeclaringType.IsVisible) {
                                visible = visible.DeclaringType.GetProperty(pi.Name);
                            } else {
                                visible = null;
                            }
                            break;
                        case MemberTypes.Event:
                            EventInfo ei = (EventInfo)foundMembers[i];
                            mi = ei.GetAddMethod() ?? ei.GetRemoveMethod() ?? ei.GetRaiseMethod();
                            visible = TryGetCallableMethod(mi);
                            if (visible.DeclaringType.IsVisible) {
                                visible = visible.DeclaringType.GetEvent(ei.Name);
                            } else {
                                visible = null;
                            }
                            break;
                        // all others can't be exposed out this way
                    }
                    if (visible != null) {
                        if (foundVisible == null) {
                            foundVisible = new List<MemberInfo>();
                        }
                        foundVisible.Add(visible);
                    }
                }

                if (foundVisible != null) {
                    foundMembers = foundVisible.ToArray();
                } else {
                    foundMembers = new MemberInfo[0];
                }
            }
            return foundMembers;
        }

        /// <summary>
        /// Given a MethodInfo which may be declared on a non-public type this attempts to
        /// return a MethodInfo which will dispatch to the original MethodInfo but is declared
        /// on a public type.
        /// 
        /// Throws InvalidOperationException if the method cannot be obtained.
        /// </summary>
        public static MethodInfo GetCallableMethod(MethodInfo method, bool privateBinding) {
            MethodInfo mi = TryGetCallableMethod(method);
            if (mi == null) {
                if (!privateBinding) {
                    throw Error.NoCallableMethods(method.DeclaringType, method.Name);
                }
            }
            return mi;
        }

        public static bool CanOptimizeField(FieldInfo fi) {
            return fi.IsPublic && fi.DeclaringType.IsVisible;
        }

        public static Type GetVisibleType(object value) {
            return GetVisibleType(GetType(value));
        }

        public static Type GetVisibleType(Type t) {
            while (!t.IsVisible) {
                t = t.BaseType;
            }
            return t;
        }

        public static MethodBase[] GetConstructors(Type t, bool privateBinding) {
            return GetConstructors(t, privateBinding, false);
        }

        public static MethodBase[] GetConstructors(Type t, bool privateBinding, bool includeProtected) {
            if (t.IsArray) {
                // The JIT verifier doesn't like new int[](3) even though it appears as a ctor.
                // We could do better and return newarr in the future.
                return new MethodBase[] { GetArrayCtor(t) };
            }

            BindingFlags bf = BindingFlags.Instance | BindingFlags.Public;
            if (privateBinding || includeProtected) {
                bf |= BindingFlags.NonPublic;
            }
            ConstructorInfo[] ci = t.GetConstructors(bf);

            // leave in protected ctors, even if we're not in private binding mode.
            if (!privateBinding && includeProtected) {
                ci = FilterConstructorsToPublicAndProtected(ci);
            }

            if (t.IsValueType) {
                // structs don't define a parameterless ctor, add a generic method for that.
                return ArrayUtils.Insert<MethodBase>(GetStructDefaultCtor(t), ci);
            }

            return ci;
        }

        public static ConstructorInfo[] FilterConstructorsToPublicAndProtected(ConstructorInfo[] ci) {
            List<ConstructorInfo> finalInfos = null;
            for (int i = 0; i < ci.Length; i++) {
                ConstructorInfo info = ci[i];
                if (!info.IsPublic && !info.IsFamily && !info.IsFamilyOrAssembly) {
                    if (finalInfos == null) {
                        finalInfos = new List<ConstructorInfo>();
                        for (int j = 0; j < i; j++) {
                            finalInfos.Add(ci[j]);
                        }
                    }
                } else if (finalInfos != null) {
                    finalInfos.Add(ci[i]);
                }
            }

            if (finalInfos != null) {
                ci = finalInfos.ToArray();
            }
            return ci;
        }

        private static MethodBase GetStructDefaultCtor(Type t) {
            return typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(t);
        }

        private static MethodBase GetArrayCtor(Type t) {
            return typeof(ScriptingRuntimeHelpers).GetMethod("CreateArray").MakeGenericMethod(t.GetElementType());
        }

        public static bool HasImplicitConversion(Type fromType, Type toType) {
            if (CompilerHelpers.HasImplicitConversion(fromType, toType, toType.GetMember("op_Implicit"))) {
                return true;
            }

            Type curType = fromType;
            do {
                if (CompilerHelpers.HasImplicitConversion(fromType, toType, curType.GetMember("op_Implicit"))) {
                    return true;
                }
                curType = curType.BaseType;
            } while (curType != null);

            return false;
        }

        public static bool TryImplicitConversion(Object value, Type to, out object result) {
            if (CompilerHelpers.TryImplicitConvert(value, to, to.GetMember("op_Implicit"), out result)) {
                return true;
            }

            Type curType = CompilerHelpers.GetType(value);
            do {
                if (CompilerHelpers.TryImplicitConvert(value, to, curType.GetMember("op_Implicit"), out result)) {
                    return true;
                }
                curType = curType.BaseType;
            } while (curType != null);

            return false;
        }

        private static bool TryImplicitConvert(Object value, Type to, MemberInfo[] implicitConv, out object result) {
            foreach (MethodInfo mi in implicitConv) {
                if (to.IsValueType == mi.ReturnType.IsValueType && to.IsAssignableFrom(mi.ReturnType)) {
                    if (mi.IsStatic) {
                        result = mi.Invoke(null, new object[] { value });
                    } else {
                        result = mi.Invoke(value, ArrayUtils.EmptyObjects);
                    }
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static bool HasImplicitConversion(Type fromType, Type to, MemberInfo[] implicitConv) {
            foreach (MethodInfo mi in implicitConv) {
                if (mi.ReturnType == to && mi.GetParameters()[0].ParameterType.IsAssignableFrom(fromType)) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsStrongBox(object target) {
            Type t = CompilerHelpers.GetType(target);

            return IsStrongBox(t);
        }

        public static bool IsStrongBox(Type t) {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        /// <summary>
        /// Returns a value which indicates failure when a OldConvertToAction of ImplicitTry or
        /// ExplicitTry.
        /// </summary>
        public static Expression GetTryConvertReturnValue(Type type) {
            Expression res;
            if (type.IsInterface || type.IsClass || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                res = Expression.Constant(null, type);
            } else {
                res = Expression.Constant(Activator.CreateInstance(type));
            }

            return res;
        }

        /// <summary>
        /// Returns a value which indicates failure when a ConvertToAction of ImplicitTry or
        /// ExplicitTry.
        /// </summary>
        public static Expression GetTryConvertReturnValue(CodeContext context, RuleBuilder rule) {
            rule.IsError = true;
            return rule.MakeReturn(context.LanguageContext.Binder, GetTryConvertReturnValue(rule.ReturnType));
        }

        public static MethodBase[] GetMethodTargets(object obj) {
            Type t = CompilerHelpers.GetType(obj);

            if (typeof(Delegate).IsAssignableFrom(t)) {
                MethodInfo mi = t.GetMethod("Invoke");
                return new MethodBase[] { mi };
            } else if (typeof(BoundMemberTracker).IsAssignableFrom(t)) {
                BoundMemberTracker bmt = obj as BoundMemberTracker;
                if (bmt.BoundTo.MemberType == TrackerTypes.Method) {
                }
            } else if (typeof(MethodGroup).IsAssignableFrom(t)) {
            } else if (typeof(MemberGroup).IsAssignableFrom(t)) {
            } else {
                return MakeCallSignatureForCallableObject(t);
            }

            return null;
        }

        private static MethodBase[] MakeCallSignatureForCallableObject(Type t) {
            List<MethodBase> res = new List<MethodBase>();
            MemberInfo[] members = t.GetMember("Call");
            foreach (MemberInfo mi in members) {
                if (mi.MemberType == MemberTypes.Method) {
                    MethodInfo method = mi as MethodInfo;
                    if (method.IsSpecialName) {
                        res.Add(method);
                    }
                }
            }
            return res.ToArray();
        }

        public static Type[] GetSiteTypes(IList<Expression> arguments, Type returnType) {
            int count = arguments.Count;

            Type[] ret = new Type[count + 1];

            for (int i = 0; i < count; i++) {
                ret[i] = arguments[i].Type;
            }

            ret[count] = returnType;

            NonNullType.AssertInitialized(ret);
            return ret;
        }

        public static Type[] GetExpressionTypes(Expression[] expressions) {
            ContractUtils.RequiresNotNull(expressions, "expressions");

            Type[] res = new Type[expressions.Length];
            for (int i = 0; i < res.Length; i++) {
                ContractUtils.RequiresNotNull(expressions[i], "expressions[i]");

                res[i] = expressions[i].Type;
            }

            return res;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Type GetReturnType(LambdaExpression lambda) {
            return lambda.Type.GetMethod("Invoke").ReturnType;
        }

        public static Type MakeCallSiteType(params Type[] types) {
            return typeof(CallSite<>).MakeGenericType(DelegateHelpers.MakeDelegate(types));
        }

        public static Type MakeCallSiteDelegateType(Type[] types) {
            return DelegateHelpers.MakeDelegate(types);
        }

        /// <summary>
        /// Creates an interpreted delegate for the lambda.
        /// </summary>
        /// <param name="lambda">The lambda to compile.</param>
        /// <returns>A delegate which can interpret the lambda.</returns>
        public static Delegate LightCompile(this LambdaExpression lambda) {
            return new LightLambda(new LightCompiler().CompileTop(lambda)).MakeDelegate(lambda.Type);
        }

        /// <summary>
        /// Creates an interpreted delegate for the lambda.
        /// </summary>
        /// <typeparam name="T">The lambda's delegate type.</typeparam>
        /// <param name="lambda">The lambda to compile.</param>
        /// <returns>A delegate which can interpret the lambda.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static T LightCompile<T>(this Expression<T> lambda) {
            return (T)(object)LightCompile((LambdaExpression)lambda);
        }

        /// <summary>
        /// Compiles the LambdaExpression.
        /// 
        /// If the lambda is compiled with emitDebugSymbols, it will be
        /// generated into a TypeBuilder. Otherwise, this method is the same as
        /// calling LambdaExpression.Compile()
        /// 
        /// This is a workaround for a CLR limitiation: DynamicMethods cannot
        /// have debugging information.
        /// </summary>
        /// <param name="lambda">the lambda to compile</param>
        /// <param name="emitDebugSymbols">true to generate a debuggable method, false otherwise</param>
        /// <returns>the compiled delegate</returns>
        public static T Compile<T>(this Expression<T> lambda, bool emitDebugSymbols) {
            return emitDebugSymbols ? CompileToMethod(lambda, true) : lambda.Compile();
        }

        /// <summary>
        /// Compiles the LambdaExpression, emitting it into a new type, and
        /// optionally making it debuggable.
        /// 
        /// This is a workaround for a CLR limitiation: DynamicMethods cannot
        /// have debugging information.
        /// </summary>
        /// <param name="lambda">the lambda to compile</param>
        /// <param name="emitDebugSymbols">true to generate a debuggable method, false otherwise</param>
        /// <returns>the compiled delegate</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static T CompileToMethod<T>(Expression<T> lambda, bool emitDebugSymbols) {
            var type = Snippets.Shared.DefineType(lambda.Name, typeof(object), false, emitDebugSymbols).TypeBuilder;
            var rewriter = new BoundConstantsRewriter(type);
            lambda = (Expression<T>)rewriter.Visit(lambda);

            var method = type.DefineMethod(lambda.Name, CompilerHelpers.PublicStatic);
            lambda.CompileToMethod(method, emitDebugSymbols);

            var finished = type.CreateType();

            rewriter.InitializeFields(finished);

            return (T)(object)Delegate.CreateDelegate(lambda.Type, finished.GetMethod(method.Name));
        }

        // Matches ILGen.TryEmitConstant
        internal static bool CanEmitConstant(object value, Type type) {
            if (value == null || CanEmitILConstant(type)) {
                return true;
            }

            Type t = value as Type;
            if (t != null && ILGen.ShouldLdtoken(t)) {
                return true;
            }

            MethodBase mb = value as MethodBase;
            if (mb != null && ILGen.ShouldLdtoken(mb)) {
                return true;
            }

            return false;
        }

        // Matches ILGen.TryEmitILConstant
        internal static bool CanEmitILConstant(Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Reduces the provided DynamicExpression into site.Target(site, *args).
        /// </summary>
        public static Expression Reduce(DynamicExpression node) {
            // Store the callsite as a constant
            var siteConstant = Expression.Constant(CallSite.Create(node.DelegateType, node.Binder));

            // ($site = siteExpr).Target.Invoke($site, *args)
            var site = Expression.Variable(siteConstant.Type, "$site");
            return Expression.Block(
                new[] { site },
                Expression.Call(
                    Expression.Field(
                        Expression.Assign(site, siteConstant),
                        siteConstant.Type.GetField("Target")
                    ),
                    node.DelegateType.GetMethod("Invoke"),
                    ArrayUtils.Insert(site, node.Arguments)
                )
            );
        }

        /// <summary>
        /// Removes all live objects and places them in static fields of a type.
        /// </summary>
        private sealed class BoundConstantsRewriter : ExpressionVisitor {
            private readonly Dictionary<object, FieldBuilder> _fields = new Dictionary<object, FieldBuilder>(ReferenceEqualityComparer<object>.Instance);
            private readonly TypeBuilder _type;

            internal BoundConstantsRewriter(TypeBuilder type) {
                _type = type;
            }

            internal void InitializeFields(Type type) {
                foreach (var pair in _fields) {
                    type.GetField(pair.Value.Name).SetValue(null, pair.Key);
                }
            }

            protected override Expression VisitConstant(ConstantExpression node) {
                if (CanEmitConstant(node.Value, node.Type)) {
                    return node;
                }

                FieldBuilder field;
                if (!_fields.TryGetValue(node.Value, out field)) {
                    field = _type.DefineField(
                        "$constant" + _fields.Count,
                        GetVisibleType(node.Value.GetType()),
                        FieldAttributes.Public | FieldAttributes.Static
                    );
                    _fields.Add(node.Value, field);
                }

                Expression result = Expression.Field(null, field);
                if (result.Type != node.Type) {
                    result = Expression.Convert(result, node.Type);
                }
                return result;
            }

            protected override Expression VisitDynamic(DynamicExpression node) {
                return Visit(Reduce(node));
            }
        }
    }
}
