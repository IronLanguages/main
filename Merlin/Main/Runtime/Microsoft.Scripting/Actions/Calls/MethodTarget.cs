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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// MethodTarget represents how a method is bound to the arguments of the call-site
    /// 
    /// Contrast this with MethodCandidate which represents the logical view of the invocation of a method
    /// </summary>
    public sealed class MethodTarget {
        private MethodBinder _binder;
        private readonly MethodBase _method;
        private int _parameterCount;
        private IList<ArgBuilder> _argBuilders;
        private ArgBuilder _instanceBuilder;
        private ReturnBuilder _returnBuilder;

        internal MethodTarget(MethodBinder binder, MethodBase method, int parameterCount, ArgBuilder instanceBuilder, IList<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
            this._binder = binder;
            this._method = method;
            this._parameterCount = parameterCount;
            this._instanceBuilder = instanceBuilder;
            this._argBuilders = argBuilders;
            this._returnBuilder = returnBuilder;

            //argBuilders.TrimExcess();
        }

        public MethodBinder Binder {
            get { return _binder; }
        }

        public MethodBase Method {
            get { return _method; }
        }

        public int ParameterCount {
            get { return _parameterCount; }
        }

        public Type ReturnType {
            get {
                return _returnBuilder.ReturnType;
            }
        }

        public Type[] GetParameterTypes() {
            List<Type> res = new List<Type>(_argBuilders.Count);
            for (int i = 0; i < _argBuilders.Count; i++) {
                Type t = _argBuilders[i].Type;
                if (t != null) {
                    res.Add(t);
                }
            }

            return res.ToArray();
        }

        public string GetSignatureString(CallTypes callType) {
            StringBuilder buf = new StringBuilder();
            Type[] types = GetParameterTypes();
            if (callType == CallTypes.ImplicitInstance) {
                types = ArrayUtils.RemoveFirst(types);
            }

            string comma = "";
            buf.Append("(");
            foreach (Type t in types) {
                buf.Append(comma);
                buf.Append(t.Name);
                comma = ", ";
            }
            buf.Append(")");
            return buf.ToString();
        }

        [Confined]
        public override string ToString() {
            return string.Format("MethodTarget({0} on {1})", Method, Method.DeclaringType.FullName);
        }

        internal Expression MakeExpression(ParameterBinder parameterBinder, IList<Expression> parameters) {
            bool[] usageMarkers;
            Expression[] spilledArgs;
            Expression[] args = GetArgumentExpressions(parameterBinder, parameters, out usageMarkers, out spilledArgs);
            
            MethodBase mb = Method;
            MethodInfo mi = mb as MethodInfo;
            Expression ret, call;
            if (!mb.IsPublic || (mb.DeclaringType != null && !mb.DeclaringType.IsVisible)) {
                if (mi != null) {
                    mi = CompilerHelpers.GetCallableMethod(mi, _binder._binder.PrivateBinding);
                    if (mi != null) mb = mi;
                }
            }

            ConstructorInfo ci = mb as ConstructorInfo;
            Debug.Assert(mi != null || ci != null);
            if (mb.IsPublic && (mb.DeclaringType == null || mb.DeclaringType.IsVisible)) {
                // public method
                if (mi != null) {
                    Expression instance = mi.IsStatic ? null : _instanceBuilder.ToExpression(parameterBinder, parameters, usageMarkers);
                    call = AstUtils.SimpleCallHelper(instance, mi, args);
                } else {
                    call = AstUtils.SimpleNewHelper(ci, args);
                }
            } else {
                // Private binding, invoke via reflection
                if (mi != null) {
                    Expression instance = mi.IsStatic ? Ast.Constant(null) : _instanceBuilder.ToExpression(parameterBinder, parameters, usageMarkers);
                    Debug.Assert(instance != null, "Can't skip instance expression");

                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeMethod"),
                        Ast.Constant(mi),
                        AstUtils.Convert(instance, typeof(object)),
                        AstUtils.NewArrayHelper(typeof(object), args)
                    );
                } else {
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeConstructor"),
                        Ast.Constant(ci),
                        AstUtils.NewArrayHelper(typeof(object), args)
                    );
                }
            }

            if (spilledArgs != null) {
                call = Expression.Block(spilledArgs.AddLast(call));
            }

            ret = _returnBuilder.ToExpression(parameterBinder, _argBuilders, parameters, call);

            List<Expression> updates = null;
            for (int i = 0; i < _argBuilders.Count; i++) {
                Expression next = _argBuilders[i].UpdateFromReturn(parameterBinder, parameters);
                if (next != null) {
                    if (updates == null) {
                        updates = new List<Expression>();
                    }
                    updates.Add(next);
                }
            }

            if (updates != null) {
                if (ret.Type != typeof(void)) {
                    ParameterExpression temp = Ast.Variable(ret.Type, "$ret");
                    updates.Insert(0, Ast.Assign(temp, ret));
                    updates.Add(temp);
                    ret = Ast.Block(new [] { temp }, updates.ToArray());
                } else {
                    updates.Insert(0, ret);
                    ret = Ast.Convert(
                        Ast.Block(updates.ToArray()),
                        typeof(void)
                    );
                }
            }

            if (parameterBinder.Temps != null) {
                ret = Ast.Block(parameterBinder.Temps, ret);
            }

            return ret;
        }

        private Expression[] GetArgumentExpressions(ParameterBinder parameterBinder, IList<Expression> parameters, out bool[] usageMarkers, out Expression[] spilledArgs) {
            int minPriority = Int32.MaxValue;
            int maxPriority = Int32.MinValue;
            foreach (ArgBuilder ab in _argBuilders) {
                minPriority = System.Math.Min(minPriority, ab.Priority);
                maxPriority = System.Math.Max(maxPriority, ab.Priority);
            }

            var args = new Expression[_argBuilders.Count];
            Expression[] actualArgs = null;
            usageMarkers = new bool[parameters.Count];
            for (int priority = minPriority; priority <= maxPriority; priority++) {
                for (int i = 0; i < _argBuilders.Count; i++) {
                    if (_argBuilders[i].Priority == priority) {
                        args[i] = _argBuilders[i].ToExpression(parameterBinder, parameters, usageMarkers);

                        // see if this has a temp that needs to be passed as the actual argument
                        Expression byref = _argBuilders[i].ByRefArgument;
                        if (byref != null) {
                            if (actualArgs == null) {
                                actualArgs = new Expression[_argBuilders.Count];
                            }
                            actualArgs[i] = byref;
                        }
                    }
                }
            }
            
            if (actualArgs != null) {                
                for (int i = 0, n = args.Length; i < n;  i++) {
                    if (args[i] != null && actualArgs[i] == null) {
                        actualArgs[i] = parameterBinder.GetTemporary(args[i].Type, null);
                        args[i] = Expression.Assign(actualArgs[i], args[i]);
                    }
                }

                spilledArgs = RemoveNulls(args);
                return RemoveNulls(actualArgs);
            }
            
            spilledArgs = null;
            return RemoveNulls(args);
        }

        private static Expression[] RemoveNulls(Expression[] args) {
            int newLength = args.Length;
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == null) {
                    newLength--;
                }
            }
            
            var result = new Expression[newLength];
            for (int i = 0, j = 0; i < args.Length; i++) {
                if (args[i] != null) {
                    result[j++] = args[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a call to this MethodTarget with the specified parameters.  Casts are inserted to force
        /// the types to the provided known types.
        /// 
        /// TODO: Remove RuleBuilder and knownTypes once we're fully meta
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="parameters">The explicit arguments</param>
        /// <param name="knownTypes">If non-null, the type for each element in parameters</param>
        /// <returns></returns>
        internal Expression MakeExpression(ParameterBinder parameterBinder, IList<Expression> parameters, IList<Type> knownTypes) {
            Debug.Assert(knownTypes == null || parameters.Count == knownTypes.Count);

            IList<Expression> args = parameters;
            if (knownTypes != null) {
                args = new Expression[parameters.Count];
                for (int i = 0; i < args.Count; i++) {
                    args[i] = parameters[i];
                    if (knownTypes[i] != null && !knownTypes[i].IsAssignableFrom(parameters[i].Type)) {
                        args[i] = AstUtils.Convert(parameters[i], CompilerHelpers.GetVisibleType(knownTypes[i]));
                    }
                }
            }

            return MakeExpression(parameterBinder, args);
        }

        private static int FindMaxPriority(IList<ArgBuilder> abs, int ceiling) {
            int max = 0;
            foreach (ArgBuilder ab in abs) {
                if (ab.Priority > ceiling) continue;

                max = System.Math.Max(max, ab.Priority);
            }
            return max;
        }

        internal static Candidate CompareEquivalentParameters(MethodTarget one, MethodTarget two) {
            // Prefer normal methods over explicit interface implementations
            if (two.Method.IsPrivate && !one.Method.IsPrivate) return Candidate.One;
            if (one.Method.IsPrivate && !two.Method.IsPrivate) return Candidate.Two;

            // Prefer non-generic methods over generic methods
            if (one.Method.IsGenericMethod) {
                if (!two.Method.IsGenericMethod) {
                    return Candidate.Two;
                } else {
                    //!!! Need to support selecting least generic method here
                    return Candidate.Equivalent;
                }
            } else if (two.Method.IsGenericMethod) {
                return Candidate.One;
            }

            //prefer methods without out params over those with them
            switch (Compare(one._returnBuilder.CountOutParams, two._returnBuilder.CountOutParams)) {
                case 1: return Candidate.Two;
                case -1: return Candidate.One;
            }

            //prefer methods using earlier conversions rules to later ones            
            for (int i = Int32.MaxValue; i >= 0; ) {
                int maxPriorityThis = FindMaxPriority(one._argBuilders, i);
                int maxPriorityOther = FindMaxPriority(two._argBuilders, i);

                if (maxPriorityThis < maxPriorityOther) return Candidate.One;
                if (maxPriorityOther < maxPriorityThis) return Candidate.Two;

                i = maxPriorityThis - 1;
            }

            return Candidate.Equivalent;
        }

        private static int Compare(int x, int y) {
            if (x < y) return -1;
            else if (x > y) return +1;
            else return 0;
        }

        internal MethodTarget MakeParamsExtended(int argCount, SymbolId[] names, int[] nameIndexes) {
            Debug.Assert(BinderHelpers.IsParamsMethod(Method));

            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>(_argBuilders.Count);

            // current argument that we consume, initially skip this if we have it.
            int curArg = CompilerHelpers.IsStatic(_method) ? 0 : 1;
            int kwIndex = -1;
            ArgBuilder paramsDictBuilder = null;

            foreach (ArgBuilder ab in _argBuilders) {
                SimpleArgBuilder sab = ab as SimpleArgBuilder;
                if (sab != null) {
                    // we consume one or more incoming argument(s)
                    if (sab.IsParamsArray) {
                        // consume all the extra arguments
                        int paramsUsed = argCount -
                            GetConsumedArguments() -
                            names.Length +
                            (CompilerHelpers.IsStatic(_method) ? 1 : 0);

                        newArgBuilders.Add(new ParamsArgBuilder(
                            sab.ParameterInfo,
                            sab.Type.GetElementType(),
                            curArg,
                            paramsUsed
                        ));

                        curArg += paramsUsed;
                    } else if (sab.IsParamsDict) {
                        // consume all the kw arguments
                        kwIndex = newArgBuilders.Count;
                        paramsDictBuilder = sab;
                    } else {
                        // consume the argument, adjust its position:
                        newArgBuilders.Add(sab.MakeCopy(curArg++));
                    }
                } else {
                    // CodeContext, null, default, etc...  we don't consume an 
                    // actual incoming argument.
                    newArgBuilders.Add(ab);
                }
            }

            if (kwIndex != -1) {
                newArgBuilders.Insert(kwIndex, new ParamsDictArgBuilder(paramsDictBuilder.ParameterInfo, curArg, names, nameIndexes));
            }

            return new MethodTarget(_binder, Method, argCount, _instanceBuilder, newArgBuilders, _returnBuilder);
        }

        private int GetConsumedArguments() {
            int consuming = 0;
            foreach (ArgBuilder argb in _argBuilders) {
                SimpleArgBuilder sab = argb as SimpleArgBuilder;
                if (sab != null && !sab.IsParamsDict) consuming++;
            }
            return consuming;
        }
    }
}
