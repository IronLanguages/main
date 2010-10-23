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
using System.Dynamic;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public sealed class RubyOverloadResolver : OverloadResolver {
        private readonly CallArguments/*!*/ _args;
        private readonly MetaObjectBuilder/*!*/ _metaBuilder;
        private readonly SelfCallConvention _callConvention;

        //
        // We want to perform Ruby protocol conversions when binding to CLR methods. 
        // However, some Ruby libraries don't allow protocol conversions on their parameters. 
        // Hence in libraries protocol conversions should only be performed when DefaultProtocol attribute is present.
        // This flag is set if protocol conversions should be applied on all parameters regardless of DefaultProtocol attribute.
        // This also implies a different narrowing level of the protocol conversions for library methods and other CLR methods 
        // (see Converter.CanConvertFrom).
        //
        private readonly bool _implicitProtocolConversions;

        private int _firstRestrictedArg;
        private int _lastSplattedArg;
        private ParameterExpression _listVariable;
        private IList _list;

        // An optional list of assumptions upon arguments that were made during resolution.
        private List<Key<int, NarrowingLevel, Expression>> _argumentAssumptions;

        internal RubyContext/*!*/ Context {
            get { return _args.RubyContext; }
        }

        internal Expression/*!*/ ScopeExpression {
            get { return _args.MetaScope.Expression; }
        }

        internal Expression/*!*/ ContextExpression {
            get { return _args.MetaContext.Expression; }
        }

        internal RubyOverloadResolver(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, SelfCallConvention callConvention,
            bool implicitProtocolConversions)
            : base(args.RubyContext.Binder) {
            _args = args;
            _metaBuilder = metaBuilder;
            _callConvention = callConvention;
            _implicitProtocolConversions = implicitProtocolConversions;
        }

        #region Step 1: Special Parameters

        protected override bool AllowMemberInitialization(OverloadInfo method) {
            return false;
        }

        /// <summary>
        /// We expand params arrays for library methods. Splat operator needs to be used to pass content of an array/list into params array method.
        /// </summary>
        protected override bool BindToUnexpandedParams(MethodCandidate/*!*/ candidate) {
            // TODO: separate flag?
            return _implicitProtocolConversions;
        }

        protected override BitArray MapSpecialParameters(ParameterMapping/*!*/ mapping) {
            var method = mapping.Overload;
            var infos = method.Parameters;
            var special = new BitArray(infos.Count);

            // Method signatures                                                                                  SelfCallConvention
            // RubyMethod/RubyCtor:   [(CallSiteStorage)*, (RubyContext|RubyScope)?, (BlockParam)?, self, args]  SelfIsParameter
            // static:                [(CallSiteStorage)*, (RubyContext|RubyScope)?, (BlockParam)?, args]        NoSelf
            // instance/extension/op: [self, (CallSiteStorage)*, (RubyContext|RubyScope)?, (BlockParam)?, args]  SelfIsInstace

            var i = 0;

            if (_callConvention == SelfCallConvention.SelfIsInstance) {
                if (method.IsStatic) {
                    Debug.Assert(RubyUtils.IsOperator(method) || method.IsExtension);

                    // receiver maps to the first parameter:
                    AddSimpleHiddenMapping(mapping, infos[i], true);
                    special[i++] = true;
                } else {
                    // receiver maps to the instance (no parameter info represents it):
                    mapping.AddParameter(new ParameterWrapper(null, method.DeclaringType, null, ParameterBindingFlags.ProhibitNull | ParameterBindingFlags.IsHidden));
                    mapping.AddInstanceBuilder(new InstanceBuilder(mapping.ArgIndex));
                }
            } else if (_callConvention == SelfCallConvention.NoSelf) {
                // instance methods on Object can be called with arbitrary receiver object including classes (static call):
                if (!method.IsStatic && method.DeclaringType == typeof(Object)) {
                    // insert an InstanceBuilder that doesn't consume any arguments, only inserts the target expression as instance:
                    mapping.AddInstanceBuilder(new ImplicitInstanceBuilder());
                }
            }

            while (i < infos.Count && infos[i].ParameterType.IsSubclassOf(typeof(RubyCallSiteStorage))) {
                mapping.AddBuilder(new RubyCallSiteStorageBuilder(infos[i]));
                special[i++] = true;
            }

            if (i < infos.Count) {
                var info = infos[i];

                if (info.ParameterType == typeof(RubyScope)) {
                    mapping.AddBuilder(new RubyScopeArgBuilder(info));
                    special[i++] = true;
                } else if (info.ParameterType == typeof(RubyContext)) {
                    mapping.AddBuilder(new RubyContextArgBuilder(info));
                    special[i++] = true;
                } else if (method.IsConstructor && info.ParameterType == typeof(RubyClass)) {
                    mapping.AddBuilder(new RubyClassCtorArgBuilder(info));
                    special[i++] = true;
                }
            }

            // If the method overload doesn't have a BlockParam parameter, we inject MissingBlockParam parameter and arg builder.
            // The parameter is treated as a regular explicit mandatory parameter.
            //
            // The argument builder provides no value for the actual argument expression, which makes the default binder to skip it
            // when emitting a tree for the actual method call (this is necessary since the method doesn't in fact have the parameter).
            // 
            // By injecting the missing block parameter we achieve that all overloads have either BlockParam, [NotNull]BlockParam or 
            // MissingBlockParam parameter. MissingBlockParam and BlockParam are convertible to each other. Default binder prefers 
            // those overloads where no conversion needs to happen, which ensures the desired semantics:
            //
            //                                        conversions with desired priority (the less number the higher priority)
            // Parameters:                call w/o block      call with non-null block       call with null block
            // (implicit, MBP, ... )      MBP -> MBP (1)            BP -> MBP (3)               BP -> MBP (2)
            // (implicit, BP,  ... )      MBP -> BP  (2)            BP -> BP  (2)               BP -> BP  (1)
            // (implicit, BP!, ... )          N/A                   BP -> BP! (1)                  N/A    
            //
            if (i < infos.Count && infos[i].ParameterType == typeof(BlockParam)) {
                AddSimpleHiddenMapping(mapping, infos[i], mapping.Overload.ProhibitsNull(i));
                special[i++] = true;
            } else if (i >= infos.Count || infos[i].ParameterType != typeof(BlockParam)) {
                mapping.AddBuilder(new MissingBlockArgBuilder(mapping.ArgIndex));
                mapping.AddParameter(new ParameterWrapper(null, typeof(MissingBlockParam), null, ParameterBindingFlags.IsHidden));
            }

            if (_callConvention == SelfCallConvention.SelfIsParameter) {
                // Ruby library methods only:
                Debug.Assert(method.IsStatic);
                Debug.Assert(i < infos.Count);

                // receiver maps to the first visible parameter:
                AddSimpleHiddenMapping(mapping, infos[i], mapping.Overload.ProhibitsNull(i));
                special[i++] = true;
            }

            return special;
        }

        private void AddSimpleHiddenMapping(ParameterMapping mapping, ParameterInfo info, bool prohibitNull) {
            mapping.AddBuilder(new SimpleArgBuilder(info, info.ParameterType, mapping.ArgIndex, false, false));
            mapping.AddParameter(new ParameterWrapper(info, info.ParameterType, null, 
                ParameterBindingFlags.IsHidden | (prohibitNull ? ParameterBindingFlags.ProhibitNull : 0)
            ));
        }

        internal static int GetHiddenParameterCount(OverloadInfo/*!*/ method, SelfCallConvention callConvention) {
            int i = 0;
            var infos = method.Parameters;

            if (callConvention == SelfCallConvention.SelfIsInstance) {
                if (method.IsStatic) {
                    Debug.Assert(RubyUtils.IsOperator(method) || method.IsExtension);
                    i++;
                }
            }

            while (i < infos.Count && infos[i].ParameterType.IsSubclassOf(typeof(RubyCallSiteStorage))) {
                i++;
            }

            if (i < infos.Count) {
                var info = infos[i];

                if (info.ParameterType == typeof(RubyScope)) {
                    i++;
                } else if (info.ParameterType == typeof(RubyContext)) {
                    i++;
                } else if (method.IsConstructor && info.ParameterType == typeof(RubyClass)) {
                    i++;
                }
            }

            if (i < infos.Count && infos[i].ParameterType == typeof(BlockParam)) {
                i++;
            }

            if (callConvention == SelfCallConvention.SelfIsParameter) {
                Debug.Assert(i < infos.Count);
                Debug.Assert(method.IsStatic);
                i++;
            }

            return i;
        }

        internal static void GetParameterCount(OverloadInfo/*!*/ method, SelfCallConvention callConvention, out int mandatory, out int optional) {
            mandatory = 0;
            optional = 0;
            for (int i = GetHiddenParameterCount(method, callConvention); i < method.ParameterCount; i++) {
                var info = method.Parameters[i];

                if (method.IsParamArray(i)) {
                    // TODO: indicate splat args separately?
                    optional++;
                } else if (info.IsOutParameter()) {
                    // Python allows passing of optional "clr.Reference" to capture out parameters
                    // Ruby should allow similar
                    optional++;
                } else if (info.IsMandatory()) {
                    mandatory++;
                } else {
                    optional++;
                }
            }
        }

        #endregion

        #region Step 2: Actual Arguments

        private static readonly DynamicMetaObject NullMetaBlockParam =
            new DynamicMetaObject(
                AstUtils.Constant(null, typeof(BlockParam)),
                BindingRestrictions.Empty,
                null
            );

        // Creates actual/normalized arguments: inserts self, expands splats, and inserts rhs arg. 
        // Adds any restrictions/conditions applied to the arguments to the given meta-builder.
        protected override ActualArguments CreateActualArguments(IList<DynamicMetaObject> namedArgs, IList<string> argNames, int preSplatLimit, int postSplatLimit) {
            var result = new List<DynamicMetaObject>();

            // self (instance):
            if (_callConvention == SelfCallConvention.SelfIsInstance) {
                result.Add(_args.MetaTarget);
            }

            if (_args.Signature.HasBlock) {
                if (_args.GetBlock() == null) {
                    // the user explicitly passed nil as a block arg:
                    result.Add(NullMetaBlockParam);
                } else {
                    // pass BlockParam:
                    if (_metaBuilder.BfcVariable == null) {
                        // we add temporary even though we might not us it if the calee doesn't have block param arg:
                        _metaBuilder.BfcVariable = _metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
                    }
                    result.Add(new DynamicMetaObject(_metaBuilder.BfcVariable, BindingRestrictions.Empty));
                }
            } else {
                // no block passed into a method with a BlockParam:
                result.Add(MissingBlockParam.Meta.Instance);
            }

            // self (parameter):
            if (_callConvention == SelfCallConvention.SelfIsParameter) {
                result.Add(_args.MetaTarget);
            }

            // the next argument is the first one for which we use restrictions coming from overload resolution:
            _firstRestrictedArg = result.Count;

            // hidden args: block, self
            int hidden = _callConvention == SelfCallConvention.NoSelf ? 1 : 2;
            return CreateActualArguments(result, _metaBuilder, _args, hidden, preSplatLimit, postSplatLimit, out _lastSplattedArg, out _list, out _listVariable);
        }

        public static IList<DynamicMetaObject/*!*/> NormalizeArguments(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, int minCount, int maxCount) {
            int lastSplattedArg;
            IList list;
            ParameterExpression listVariable;

            // 2 hidden arguments: block and self
            var actualArgs = CreateActualArguments(new List<DynamicMetaObject>(), metaBuilder, args, 2, maxCount, maxCount,
                out lastSplattedArg, out list, out listVariable);

            int actualCount = actualArgs.Count + actualArgs.CollapsedCount;

            if (actualCount < minCount) {
                metaBuilder.SetWrongNumberOfArgumentsError(actualCount, minCount);
                return null;
            } else if (actualCount > maxCount) {
                metaBuilder.SetWrongNumberOfArgumentsError(actualCount, maxCount);
                return null;
            }

            // any collapsed args are out of limits:
            return actualArgs.Arguments;
        }

        private static ActualArguments/*!*/ CreateActualArguments(List<DynamicMetaObject>/*!*/ normalized, MetaObjectBuilder/*!*/ metaBuilder,
            CallArguments/*!*/ args, int hidden, int preSplatLimit, int postSplatLimit, out int lastSplattedArg, out IList list, out ParameterExpression listVariable) {

            int firstSplattedArg, splatIndex, collapsedArgCount;

            // simple arguments:
            for (int i = 0; i < args.SimpleArgumentCount; i++) {
                normalized.Add(args.GetSimpleMetaArgument(i));
            }

            // splat argument:
            list = null;
            listVariable = null;
            if (args.Signature.HasSplattedArgument) {
                firstSplattedArg = normalized.Count;

                int listLength;
                var splatted = args.GetSplattedMetaArgument();
                list = (IList)splatted.Value;
                metaBuilder.AddSplattedArgumentTest(list, splatted.Expression, out listLength, out listVariable);

                int i = 0;
                while (i < Math.Min(listLength, preSplatLimit - firstSplattedArg)) {
                    normalized.Add(MakeSplattedItem(list, listVariable, i));
                    i++;
                }

                // skip items that are not needed for overload resolution
                splatIndex = normalized.Count;

                i = Math.Max(i, listLength - (postSplatLimit - (args.Signature.HasRhsArgument ? 1 : 0)));
                while (i < listLength) {
                    normalized.Add(MakeSplattedItem(list, listVariable, i));
                    i++;
                }

                collapsedArgCount = listLength - (normalized.Count - firstSplattedArg);
                lastSplattedArg = normalized.Count - 1;
            } else {
                splatIndex = firstSplattedArg = lastSplattedArg = -1;
                collapsedArgCount = 0;
            }

            Debug.Assert(collapsedArgCount >= 0);

            // rhs argument:
            if (args.Signature.HasRhsArgument) {
                normalized.Add(args.GetRhsMetaArgument());
            }

            return new ActualArguments(
                normalized.ToArray(),
                DynamicMetaObject.EmptyMetaObjects,
                ArrayUtils.EmptyStrings,
                hidden,
                collapsedArgCount,
                firstSplattedArg,
                splatIndex
            );
        }

        internal static DynamicMetaObject/*!*/ MakeSplattedItem(IList/*!*/ list, Expression/*!*/ listVariable, int index) {
            return DynamicMetaObject.Create(
                list[index],
                Ast.Call(listVariable, typeof(IList).GetMethod("get_Item"), AstUtils.Constant(index))
            );
        }

        #endregion

        #region Step 3: Restrictions

        internal void AddArgumentRestrictions(MetaObjectBuilder/*!*/ metaBuilder, BindingTarget/*!*/ bindingTarget) {
            var args = GetActualArguments();
            var restrictedArgs = bindingTarget.Success ? bindingTarget.RestrictedArguments.GetObjects() : args.Arguments;

            for (int i = _firstRestrictedArg; i < restrictedArgs.Count; i++) {
                var arg = (bindingTarget.Success ? restrictedArgs[i] : restrictedArgs[i].Restrict(restrictedArgs[i].GetLimitType()));

                if (i >= args.FirstSplattedArg && i <= _lastSplattedArg) {
                    metaBuilder.AddCondition(arg.Restrictions.ToExpression());
                } else {
                    metaBuilder.AddRestriction(arg.Restrictions);
                }
            }

            // Adds condition for collapsed arguments - it is the same whether we succeed or not:
            var splatCondition = GetCollapsedArgsCondition();
            if (splatCondition != null) {
                metaBuilder.AddCondition(splatCondition);
            }

            if (_argumentAssumptions != null) {
                foreach (var assumption in _argumentAssumptions) {
                    if (assumption.Second == bindingTarget.NarrowingLevel) {
                        metaBuilder.AddCondition(assumption.Third);
                    }
                }
            }
        }

        #endregion

        #region Step 4: Argument Building, Conversions

        /// <summary>
        /// Returns true if fromArg of type fromType can be assigned to toParameter with a conversion on given narrowing level.
        /// </summary>
        public override bool CanConvertFrom(Type/*!*/ fromType, DynamicMetaObject fromArg, ParameterWrapper/*!*/ toParameter, NarrowingLevel level) {
            var result = Converter.CanConvertFrom(fromArg, fromType, toParameter.Type, toParameter.ProhibitNull, level, 
                HasExplicitProtocolConversion(toParameter), _implicitProtocolConversions
            );

            if (result.Assumption != null) {
                if (_argumentAssumptions == null) {
                    _argumentAssumptions = new List<Key<int, NarrowingLevel, Ast>>();
                }

                if (_argumentAssumptions.FindIndex((k) => k.First == toParameter.ParameterInfo.Position && k.Second == level) < 0) {
                    _argumentAssumptions.Add(Key.Create(toParameter.ParameterInfo.Position, level, result.Assumption));
                }
            }

            return result.IsConvertible;
        }

        public override bool CanConvertFrom(ParameterWrapper/*!*/ fromParameter, ParameterWrapper/*!*/ toParameter) {
            return Converter.CanConvertFrom(null, fromParameter.Type, toParameter.Type, toParameter.ProhibitNull, NarrowingLevel.None, false, false).IsConvertible;
        }

        private bool HasExplicitProtocolConversion(ParameterWrapper/*!*/ parameter) {
            return
                parameter.ParameterInfo != null &&
                parameter.ParameterInfo.IsDefined(typeof(DefaultProtocolAttribute), false) &&
                !parameter.IsParamsArray; // default protocol doesn't apply on param-array/dict itself, only on the expanded parameters
        }

        public override Candidate SelectBestConversionFor(DynamicMetaObject/*!*/ arg, ParameterWrapper/*!*/ candidateOne, 
            ParameterWrapper/*!*/ candidateTwo, NarrowingLevel level) {

            Type typeOne = candidateOne.Type;
            Type typeTwo = candidateTwo.Type;
            Type actualType = arg.GetLimitType();

            if (actualType == typeof(DynamicNull)) {
                // if nil is passed as a block argument prefers BlockParam over a missing block:
                if (typeOne == typeof(BlockParam) && typeTwo == typeof(MissingBlockParam)) {
                    Debug.Assert(!candidateOne.ProhibitNull);
                    return Candidate.One;
                }

                if (typeOne == typeof(MissingBlockParam) && typeTwo == typeof(BlockParam)) {
                    Debug.Assert(!candidateTwo.ProhibitNull);
                    return Candidate.Two;
                }
            } else {
                if (typeOne == actualType) {
                    if (typeTwo == actualType) {
                        // prefer non-nullable reference type over nullable:
                        if (!actualType.IsValueType) {
                            if (candidateOne.ProhibitNull) {
                                return Candidate.One;
                            } else if (candidateTwo.ProhibitNull) {
                                return Candidate.Two;
                            }
                        }
                    } else {
                        return Candidate.One;
                    }
                } else if (typeTwo == actualType) {
                    return Candidate.Two;
                }
            }

            // prefer integer type over enum:
            if (typeOne.IsEnum && Enum.GetUnderlyingType(typeOne) == typeTwo) {
                return Candidate.Two;
            }

            if (typeTwo.IsEnum && Enum.GetUnderlyingType(typeTwo) == typeOne) {
                return Candidate.One;
            }

            return base.SelectBestConversionFor(arg, candidateOne, candidateTwo, level);
        }

        public override Expression/*!*/ Convert(DynamicMetaObject/*!*/ metaObject, Type restrictedType, ParameterInfo info, Type/*!*/ toType) {
            Expression expr = metaObject.Expression;
            Type fromType = restrictedType ?? expr.Type;

            // block:
            if (fromType == typeof(MissingBlockParam)) {
                Debug.Assert(toType == typeof(BlockParam) || toType == typeof(MissingBlockParam));
                return AstUtils.Constant(null);
            }

            if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam)) {
                return AstUtils.Constant(null);
            }

            // protocol conversions:
            if (info != null && info.IsDefined(typeof(DefaultProtocolAttribute), false)) {
                var action = RubyConversionAction.TryGetDefaultConversionAction(Context, toType);
                if (action != null) {
                    // TODO: inline implicit conversions:
                    return AstUtils.LightDynamic(action, toType, expr);
                }

                // Do not throw an exception here to allow generic type parameters to be used with D.P. attribute.
                // The semantics should be to use DP if available for the current instantiation and ignore it otherwise.
            }

            if (restrictedType != null) {
                if (restrictedType == typeof(DynamicNull)) {
                    if (!toType.IsValueType || toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        return AstUtils.Constant(null, toType);
                    } else if (toType == typeof(bool)) {
                        return AstUtils.Constant(false);
                    }
                }

                if (toType.IsAssignableFrom(restrictedType)) {
                    // expr can be converted to restrictedType, which can be converted toType => we can convert expr to toType:
                    return AstUtils.Convert(expr, CompilerHelpers.GetVisibleType(toType));
                }

                // if there is a simple conversion from restricted type, convert the expression to the restricted type and use that conversion:
                Type visibleRestrictedType = CompilerHelpers.GetVisibleType(restrictedType);
                if (Converter.CanConvertFrom(metaObject, visibleRestrictedType, toType, false, NarrowingLevel.None, false, false).IsConvertible) {
                    expr = AstUtils.Convert(expr, visibleRestrictedType);
                }
            }

            return Converter.ConvertExpression(expr, toType, _args.RubyContext, _args.MetaContext.Expression, _implicitProtocolConversions);
        }

        protected override Expression/*!*/ GetSplattedExpression() {
            return _listVariable;
        }

        protected override object GetSplattedItem(int index) {
            return _list[index];
        }

        internal sealed class RubyContextArgBuilder : ArgBuilder {
            public RubyContextArgBuilder(ParameterInfo/*!*/ info)
                : base(info) {
            }

            public override int Priority {
                get { return -1; }
            }

            public override int ConsumedArgumentCount {
                get { return 0; }
            }

            protected override Expression ToExpression(OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                return ((RubyOverloadResolver)resolver).ContextExpression;
            }
        }

        internal sealed class RubyCallSiteStorageBuilder : ArgBuilder {
            public RubyCallSiteStorageBuilder(ParameterInfo/*!*/ info)
                : base(info) {
            }

            public override int Priority {
                get { return -1; }
            }

            public override int ConsumedArgumentCount {
                get { return 0; }
            }

            protected override Expression ToExpression(OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                return AstUtils.Constant(Activator.CreateInstance(ParameterInfo.ParameterType, ((RubyOverloadResolver)resolver).Context));
            }
        }

        internal sealed class RubyScopeArgBuilder : ArgBuilder {
            public RubyScopeArgBuilder(ParameterInfo/*!*/ info)
                : base(info) {
            }

            public override int Priority {
                get { return -1; }
            }

            public override int ConsumedArgumentCount {
                get { return 0; }
            }

            protected override Expression ToExpression(OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                return ((RubyOverloadResolver)resolver).ScopeExpression;
            }
        }

        internal sealed class RubyClassCtorArgBuilder : ArgBuilder {
            public RubyClassCtorArgBuilder(ParameterInfo/*!*/ info)
                : base(info) {
            }

            public override int Priority {
                get { return -1; }
            }

            public override int ConsumedArgumentCount {
                get { return 0; }
            }

            protected override Expression ToExpression(OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                return ((RubyOverloadResolver)resolver)._args.TargetExpression;
            }
        }

        internal sealed class ImplicitInstanceBuilder : InstanceBuilder {
            public ImplicitInstanceBuilder()
                : base(-1) {
            }

            public override bool HasValue {
                get { return true; }
            }

            public override int ConsumedArgumentCount {
                get { return 0; }
            }

            protected override Expression/*!*/ ToExpression(ref MethodInfo/*!*/ method, OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                return ((RubyOverloadResolver)resolver)._args.TargetExpression;
            }
        }

        internal sealed class MissingBlockArgBuilder : SimpleArgBuilder {
            public MissingBlockArgBuilder(int index)
                : base(typeof(MissingBlockParam), index, false, false) {
            }

            public override int Priority {
                get { return -1; }
            }

            public override int ConsumedArgumentCount {
                get { return 1; }
            }

            protected override SimpleArgBuilder/*!*/ Copy(int newIndex) {
                return new MissingBlockArgBuilder(newIndex);
            }

            protected override Expression ToExpression(OverloadResolver/*!*/ resolver, RestrictedArguments/*!*/ args, bool[]/*!*/ hasBeenUsed) {
                Debug.Assert(Index < args.Length);
                Debug.Assert(Index < hasBeenUsed.Length);
                hasBeenUsed[Index] = true;
                return null;
            }
        }

        #endregion

        #region Step 5: Errors

        public override Microsoft.Scripting.Actions.ErrorInfo MakeInvalidParametersError(BindingTarget target) {
            Expression exceptionValue;
            switch (target.Result) {
                case BindingResult.AmbiguousMatch:
                    exceptionValue = MakeAmbiguousCallError(target);
                    break;

                case BindingResult.IncorrectArgumentCount:
                    exceptionValue = MakeIncorrectArgumentCountError(target);
                    break;

                case BindingResult.CallFailure:
                    exceptionValue = MakeCallFailureError(target);
                    break;

                case BindingResult.NoCallableMethod:
                    exceptionValue = Methods.CreateArgumentsError.OpCall(
                        AstUtils.Constant(String.Format("Method '{0}' is not callable", target.Name))
                    );
                    break;

                default: 
                    throw new InvalidOperationException();
            }
            return Microsoft.Scripting.Actions.ErrorInfo.FromException(exceptionValue);
        }

        private Expression MakeAmbiguousCallError(BindingTarget target) {
            StringBuilder sb = new StringBuilder(string.Format("Found multiple methods for '{0}': ", target.Name));
            string outerComma = "";
            foreach (MethodCandidate candidate in target.AmbiguousMatches) {
                IList<ParameterWrapper> parameters = candidate.GetParameters();
                
                string innerComma = "";

                sb.Append(outerComma);
                sb.Append(target.Name);
                sb.Append('(');
                foreach (var param in parameters) {
                    if (!param.IsHidden) {
                        sb.Append(innerComma);
                        sb.Append(Binder.GetTypeName(param.Type));
                        if (param.ProhibitNull) {
                            sb.Append('!');
                        }
                        innerComma = ", ";
                    }
                }

                sb.Append(')');
                outerComma = ", ";
            }

            return Methods.MakeAmbiguousMatchError.OpCall(AstUtils.Constant(sb.ToString()));
        }

        private Expression MakeIncorrectArgumentCountError(BindingTarget target) {
            IList<int> available = target.ExpectedArgumentCount;
            int expected;

            if (available.Count > 0) {
                int minGreater = Int32.MaxValue;
                int maxLesser = Int32.MinValue;
                int max = Int32.MinValue;
                foreach (int arity in available) {
                    if (arity > target.ActualArgumentCount) {
                        minGreater = Math.Min(minGreater, arity);
                    } else {
                        maxLesser = Math.Max(maxLesser, arity);
                    }

                    max = Math.Max(max, arity);
                }

                expected = (target.ActualArgumentCount < maxLesser ? maxLesser : Math.Min(minGreater, max));
            } else {
                // no overload is callable:
                expected = 0;
            }

            return Methods.MakeWrongNumberOfArgumentsError.OpCall(AstUtils.Constant(target.ActualArgumentCount), AstUtils.Constant(expected));
        }

        private Expression MakeCallFailureError(BindingTarget target) {
            foreach (CallFailure cf in target.CallFailures) {
                switch (cf.Reason) {
                    case CallFailureReason.ConversionFailure:
                        foreach (ConversionResult cr in cf.ConversionResults) {
                            if (cr.Failed) {
                                if (typeof(Proc).IsAssignableFrom(cr.To)) {
                                    return Methods.CreateArgumentsErrorForProc.OpCall(AstUtils.Constant(cr.GetArgumentTypeName(Binder)));
                                }

                                Debug.Assert(typeof(BlockParam).IsSealed);
                                if (cr.To == typeof(BlockParam)) {
                                    return Methods.CreateArgumentsErrorForMissingBlock.OpCall();
                                }

                                string toType;
                                if (cr.To.IsGenericType && cr.To.GetGenericTypeDefinition() == typeof(Union<,>)) {
                                    var g = cr.To.GetGenericArguments();
                                    toType = Binder.GetTypeName(g[0]) + " or " + Binder.GetTypeName(g[1]);
                                } else {
                                    toType = Binder.GetTypeName(cr.To);
                                }

                                return Methods.CreateTypeConversionError.OpCall(
                                    AstUtils.Constant(cr.GetArgumentTypeName(Binder)),
                                    AstUtils.Constant(toType)
                                );
                            }
                        }
                        break;

                    case CallFailureReason.TypeInference:
                        // TODO: Display generic parameters so it's clear what we couldn't infer.
                        return Methods.CreateArgumentsError.OpCall(
                            AstUtils.Constant(String.Format("generic arguments could not be infered for method '{0}'", target.Name))
                        );

                    case CallFailureReason.DuplicateKeyword:
                    case CallFailureReason.UnassignableKeyword:
                    default: 
                        throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException();
        }

        #endregion
    }
}