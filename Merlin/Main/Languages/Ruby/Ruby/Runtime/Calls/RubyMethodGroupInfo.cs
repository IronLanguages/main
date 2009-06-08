/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;

namespace IronRuby.Runtime.Calls {
    /// <summary>
    /// A group of CLR methods that are treated as a single Ruby method.
    /// </summary>
    public class RubyMethodGroupInfo : RubyMethodGroupBase {
        private MethodBase/*!*/[] _staticDispatchMethods;
        private bool? _hasVirtuals;

        // True: The group contains only static methods and can only be called statically (with no receiver).
        // False: The group contain instance methods and/or extension methods, or operators.
        private readonly bool _isStatic;

        internal RubyMethodGroupInfo(MethodBase/*!*/[]/*!*/ methods, RubyModule/*!*/ declaringModule, bool isStatic)
            : base(methods, RubyMemberFlags.Public, declaringModule) {
            _isStatic = isStatic;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, RubyMemberFlags flags, RubyModule/*!*/ module)
            : base(info.MethodBases, flags, module) {
            _isStatic = info._isStatic;
            _hasVirtuals = info._hasVirtuals;
            _staticDispatchMethods = info._staticDispatchMethods;
            // Note: overloadOwners and maxCachedOverloadLevel are cleared whenever the group is copied.
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, MethodBase/*!*/[] methods)
            : base(methods, info.Flags, info.DeclaringModule) {
            _isStatic = info._isStatic;
            // Note: overloadOwners and maxCachedOverloadLevel are cleared whenever the group is copied.
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyMethodGroupInfo(this, flags, module);
        }

        protected override RubyMemberInfo/*!*/ Copy(MethodBase/*!*/[]/*!*/ methods) {
            return new RubyMethodGroupInfo(this, methods);
        }

        internal override SelfCallConvention CallConvention {
            get { return _isStatic ? SelfCallConvention.NoSelf : SelfCallConvention.SelfIsInstance; }
        }

        internal override bool IsRubyMember {
            get { return false; }
        }

        internal bool IsStatic {
            get { return _isStatic; }
        }

        internal override bool ImplicitProtocolConversions {
            get { return true; }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return ArrayUtils.MakeArray(MethodBases);
        }
        
        #region Static dispatch to virtual methods

        private bool HasVirtuals {
            get {
                if (!_hasVirtuals.HasValue) {
                    if (_isStatic) {
                        _hasVirtuals = false;
                    } else {
                        bool hasVirtuals = false;
                        foreach (MethodBase method in MethodBases) {
                            if (method.IsVirtual) {
                                hasVirtuals = true;
                                break;
                            }
                        }
                        _hasVirtuals = hasVirtuals;
                    }
                }
                return _hasVirtuals.Value;
            }
        }

        protected override MethodBase/*!*/[]/*!*/ GetStaticDispatchMethods(Type/*!*/ baseType, string/*!*/ name) {
            if (!HasVirtuals) {
                return MethodBases;
            }
            if (_staticDispatchMethods == null) {
                _staticDispatchMethods = new MethodBase[MethodBases.Length];
                for (int i = 0; i < MethodBases.Length; i++) {
                    MethodBase method = MethodBases[i];
                    _staticDispatchMethods[i] = method;

                    MethodInfo methodInfo = method as MethodInfo;
                    if (methodInfo != null && methodInfo.IsVirtual) {
                        _staticDispatchMethods[i] = WrapMethod(methodInfo, baseType);
                    }
                }
            }
            return _staticDispatchMethods;
        }

#if DEBUG
        internal const string SuperCallMethodWrapperNameSuffix = "$RubyMethodGroupInfo.SuperCallMethodWrapper";
#else
        internal const string SuperCallMethodWrapperNameSuffix = "";
#endif

        public static DynamicMethod/*!*/ WrapMethod(MethodInfo/*!*/ info, Type/*!*/ associatedType) {
            var originalParams = info.GetParameters();
            var newParams = new Type[originalParams.Length + 1];
            
            newParams[0] = info.DeclaringType;
            for (int i = 0; i < originalParams.Length; i++) {
                newParams[i + 1] = originalParams[i].ParameterType;
            }

            DynamicMethod result = new DynamicMethod(
                RubyExceptionData.EncodeMethodName(null, info.Name, SourceSpan.None) + SuperCallMethodWrapperNameSuffix,
                info.ReturnType, newParams, associatedType
            );

            ILGenerator ilg = result.GetILGenerator();
            if (info.IsAbstract) {
                ilg.Emit(OpCodes.Ldtoken, info);
                ilg.EmitCall(OpCodes.Call, Methods.MakeAbstractMethodCalledError, null);
                ilg.Emit(OpCodes.Throw);
            } else {
                for (int i = 0; i < newParams.Length; i++) {
                    ilg.Emit(OpCodes.Ldarg, i);
                }
                ilg.EmitCall(OpCodes.Call, info, null);
                ilg.Emit(OpCodes.Ret);
            }
            return result;
        }

        #endregion

        #region Dynamic Call

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var visibleOverloads = GetVisibleOverloads(args, MethodBases);
            if (visibleOverloads.Count == 0) {
                metaBuilder.SetError(Methods.MakeClrProtectedMethodCalledError.OpCall(
                    args.MetaContext.Expression, args.MetaTarget.Expression, Ast.Constant(name)
                ));
            } else {
                BuildCallNoFlow(metaBuilder, args, name, visibleOverloads, CallConvention, ImplicitProtocolConversions);
            }
        }

        private IList<MethodBase>/*!*/ GetVisibleOverloads(CallArguments/*!*/ args, IList<MethodBase>/*!*/ overloads) {
            IList<MethodBase> newOverloads = null;
            Debug.Assert(overloads.Count > 0);

            // handle CLR-protected methods:

            // TODO (opt):
            // We might be able to cache the callable overloads in a MethodGroup. 
            // However, the _overloadOwners of that group would need to point to the original overload owners, not the current class, in order
            // to preserve semantics of overload deletion/redefinition (deletion of the protected overload would need to imply deletion
            // of the correpsondig public overload in the cached MethodGroup).
            if (!args.RubyContext.DomainManager.Configuration.PrivateBinding) {
                Type underlyingType = null;
                BindingFlags bindingFlags = 0;

                for (int i = 0; i < overloads.Count; i++) {
                    var overload = overloads[i];
                    if (overload.IsProtected()) {
                        if (newOverloads == null) {
                            newOverloads = CollectionUtils.GetRange(overloads, 0, i);

                            RubyClass cls = args.Target as RubyClass;
                            if (cls != null) {
                                // target is a non-singleton class, look for the methods on its underlying type if it is Ruby class.
                                bindingFlags = BindingFlags.Static;
                            } else {
                                // use the first non-singleton class:
                                cls = args.TargetClass.GetNonSingletonClass();
                                bindingFlags = BindingFlags.Instance;
                            }

                            if (cls.IsRubyClass && !cls.IsSingletonClass) {
                                underlyingType = cls.GetUnderlyingSystemType();
                            }
                        }

                        if (underlyingType != null) {
                            // TODO (opt): we can define a method on the emitted type that does this more efficently:
                            Type[] genericArguments = overload.IsGenericMethod ? overload.GetGenericArguments() : null;

                            MethodInfo visibleMethod = GetMethodOverload(
                                ReflectionUtils.GetParameterTypes(overload.GetParameters()), 
                                genericArguments,
                                underlyingType,
                                ClsTypeEmitter.BaseMethodPrefix + overload.Name, 
                                BindingFlags.Public | bindingFlags | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod
                            );

                            Debug.Assert(visibleMethod != null);

                            newOverloads.Add(visibleMethod);
                        }
                    } else if (newOverloads != null) {
                        newOverloads.Add(overload);
                    }
                }
            }

            return newOverloads ?? overloads;
        }

        private static MethodInfo/*!*/ GetMethodOverload(Type/*!*/[]/*!*/ parameterTypes, Type/*!*/[] genericParameterTypes, 
            Type/*!*/ type, string/*!*/ name, BindingFlags bindingFlags) {

            var overloads = type.GetMember(name, MemberTypes.Method, bindingFlags);
            for (int i = 0; i < overloads.Length; i++) {
                MethodInfo overload = (MethodInfo)overloads[i];
                if ((genericParameterTypes != null) != overload.IsGenericMethod) {
                    continue;
                }

                if (overload.IsGenericMethod) {
                    if (overload.GetGenericArguments().Length != genericParameterTypes.Length) {
                        continue;
                    }
                    overload = overload.MakeGenericMethod(genericParameterTypes);
                }

                if (ReflectionUtils.GetParameterTypes(overload.GetParameters()).ValueEquals(parameterTypes)) {
                    return overload;
                }
            }
            return null;
        }

        #endregion
    }
}

