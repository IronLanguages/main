/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public enum SelfCallConvention {
        SelfIsInstance,
        SelfIsParameter,
        NoSelf
    }

    /// <summary>
    /// Performs method binding for calling CLR methods.
    /// Currently this is used for all builtin libary methods and interop calls to CLR methods
    /// </summary>
    public abstract class RubyMethodGroupBase : RubyMemberInfo {
        // Not protected by a lock. Immutable after initialized. 
        private OverloadInfo/*!*/[] _methodBases;

        protected RubyMethodGroupBase(OverloadInfo/*!*/[] methods, RubyMemberFlags flags, RubyModule/*!*/ declaringModule)
            : base(flags, declaringModule) {
            if (methods != null) {
                SetMethodBasesNoLock(methods);
            }
        }

        protected abstract RubyMemberInfo/*!*/ Copy(OverloadInfo/*!*/[]/*!*/ methods);

        internal protected virtual OverloadInfo/*!*/[]/*!*/ MethodBases {
            get { return _methodBases; }
        }

        internal OverloadInfo/*!*/[]/*!*/ SetMethodBasesNoLock(OverloadInfo/*!*/[]/*!*/ methods) {
            Debug.Assert(
                CollectionUtils.TrueForAll(methods, (method) => method.IsStatic || method.DeclaringType == typeof(Object)) ||
                CollectionUtils.TrueForAll(methods, (method) => !method.IsStatic || method.IsExtension || RubyUtils.IsOperator(method))
            );

            return _methodBases = methods;
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return ArrayUtils.ToArray(MethodBases, (o) => (MemberInfo)o.ReflectionInfo);
        }

        internal abstract SelfCallConvention CallConvention { get; }
        internal abstract bool ImplicitProtocolConversions { get; }

        public override int GetArity() {
            int minParameters = Int32.MaxValue;
            int maxParameters = 0;
            bool hasOptional = false;
            foreach (OverloadInfo method in MethodBases) {
                int mandatory, optional;
                RubyOverloadResolver.GetParameterCount(method, CallConvention, out mandatory, out optional);
                if (mandatory < minParameters) {
                    minParameters = mandatory;
                }
                if (mandatory > maxParameters) {
                    maxParameters = mandatory;
                }
                if (!hasOptional && optional > 0) {
                    hasOptional = true;
                }
            }
            if (hasOptional || maxParameters > minParameters) {
                return -minParameters - 1;
            } else {
                return minParameters;
            }
        }

        #region Generic Parameters, Overloads Selection

        public override RubyMemberInfo TryBindGenericParameters(Type/*!*/[]/*!*/ typeArguments) {
            var boundMethods = new List<OverloadInfo>();
            foreach (OverloadInfo method in MethodBases) {
                if (method.IsGenericMethodDefinition) {
                    if (typeArguments.Length == method.GenericArguments.Count) {
                        boundMethods.Add(method.MakeGenericMethod(typeArguments));
                    }
                } else if (typeArguments.Length == 0) {
                    boundMethods.Add(method);
                }
            }

            if (boundMethods.Count == 0) {
                return null;
            }

            return Copy(boundMethods.ToArray());
        }

        /// <summary>
        /// Filters out methods that don't exactly match parameter types except for hidden parameters (RubyContext, RubyScope, site local storage).
        /// </summary>
        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            var boundMethods = new List<OverloadInfo>();
            foreach (var method in MethodBases) {
                if (IsOverloadSignature(method, parameterTypes)) {
                    boundMethods.Add(method);
                }
            }

            if (boundMethods.Count == 0) {
                return null;
            }

            return Copy(boundMethods.ToArray());
        }

        private bool IsOverloadSignature(OverloadInfo/*!*/ method, Type/*!*/[]/*!*/ parameterTypes) {
            int firstInfo = RubyOverloadResolver.GetHiddenParameterCount(method, CallConvention);
            var infos = method.Parameters;
            
            if (infos.Count - firstInfo != parameterTypes.Length) {
                return false;
            }

            for (int i = 0; i < parameterTypes.Length; i++) {
                if (infos[firstInfo + i].ParameterType != parameterTypes[i]) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Dynamic Sites

        private static Type/*!*/ GetAssociatedSystemType(RubyModule/*!*/ module) {
            if (module.IsClass) {
                Type type = ((RubyClass)module).GetUnderlyingSystemType();
                if (type != null) {
                    return type;
                }
            }
            return typeof(SuperCallAction);
        }

        internal override void BuildSuperCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            Assert.NotNull(declaringModule, metaBuilder, args);
            var visibleOverloads = GetVisibleOverloads(args, MethodBases, true);
            if (visibleOverloads.Count == 0) {
                metaBuilder.SetError(Methods.MakeClrVirtualMethodCalledError.OpCall(
                    args.MetaContext.Expression, args.MetaTarget.Expression, Ast.Constant(name)
                ));
            } else {
                BuildCallNoFlow(metaBuilder, args, name, visibleOverloads, CallConvention, ImplicitProtocolConversions);
            }
        }

        internal static IList<OverloadInfo>/*!*/ GetVisibleOverloads(CallArguments/*!*/ args, IList<OverloadInfo>/*!*/ overloads, bool isSuperCall) {
            IList<OverloadInfo> newOverloads = null;
            Debug.Assert(overloads.Count > 0);

            // handle CLR-protected and virtual methods:

            // TODO (opt):
            // We might be able to cache the callable overloads in a MethodGroup. 
            // However, the _overloadOwners of that group would need to point to the original overload owners, not the current class, in order
            // to preserve semantics of overload deletion/redefinition (deletion of the protected overload would need to imply deletion
            // of the correpsondig public overload in the cached MethodGroup).
            if (isSuperCall || !args.RubyContext.DomainManager.Configuration.PrivateBinding) {
                Type underlyingType = null;
                BindingFlags bindingFlags = 0;

                for (int i = 0; i < overloads.Count; i++) {
                    var overload = overloads[i];
                    if ((isSuperCall && overload.IsVirtual && !overload.IsFinal) || overload.IsProtected) {
                        if (newOverloads == null) {
                            newOverloads = CollectionUtils.GetRange(overloads, 0, i);

                            RubyClass cls;
                            IRubyType rt = args.Target as IRubyType;
                            if (rt != null) {
                                bindingFlags = BindingFlags.Instance;
                                underlyingType = args.Target.GetType();
                            } else if ((cls = args.Target as RubyClass) != null && cls.IsRubyClass && !cls.IsSingletonClass) {
                                bindingFlags = BindingFlags.Static;
                                underlyingType = cls.GetUnderlyingSystemType();
                            }
                        }

                        if (underlyingType != null) {
                            // TODO (opt): we can define a method on the emitted type that does this more efficently:
                            IList<Type> genericArguments = overload.IsGenericMethod ? overload.GenericArguments : null;

                            OverloadInfo visibleMethod = GetMethodOverload(
                                ArrayUtils.ToArray(overload.Parameters, (pi) => pi.ParameterType),
                                genericArguments,
                                underlyingType,
                                ClsTypeEmitter.BaseMethodPrefix + overload.Name,
                                BindingFlags.Public | bindingFlags | BindingFlags.InvokeMethod
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

        private static OverloadInfo/*!*/ GetMethodOverload(Type/*!*/[]/*!*/ parameterTypes, IList<Type/*!*/> genericParameterTypes,
            Type/*!*/ type, string/*!*/ name, BindingFlags bindingFlags) {

            var overloads = type.GetMember(name, MemberTypes.Method, bindingFlags);
            for (int i = 0; i < overloads.Length; i++) {
                MethodInfo overload = (MethodInfo)overloads[i];
                MethodInfo originalOverload = overload;
                if ((genericParameterTypes != null) != overload.IsGenericMethod) {
                    continue;
                }

                if (overload.IsGenericMethod) {
                    if (overload.GetGenericArguments().Length != genericParameterTypes.Count) {
                        continue;
                    }
                    overload = overload.MakeGenericMethod(ArrayUtils.ToArray(genericParameterTypes));
                }

                if (ArrayUtils.ToArray(overload.GetParameters(), (pi) => pi.ParameterType).ValueEquals(parameterTypes)) {
                    return new ReflectionOverloadInfo(originalOverload);
                }
            }
            return null;
        }

        internal static BindingTarget/*!*/ ResolveOverload(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name,
            IList<OverloadInfo>/*!*/ overloads, SelfCallConvention callConvention, bool implicitProtocolConversions, 
            out RubyOverloadResolver/*!*/ resolver) {

            resolver = new RubyOverloadResolver(metaBuilder, args, callConvention, implicitProtocolConversions);
            var bindingTarget = resolver.ResolveOverload(name, overloads, NarrowingLevel.None, NarrowingLevel.All);

            bool calleeHasBlockParam = bindingTarget.Success && HasBlockParameter(bindingTarget.Overload);
            
            // At runtime the BlockParam is created with a new RFC instance that identifies the library method frame as 
            // a proc-converter target of a method unwinder triggered by break from a block.
            if (args.Signature.HasBlock && calleeHasBlockParam) {
                metaBuilder.ControlFlowBuilder = RuleControlFlowBuilder;
            }

            // add restrictions used for overload resolution:
            resolver.AddArgumentRestrictions(metaBuilder, bindingTarget);
            
            return bindingTarget;
        }

        /// <summary>
        /// Resolves an library method overload and builds call expression.
        /// The resulting expression on meta-builder doesn't handle block control flow yet.
        /// </summary>
        internal static void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name,
            IList<OverloadInfo>/*!*/ overloads, SelfCallConvention callConvention, bool implicitProtocolConversions) {

            RubyOverloadResolver resolver;
            var bindingTarget = ResolveOverload(metaBuilder, args, name, overloads, callConvention, implicitProtocolConversions, out resolver);
            if (bindingTarget.Success) {
                // TODO: create a custom overload info:
                if (ReferenceEquals(bindingTarget.Overload.ReflectionInfo, Methods.CreateDefaultInstance)) {
                    Debug.Assert(args.TargetClass.TypeTracker.Type.IsValueType);
                    metaBuilder.Result = Ast.New(args.TargetClass.TypeTracker.Type);
                } else if (args.Signature.IsVirtualCall && bindingTarget.Overload.IsVirtual) {
                    // Virtual methods that have been detached from the CLR type and 
                    // defined on the corresponding Ruby class or its subclass are not
                    // directly invoked from a dynamic virtual call to prevent recursion.
                    // Instead the base call is performed. 
                    //
                    // Example:
                    // class C < ArrayList           
                    //   define_method(:Add, instance_method(:Add))          
                    // end
                    // 
                    // C.new.Add(1)
                    // 
                    // C.new.Add dispatches to the virtual ArrayList::Add, which in turn dispatches to the auto-generated override C$1::Add.
                    // That gets here since the defined method is a Ruby method (a detached CLR method group). If we called it directly
                    // it would invoke the C$1::Add override again leading to a stack overflow. So we need to use a base call instead.
                    metaBuilder.Result = Ast.Field(null, Fields.ForwardToBase);
                } else {
                    metaBuilder.Result = bindingTarget.MakeExpression();
                }
            } else {
                metaBuilder.SetError(resolver.MakeInvalidParametersError(bindingTarget).Expression);
            }
        }

        /// <summary>
        /// Takes current result and wraps it into try-filter(MethodUnwinder)-finally block that ensures correct "break" behavior for 
        /// library method calls with block given in bfcVariable (BlockParam).
        /// </summary>
        public static void RuleControlFlowBuilder(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (metaBuilder.Error) {
                return;
            }

            var metaBlock = args.GetMetaBlock();
            Debug.Assert(metaBlock != null, "RuleControlFlowBuilder should only be used if the signature has a block");
            
            // We construct CF only for non-nil blocks thus we need a test for it:
            if (metaBlock.Value == null) {
                metaBuilder.AddRestriction(Ast.Equal(metaBlock.Expression, AstUtils.Constant(null)));
                return;
            }

            // don't need to test the exact type of the Proc since the code is subclass agnostic:
            metaBuilder.AddRestriction(Ast.NotEqual(metaBlock.Expression, AstUtils.Constant(null)));
            Expression bfcVariable = metaBuilder.BfcVariable;
            Debug.Assert(bfcVariable != null);
            
            // Method call with proc can invoke control flow that returns an arbitrary value from the call, so we need to type result to Object.
            // Otherwise, the result could only be result of targetExpression unless its return type is void.
            Expression resultVariable = metaBuilder.GetTemporary(typeof(object), "#result");
            ParameterExpression unwinder;

            metaBuilder.Result = Ast.Block(
                Ast.Assign(bfcVariable, Methods.CreateBfcForLibraryMethod.OpCall(AstUtils.Convert(args.GetBlockExpression(), typeof(Proc)))),
                AstUtils.Try(
                    Ast.Assign(resultVariable, AstUtils.Convert(metaBuilder.Result, typeof(object)))
                ).Filter(unwinder = Ast.Parameter(typeof(MethodUnwinder), "#unwinder"),
                    Methods.IsProcConverterTarget.OpCall(bfcVariable, unwinder),

                    Ast.Assign(resultVariable, Ast.Field(unwinder, MethodUnwinder.ReturnValueField)),
                    AstUtils.Default(typeof(object))
                ).Finally(
                    Methods.LeaveProcConverter.OpCall(bfcVariable)
                ),
                resultVariable
            );
        }

        private static bool HasBlockParameter(OverloadInfo/*!*/ method) {
            foreach (ParameterInfo param in method.Parameters) {
                if (param.ParameterType == typeof(BlockParam)) {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}

