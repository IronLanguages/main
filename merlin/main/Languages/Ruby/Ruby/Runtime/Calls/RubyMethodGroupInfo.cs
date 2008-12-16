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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;

using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    
    /// <summary>
    /// Performs method binding for calling CLR methods.
    /// Currently this is used for all builtin libary methods and interop calls to CLR methods
    /// </summary>
    public sealed class RubyMethodGroupInfo : RubyMemberInfo {
        private readonly Delegate/*!*/[]/*!*/ _overloads;
        private IList<MethodBase> _methodBases;
        private IList<MethodBase> _staticDispatchMethods;
        private bool? _hasVirtuals;

        // remove call site type object (CLR static methods don't accept self type):
        private readonly bool _isClrStatic;
        private readonly bool _isRubyMethod;

        public bool IsRubyMethod {
            get { return _isRubyMethod; }
        }

        private IList<MethodBase>/*!*/ MethodBases {
            get {
                if (_methodBases == null) {
                    var result = new MethodBase[_overloads.Length];
                    for (int i = 0; i < _overloads.Length; i++) {
                        result[i] = _overloads[i].Method;
                    }
                    _methodBases = result;
                }

                // either all methods in the group are static or instance, a mixture is not allowed:
                Debug.Assert(
                    CollectionUtils.TrueForAll(_methodBases, (method) => method.IsStatic) ||
                    CollectionUtils.TrueForAll(_methodBases, (method) => !method.IsStatic)
                );

                return _methodBases;
            }
        }

        /// <summary>
        /// Creates a Ruby method implemented by a method group of CLR methods.
        /// </summary>
        internal RubyMethodGroupInfo(Delegate/*!*/[]/*!*/ overloads, RubyMemberFlags flags,  
            RubyModule/*!*/ declaringModule)
            : base(flags, declaringModule) {
            Assert.NotNullItems(overloads);
            Assert.NotNull(declaringModule);
            _overloads = overloads;

            _isClrStatic = false;
            _isRubyMethod = true;
        }

        /// <summary>
        /// Creates a CLR method group.
        /// </summary>
        internal RubyMethodGroupInfo(IList<MethodBase/*!*/>/*!*/ methods, RubyModule/*!*/ declaringModule, bool isStatic)
            : base(RubyMemberFlags.Public, declaringModule) {
            Assert.NotNull(methods, declaringModule);
            _methodBases = methods;
            _isClrStatic = isStatic;
            _isRubyMethod = false;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, RubyMemberFlags flags, RubyModule/*!*/ module)
            : base(flags, module) {
            _methodBases = info._methodBases;
            _overloads = info._overloads;
            _isRubyMethod = info._isRubyMethod;
            _isClrStatic = info._isClrStatic;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyMethodGroupInfo(this, flags, module);
        }

        public override int Arity {
            get {
                int minParameters = Int32.MaxValue;
                int maxParameters = 0;
                bool hasOptional = false;
                foreach (MethodBase method in MethodBases) {
                    int mandatory, optional;
                    bool acceptsBlock;
                    RubyBinder.GetParameterCount(method.GetParameters(), out mandatory, out optional, out acceptsBlock);
                    if (mandatory > 0) {
                        mandatory--; // account for "self"
                    }
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
        }

        #region Static dispatch to virtual methods

        private bool HasVirtuals {
            get {
                if (!_hasVirtuals.HasValue) {
                    if (_isClrStatic) {
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

        private IList<MethodBase>/*!*/ GetStaticDispatchMethods(Type/*!*/ baseType, string/*!*/ name) {
            if (!HasVirtuals) {
                return MethodBases;
            }
            if (_staticDispatchMethods == null) {
                _staticDispatchMethods = new MethodBase[MethodBases.Count];
                for (int i = 0; i < MethodBases.Count; i++) {
                    MethodBase method = MethodBases[i];
                    _staticDispatchMethods[i] = method;

                    MethodInfo methodInfo = (method as MethodInfo);
                    if (methodInfo != null && methodInfo.IsVirtual) {
                        _staticDispatchMethods[i] = WrapMethod(methodInfo, baseType);
                    }
                }
            }
            return _staticDispatchMethods;
        }

        public static DynamicMethod/*!*/ WrapMethod(MethodInfo/*!*/ info, Type/*!*/ associatedType) {
            var originalParams = info.GetParameters();
            var newParams = new Type[originalParams.Length + 1];
            string name = "";
            newParams[0] = info.DeclaringType;
            for (int i = 0; i < originalParams.Length; i++) {
                newParams[i + 1] = originalParams[i].ParameterType;
            }
            DynamicMethod result = new DynamicMethod(name, info.ReturnType, newParams, associatedType);
            ILGenerator ilg = result.GetILGenerator();
            for (int i = 0; i < newParams.Length; i++) {
                ilg.Emit(OpCodes.Ldarg, i);
            }
            ilg.EmitCall(OpCodes.Call, info, null);
            ilg.Emit(OpCodes.Ret);
            return result;
        }

        #endregion

        #region Dynamic Sites

        private static Type GetAssociatedSystemType(RubyModule/*!*/ module) {
            if (module.IsClass) {
                RubyClass cls = (module as RubyClass);
                Type type = cls.GetUnderlyingSystemType();
                if (type != null) {
                    return type;
                }
            }
            return typeof(SuperCallAction);
        }

        internal override void BuildSuperCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            Assert.NotNull(declaringModule, metaBuilder, args);

            IList<MethodBase> methods;
            if (!declaringModule.IsSingletonClass) {
                Type associatedType = GetAssociatedSystemType(declaringModule);
                methods = GetStaticDispatchMethods(associatedType, name);
            } else {
                methods = MethodBases;
            }

            BuildCallNoFlow(metaBuilder, args, name, methods, !_isClrStatic, !_isRubyMethod);
        }

        internal static BindingTarget/*!*/ ResolveOverload(string/*!*/ name, IList<MethodBase>/*!*/ overloads, CallArguments/*!*/ args, 
            bool includeSelf, bool selfIsInstance) {

            var methodBinder = MethodBinder.MakeBinder(args.RubyContext.Binder, name, overloads, SymbolId.EmptySymbols, NarrowingLevel.None, NarrowingLevel.All);
            var argTypes = GetSignatureToMatch(args, includeSelf, selfIsInstance);
            return methodBinder.MakeBindingTarget(CallTypes.None, argTypes);
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            Assert.NotNull(name, metaBuilder, args);

            BuildCallNoFlow(metaBuilder, args, name, MethodBases, !_isClrStatic, !_isRubyMethod);
        }

        /// <summary>
        /// Resolves an library method overload and builds call expression.
        /// The resulting expression on meta-builder doesn't handle block control flow yet.
        /// </summary>
        internal static void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, 
            IList<MethodBase>/*!*/ overloads, bool includeSelf, bool selfIsInstance) {

            var bindingTarget = ResolveOverload(name, overloads, args, includeSelf, selfIsInstance);
            if (bindingTarget.Success) {
                bool calleeHasBlockParam = HasBlockParameter(bindingTarget.Method);

                // Allocates a variable holding BlockParam. At runtime the BlockParam is created with a new RFC instance that
                // identifies the library method frame as a proc-converter target of a method unwinder triggered by break from a block.
                //
                // NOTE: We check for null block here -> test fore that fact is added in MakeActualArgs
                if (metaBuilder.BfcVariable == null && args.Signature.HasBlock && args.GetBlock() != null && calleeHasBlockParam) {
                    metaBuilder.BfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
                }

                var actualArgs = MakeActualArgs(metaBuilder, args, includeSelf, selfIsInstance, calleeHasBlockParam, true);
                var parameterBinder = new RubyParameterBinder(args.RubyContext.Binder, args.MetaContext.Expression, args.Signature.HasScope);
                var targetExpression = bindingTarget.MakeExpression(parameterBinder, actualArgs);

                metaBuilder.Result = targetExpression;
            } else if (bindingTarget.Result == BindingResult.AmbiguousMatch) {
                metaBuilder.SetError(
                    Methods.MakeAmbiguousMatchError.OpCall(Ast.Constant(name))
                );
            } else {
                metaBuilder.SetError(
                    Methods.MakeInvalidArgumentTypesError.OpCall(Ast.Constant(name))
                );
            }
        }

        internal override void ApplyBlockFlowHandling(MetaObjectBuilder metaBuilder, CallArguments args) {
            ApplyBlockFlowHandlingInternal(metaBuilder, args);
        }

        /// <summary>
        /// Takes current result and wraps it into try-filter(MethodUnwinder)-finally block that ensures correct "break" behavior for 
        /// library method calls with block given in bfcVariable (BlockParam).
        /// </summary>
        internal static void ApplyBlockFlowHandlingInternal(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (metaBuilder.Error) {
                return;
            }

            Expression expression = metaBuilder.Result;
            Expression bfcVariable = metaBuilder.BfcVariable;

            // Method call with proc can invoke control flow that returns an arbitrary value from the call, so we need to type result to Object.
            // Otherwise, the result could only be result of targetExpression unless its return type is void.
            Type resultType = (bfcVariable != null) ? typeof(object) : expression.Type;

            Expression resultVariable;
            if (resultType != typeof(void)) {
                resultVariable = metaBuilder.GetTemporary(resultType, "#result");
            } else {
                resultVariable = Expression.Empty();
            }

            if (expression.Type != typeof(void)) {
                expression = Ast.Assign(resultVariable, AstUtils.Convert(expression, resultType));
            }

            // a non-null proc is being passed to the callee:
            if (bfcVariable != null) {
                ParameterExpression methodUnwinder = metaBuilder.GetTemporary(typeof(MethodUnwinder), "#unwinder");

                expression = AstFactory.Block(
                    Ast.Assign(bfcVariable, Methods.CreateBfcForLibraryMethod.OpCall(AstUtils.Convert(args.GetBlockExpression(), typeof(Proc)))),
                    AstUtils.Try(
                        expression
                    ).Filter(methodUnwinder, Methods.IsProcConverterTarget.OpCall(bfcVariable, methodUnwinder),
                        Ast.Assign(resultVariable, Ast.Field(methodUnwinder, MethodUnwinder.ReturnValueField)),
                        Expression.Default(expression.Type)
                    ).Finally(
                        Methods.LeaveProcConverter.OpCall(bfcVariable)
                    ),
                    resultVariable
                );
            }

            metaBuilder.Result = expression;
        }

        private static bool HasBlockParameter(MethodBase/*!*/ method) {
            foreach (ParameterInfo param in method.GetParameters()) {
                if (param.ParameterType == typeof(BlockParam)) {
                    return true;
                }
            }
            return false;
        }

        public static Expression[]/*!*/ MakeActualArgs(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args,
            bool includeSelf, bool selfIsInstance, bool calleeHasBlockParam, bool injectMissingBlockParam) {

            var actualArgs = new List<Expression>(args.ExplicitArgumentCount);

            // self (instance):
            if (includeSelf && selfIsInstance) {
                // test already added by method resolver
                Debug.Assert(args.TargetExpression != null);
                AddArgument(actualArgs, args.Target, args.TargetExpression);
            }

            Proc block = null;
            Expression blockExpression = null;

            // block test - we need to test for a block regardless of whether it is actually passed to the method or not
            // since the information that the block is not null is used for overload resolution.
            if (args.Signature.HasBlock) {
                block = args.GetBlock();
                blockExpression = args.GetBlockExpression();

                if (block == null) {
                    metaBuilder.AddRestriction(Ast.Equal(blockExpression, Ast.Constant(null)));
                } else {
                    // don't need to test the exact type of the Proc since the code is subclass agnostic:
                    metaBuilder.AddRestriction(Ast.NotEqual(blockExpression, Ast.Constant(null)));
                }
            }

            // block:
            if (calleeHasBlockParam) {
                if (args.Signature.HasBlock) {
                    if (block == null) {
                        // the user explicitly passed nil as a block arg:
                        actualArgs.Add(Ast.Constant(null));
                    } else {
                        // pass BlockParam:
                        Debug.Assert(metaBuilder.BfcVariable != null);
                        actualArgs.Add(metaBuilder.BfcVariable);
                    }
                } else {
                    // no block passed into a method with a BlockParam:
                    actualArgs.Add(Ast.Constant(null));
                }
            } else if (injectMissingBlockParam) {
                // no block passed into a method w/o a BlockParam (we still need to fill the missing block argument):
                actualArgs.Add(Ast.Constant(null));
            }

            // self (non-instance):
            if (includeSelf && !selfIsInstance) {
                // test already added by method resolver
                AddArgument(actualArgs, args.Target, args.TargetExpression);
            }

            // simple arguments:
            for (int i = 0; i < args.SimpleArgumentCount; i++) {
                var value = args.GetSimpleArgument(i);
                var expr = args.GetSimpleArgumentExpression(i);

                metaBuilder.AddObjectTypeRestriction(value, expr);
                AddArgument(actualArgs, value, expr);
            }

            // splat argument:
            int listLength;
            ParameterExpression listVariable;
            if (args.Signature.HasSplattedArgument) {
                object splattedArg = args.GetSplattedArgument();
                Expression splattedArgExpression = args.GetSplattedArgumentExpression();

                if (metaBuilder.AddSplattedArgumentTest(splattedArg, splattedArgExpression, out listLength, out listVariable)) {

                    // AddTestForListArg only returns 'true' if the argument is a List<object>
                    var list = (List<object>)splattedArg;

                    // get arguments, add tests
                    for (int j = 0; j < listLength; j++) {
                        var value = list[j];
                        var expr = Ast.Call(listVariable, typeof(List<object>).GetMethod("get_Item"), Ast.Constant(j));

                        metaBuilder.AddObjectTypeCondition(value, expr);
                        AddArgument(actualArgs, value, expr);
                    }

                } else {
                    // argument is not an array => add the argument itself:
                    AddArgument(actualArgs, splattedArg, splattedArgExpression);
                }
            }

            // rhs argument:
            if (args.Signature.HasRhsArgument) {
                var value = args.GetRhsArgument();
                var expr = args.GetRhsArgumentExpression();

                metaBuilder.AddObjectTypeRestriction(value, expr);
                AddArgument(actualArgs, value, expr);
            }

            return actualArgs.ToArray();
        }

        private static void AddArgument(List<Expression>/*!*/ actualArgs, object arg, Expression/*!*/ expr) {
            if (arg == null) {
                actualArgs.Add(Ast.Constant(null));
            } else {
                var type = CompilerHelpers.GetVisibleType(arg);
                if (type.IsValueType) {
                    actualArgs.Add(expr);
                } else {
                    actualArgs.Add(AstUtils.Convert(expr, type));
                }
            }
        }

        private static Type[]/*!*/ GetSignatureToMatch(CallArguments/*!*/ args, bool includeSelf, bool selfIsInstance) {
            var result = new List<Type>(args.ExplicitArgumentCount);

            // self (instance):
            if (includeSelf && selfIsInstance) {
                result.Add(CompilerHelpers.GetType(args.Target));
            }

            // block:
            if (args.Signature.HasBlock) {
                // use None to let binder know that [NotNull]BlockParam is not applicable
                result.Add(args.GetBlock() != null ? typeof(BlockParam) : typeof(DynamicNull));
            } else {
                result.Add(typeof(MissingBlockParam));
            }

            // self (non-instance):
            if (includeSelf && !selfIsInstance) {
                result.Add(CompilerHelpers.GetType(args.Target));
            }

            // simple args:
            for (int i = 0; i < args.SimpleArgumentCount; i++) {
                result.Add(CompilerHelpers.GetType(args.GetSimpleArgument(i)));
            }

            // splat arg:
            if (args.Signature.HasSplattedArgument) {
                object splattedArg = args.GetSplattedArgument();
                
                var list = splattedArg as List<object>;
                if (list != null) {
                    foreach (object obj in list) {
                        result.Add(CompilerHelpers.GetType(obj));
                    }
                } else {
                    result.Add(CompilerHelpers.GetType(splattedArg));
                }
            }

            // rhs arg:
            if (args.Signature.HasRhsArgument) {
                result.Add(CompilerHelpers.GetType(args.GetRhsArgument()));
            }

            return result.ToArray();
        }

        #endregion
    }
}

