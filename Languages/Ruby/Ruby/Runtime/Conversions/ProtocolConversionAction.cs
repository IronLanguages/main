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
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Conversions {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public abstract class RubyConversionAction : RubyMetaBinder {
        protected RubyConversionAction() 
            : base(null) {
        }

        protected RubyConversionAction(RubyContext context)
            : base(context) {
        }

        // All protocol conversion actions ignore the current scope and use RubyContext only. 
        // That means if the to_xxx conversion methods or respond_to? are aliased to scope manipulating methods (e.g. eval, private, ...)
        // it won't work. We assume that this is acceptable (it could be easily changed, we can define 2 kinds of protocol actions: with scope and with context).
        public override RubyCallSignature Signature {
            get { return RubyCallSignature.WithImplicitSelf(0); }
        }

        public override string/*!*/ ToString() {
            return GetType().Name + (Context != null ? " @" + Context.RuntimeId.ToString() : null);
        }

        public static RubyConversionAction TryGetDefaultConversionAction(RubyContext context, Type/*!*/ parameterType) {
            var factory = TryGetDefaultConversionAction(parameterType);
            return factory != null ? factory(context != null ? context.MetaBinderFactory : RubyMetaBinderFactory.Shared) : null;
        }

        public static RubyConversionAction/*!*/ GetConversionAction(RubyContext context, Type/*!*/ parameterType, bool allowProtocolConversions) {
            var factory = TryGetConversionAction(parameterType, allowProtocolConversions);
            return factory != null ? factory(context != null ? context.MetaBinderFactory : RubyMetaBinderFactory.Shared) : null;
        }

        public static bool HasDefaultConversion(Type/*!*/ parameterType) {
            return TryGetDefaultConversionAction(parameterType) != null;
        }

        private static Func<RubyMetaBinderFactory, RubyConversionAction> TryGetConversionAction(Type/*!*/ parameterType, bool allowProtocolConversion) {
            if (allowProtocolConversion) {
                var result = TryGetDefaultConversionAction(parameterType);
                if (result != null) {
                    return result;
                }
            }

            return (factory) => factory.GenericConversionAction(parameterType);
        }

        private static Func<RubyMetaBinderFactory, RubyConversionAction> TryGetDefaultConversionAction(Type/*!*/ parameterType) {
            // TODO: 
            // nullable int (see Array#fill, Sockets:ConvertToSocketFlag, Kernel#open(perm=nil), File.chown, IO#read)

            // TODO: do we want to use a default protocol for enums?
            if (parameterType.IsEnum) {
                return null;
            }

            switch (Type.GetTypeCode(parameterType)) {
                case TypeCode.SByte: return (factory) => factory.Conversion<ConvertToSByteAction>();
                case TypeCode.Byte: return (factory) => factory.Conversion<ConvertToByteAction>();
                case TypeCode.Int16: return (factory) => factory.Conversion<ConvertToInt16Action>();
                case TypeCode.UInt16: return (factory) => factory.Conversion<ConvertToUInt16Action>();
                case TypeCode.Int32: return (factory) => factory.Conversion<ConvertToFixnumAction>();
                case TypeCode.UInt32: return (factory) => factory.Conversion<ConvertToUInt32Action>();
                case TypeCode.Int64: return (factory) => factory.Conversion<ConvertToInt64Action>();
                case TypeCode.UInt64: return (factory) => factory.Conversion<ConvertToUInt64Action>();
                case TypeCode.Single: return (factory) => factory.Conversion<ConvertToSingleAction>();
                case TypeCode.Double: return (factory) => factory.Conversion<ConvertToFAction>();
                case TypeCode.String: return (factory) => factory.Conversion<ConvertToSymbolAction>();
            }

            if (parameterType == typeof(MutableString)) {
                return (factory) => factory.Conversion<ConvertToStrAction>();
            }

            if (parameterType == typeof(BigInteger)) {
                return (factory) => factory.Conversion<ConvertToBignumAction>();
            }

            if (parameterType == typeof(IntegerValue)) {
                return (factory) => factory.Conversion<ConvertToIntAction>();
            }

            if (parameterType == typeof(Union<int, MutableString>)) {
                return (factory) => factory.CompositeConversion(CompositeConversion.ToFixnumToStr);
            }

            if (parameterType == typeof(Union<MutableString, int>)) {
                return (factory) => factory.CompositeConversion(CompositeConversion.ToStrToFixnum);
            }

            if (parameterType == typeof(RubyRegex)) {
                return (factory) => factory.Conversion<ConvertToRegexAction>();
            }

            if (parameterType == typeof(IList)) {
                return (factory) => factory.Conversion<ConvertToArrayAction>();
            }

            if (parameterType == typeof(IDictionary<object, object>)) {
                return (factory) => factory.Conversion<ConvertToHashAction>();
            }

            if (parameterType == typeof(int?)) {
                return (factory) => factory.Conversion<TryConvertToFixnumAction>();
            }

            return null;
        }

        internal static Expression ImplicitConvert(Type/*!*/ toType, CallArguments/*!*/ args) {
            return Converter.ImplicitConvert(args.TargetExpression, CompilerHelpers.GetType(args.Target), toType);
        }

        internal static Expression ExplicitConvert(Type/*!*/ toType, CallArguments/*!*/ args) {
            return Converter.ExplicitConvert(args.TargetExpression, CompilerHelpers.GetType(args.Target), toType);
        }

        internal static Expression Convert(Type/*!*/ toType, CallArguments/*!*/ args) {
            var fromType = CompilerHelpers.GetType(args.Target);
            return Converter.ImplicitConvert(args.TargetExpression, fromType, toType)
                ?? Converter.ExplicitConvert(args.TargetExpression, fromType, toType);
        }
    }

    // Conversion operations:
    // 1) tries implicit conversions
    // 2) calls respond_to? :to_xxx
    // 3) calls to_xxx
    public abstract class ProtocolConversionAction : RubyConversionAction {
        protected ProtocolConversionAction() {
        }

        protected abstract string/*!*/ ToMethodName { get; }
        protected abstract MethodInfo ConversionResultValidator { get; }
        protected abstract string/*!*/ TargetTypeName { get; }

        public override Type/*!*/ ReturnType {
            get { 
                return (ConversionResultValidator != null) ? ConversionResultValidator.ReturnType : typeof(object); 
            }
        }

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            Debug.Assert(defaultFallback, "custom fallback not supported");
            BuildConversion(metaBuilder, args, ReturnType, this);
            return true;
        }

        internal static void BuildConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Type/*!*/ resultType,
            params ProtocolConversionAction/*!*/[]/*!*/ conversions) {
            Assert.NotNull(metaBuilder, args, conversions);
            Debug.Assert(args.SimpleArgumentCount == 0 && !args.Signature.HasBlock && !args.Signature.HasSplattedArgument && !args.Signature.HasRhsArgument);
            Debug.Assert(!args.Signature.HasScope);

            // implicit conversions should only depend on the static type:
            foreach (var conversion in conversions) {
                if (conversion.TryImplicitConversion(metaBuilder, args)) {
                    metaBuilder.AddObjectTypeRestriction(args.Target, args.TargetExpression);

                    if (!metaBuilder.Error) {
                        metaBuilder.Result = ConvertResult(metaBuilder.Result, resultType);
                    }
                    return;
                }
            }

            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            Expression targetClassNameConstant = AstUtils.Constant(targetClass.GetNonSingletonClass().Name, typeof(string));
            MethodResolutionResult respondToMethod, methodMissing = MethodResolutionResult.NotFound;
            ProtocolConversionAction selectedConversion = null;
            RubyMemberInfo conversionMethod = null;

            using (targetClass.Context.ClassHierarchyLocker()) {
                // check for type version:
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext,
                    ArrayUtils.Insert(Symbols.RespondTo, Symbols.MethodMissing, ArrayUtils.ConvertAll(conversions, (c) => c.ToMethodName))
                );

                // we can optimize if Kernel#respond_to? method is not overridden:
                respondToMethod = targetClass.ResolveMethodForSiteNoLock(Symbols.RespondTo, VisibilityContext.AllVisible);
                if (respondToMethod.Found && respondToMethod.Info.DeclaringModule == targetClass.Context.KernelModule && respondToMethod.Info is RubyLibraryMethodInfo) { // TODO: better override detection
                    respondToMethod = MethodResolutionResult.NotFound;

                    // get the first applicable conversion:
                    foreach (var conversion in conversions) {
                        selectedConversion = conversion;
                        conversionMethod = targetClass.ResolveMethodForSiteNoLock(conversion.ToMethodName, VisibilityContext.AllVisible).Info;
                        if (conversionMethod != null) {
                            break;
                        } else {
                            // find method_missing - we need to add "to_xxx" methods to the missing methods table:
                            if (!methodMissing.Found) {
                                methodMissing = targetClass.ResolveMethodNoLock(Symbols.MethodMissing, VisibilityContext.AllVisible);
                            }
                            methodMissing.InvalidateSitesOnMissingMethodAddition(conversion.ToMethodName, targetClass.Context);
                        }
                    }
                }
            }

            if (!respondToMethod.Found) {
                if (conversionMethod == null) {
                    // error:
                    selectedConversion.SetError(metaBuilder, args, targetClassNameConstant, resultType);
                    return;
                } else {
                    // invoke target.to_xxx() and validate it; returns an instance of TTargetType:
                    conversionMethod.BuildCall(metaBuilder, args, selectedConversion.ToMethodName);

                    if (!metaBuilder.Error) {
                        metaBuilder.Result = ConvertResult(
                            selectedConversion.MakeValidatorCall(args, targetClassNameConstant, metaBuilder.Result), 
                            resultType
                        );
                    }
                    return;
                }
            }

            // slow path: invoke respond_to?, to_xxx and result validation:
            for (int i = conversions.Length - 1; i >= 0; i--) {
                string toMethodName = conversions[i].ToMethodName;
                
                var conversionCallSite = AstUtils.LightDynamic(
                    RubyCallAction.Make(args.RubyContext, toMethodName, RubyCallSignature.WithImplicitSelf(0)),
                    args.TargetExpression
                );

                metaBuilder.Result = Ast.Condition(
                    // If

                    // respond_to?()
                    Methods.IsTrue.OpCall(
                        AstUtils.LightDynamic(
                            RubyCallAction.Make(args.RubyContext, Symbols.RespondTo, RubyCallSignature.WithImplicitSelf(1)),
                            args.TargetExpression, 
                            Ast.Constant(args.RubyContext.CreateSymbol(toMethodName, RubyEncoding.Binary))
                        )
                    ),

                    // Then

                    // to_xxx():
                    ConvertResult(
                        conversions[i].MakeValidatorCall(args, targetClassNameConstant, conversionCallSite),
                        resultType
                    ),

                    // Else

                    (i < conversions.Length - 1) ? metaBuilder.Result : 
                        conversions[i].MakeErrorExpression(args, targetClassNameConstant, resultType)
                );
            }
        }

        internal protected abstract bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

        protected virtual Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return Ast.Throw(
                Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, AstUtils.Constant(TargetTypeName)),
                resultType
            );
        }

        protected virtual void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, AstUtils.Constant(TargetTypeName)));            
        }

        protected virtual Expression/*!*/ MakeValidatorCall(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Expression/*!*/ result) {
            var validator = ConversionResultValidator;
            return (validator != null) ? validator.OpCall(targetClassNameConstant, AstUtils.Box(result)) : result;
        }

        private static Expression/*!*/ ConvertResult(Expression/*!*/ expression, Type/*!*/ resultType) {
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Union<,>)) {
                var args = resultType.GetGenericArguments();
                var ctor = resultType.GetConstructor(args);
                if (args[0].IsAssignableFrom(expression.Type)) {
                    return Ast.New(ctor, expression, AstUtils.Default(args[1]));
                } else {
                    Debug.Assert(args[1].IsAssignableFrom(expression.Type));
                    return Ast.New(ctor, AstUtils.Default(args[0]), expression);
                }
            } else {
                return AstUtils.Convert(expression, resultType);
            }
        }
    }

    public abstract class ProtocolConversionAction<TSelf> : ProtocolConversionAction
        where TSelf : ProtocolConversionAction<TSelf>, new() {
        
        public static TSelf/*!*/ Make(RubyContext/*!*/ context) {
            return context.MetaBinderFactory.Conversion<TSelf>();
        }

        [Emitted]
        public static TSelf/*!*/ MakeShared() {
            return RubyMetaBinderFactory.Shared.Conversion<TSelf>();
        }

        protected ProtocolConversionAction() {
            Debug.Assert(GetType() == typeof(TSelf));
        }

        public override Expression/*!*/ CreateExpression() {
            return Methods.GetMethod(typeof(ProtocolConversionAction<TSelf>), "MakeShared").OpCall();
        }
    }

    public abstract class ConvertToReferenceTypeAction<TSelf, TTargetType> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToReferenceTypeAction<TSelf, TTargetType>, new()
        where TTargetType : class {

        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            return TryImplicitConversionInternal(metaBuilder, args);
        }

        internal static bool TryImplicitConversionInternal(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                metaBuilder.Result = AstUtils.Constant(null, typeof(TTargetType));
                return true;
            }

            var convertedTarget = args.Target as TTargetType;
            if (convertedTarget != null) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(TTargetType));
                return true;
            }
            return false;
        }
    }

    public abstract class TryConvertToReferenceTypeAction<TSelf, TTargetType> : ConvertToReferenceTypeAction<TSelf, TTargetType>
        where TSelf : TryConvertToReferenceTypeAction<TSelf, TTargetType>, new()
        where TTargetType : class {

        // return null if the object doesn't handle the conversion:
        protected override Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return AstUtils.Constant(null, resultType);
        }

        // return null if the object doesn't handle the conversion:
        protected override void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.Result = AstUtils.Constant(null, resultType);
        }
    }

    public sealed class ConvertToProcAction : ConvertToReferenceTypeAction<ConvertToProcAction, Proc> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToProc; } }
        protected override string/*!*/ TargetTypeName { get { return "Proc"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToProcValidator; } }
    }

    #region String, Path, Symbol, Regex

    public sealed class ConvertToStrAction : ConvertToReferenceTypeAction<ConvertToStrAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }

        protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args) {
            if (base.TryImplicitConversion(metaBuilder, args)) {
                return true;
            }

            if (args.Target is RubySymbol) {
                metaBuilder.Result = Methods.ConvertSymbolToMutableString.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubySymbol)));
                return true;
            }

            return false;
        }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args, 
            out MethodInfo postConverter) {
            postConverter = Methods.StringToMutableString;
            return context.MetaBinderFactory.InteropConvert(typeof(string), true);
        }
    }

    public sealed class ConvertToPathAction : ConvertToReferenceTypeAction<ConvertToPathAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToPath; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return null; } }
        
        protected override Expression/*!*/ MakeValidatorCall(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Expression/*!*/ result) {
            return AstUtils.LightDynamic(ConvertToStrAction.Make(args.RubyContext), AstUtils.Box(result));
        }
    }

    public sealed class TryConvertToPathAction : TryConvertToReferenceTypeAction<TryConvertToPathAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToPath; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }
    }

    public sealed class TryConvertToStrAction : TryConvertToReferenceTypeAction<TryConvertToStrAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }

        protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args) {
            if (base.TryImplicitConversion(metaBuilder, args)) {
                return true;
            }

            var convertedTarget = args.Target as RubySymbol;
            if (convertedTarget != null) {
                metaBuilder.Result = Methods.ConvertSymbolToMutableString.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubySymbol)));
                return true;
            }
            return false;
        }
    }

    // TODO: escaping vs. non-escaping?
    // This conversion escapes the regex. 
    public sealed class ConvertToRegexAction : ConvertToReferenceTypeAction<ConvertToRegexAction, RubyRegex> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Regexp"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToRegexValidator; } }
    }

    // TODO: remove (replace by MutableString/RubySymbol conversion)
    public sealed class ConvertToSymbolAction : ConvertToReferenceTypeAction<ConvertToSymbolAction, string> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Symbol"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSymbolValidator; } }

        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (base.TryImplicitConversion(metaBuilder, args)) {
                return true;
            }

            object target = args.Target;
            var targetExpression = args.TargetExpression;

            var str = target as MutableString;
            if (str != null) {
                metaBuilder.Result = Methods.ConvertMutableStringToClrString.OpCall(AstUtils.Convert(targetExpression, typeof(MutableString)));
                return true;
            }

            if (target is RubySymbol) {
                metaBuilder.Result = Methods.ConvertSymbolToClrString.OpCall(AstUtils.Convert(targetExpression, typeof(RubySymbol)));
                return true;
            }

            if (target is int) {
                metaBuilder.Result = Methods.ConvertRubySymbolToClrString.OpCall(
                    AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)),
                    AstUtils.Convert(targetExpression, typeof(int))
                );
                return true;
            }

            return false;
        }
    }

    #endregion

    #region Array, Hash, Enumerable

    public sealed class ConvertToArrayAction : ConvertToReferenceTypeAction<ConvertToArrayAction, IList> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
    }

    public sealed class TryConvertToArrayAction : TryConvertToReferenceTypeAction<TryConvertToArrayAction, IList> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
    }

    // TODO: should be like to_s - default to_a is always called w/o call to respond_to?
    public sealed class TryConvertToAAction : TryConvertToReferenceTypeAction<TryConvertToAAction, IList> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToA; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
    }

    public sealed class ConvertToHashAction : ConvertToReferenceTypeAction<ConvertToHashAction, IDictionary<object, object>> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToHash; } }
        protected override string/*!*/ TargetTypeName { get { return "Hash"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToHashValidator; } }
    }

    public sealed class TryConvertToHashAction : TryConvertToReferenceTypeAction<TryConvertToHashAction, IDictionary<object, object>> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToHash; } }
        protected override string/*!*/ TargetTypeName { get { return "Hash"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToHashValidator; } }
    }

    /// <summary>
    /// Splats the target value. Returns the value if it cannot be splatted.
    /// </summary>
    public abstract class TrySplatAction<TSelf> : ConvertToReferenceTypeAction<TSelf, IList>
        where TSelf : TrySplatAction<TSelf>, new() {

        protected override string/*!*/ TargetTypeName { get { return "Array"; } }

        public override Type/*!*/ ReturnType {
            get { return typeof(object); }
        }

        // return the target object on error:
        protected override Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return AstUtils.Box(args.TargetExpression);
        }

        // return the target object on error:
        protected override void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.Result = AstUtils.Box(args.TargetExpression);
        }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject>/*!*/ args, out MethodInfo postProcessor) {
            postProcessor = null;
            return context.MetaBinderFactory.InteropSplat();
        }
    }

    /// <summary>
    /// Splats the target value. Wraps the value in a RubyArray if the value cannot be splatted.
    /// </summary>
    public abstract class SplatAction<TSelf> : ConvertToReferenceTypeAction<TSelf, IList>
        where TSelf : SplatAction<TSelf>, new() {

        protected override string/*!*/ TargetTypeName { get { return "Array"; } }

        protected internal override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            var convertedTarget = args.Target as IList;
            if (convertedTarget != null) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(IList));
                return true;
            }
            return false;
        }

        // return the target object on error:
        protected override Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return Ast.Convert(Methods.MakeArray1.OpCall(AstUtils.Box(args.TargetExpression)), typeof(IList));
        }

        // return the target object on error:
        protected override void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.Result = Ast.Convert(Methods.MakeArray1.OpCall(AstUtils.Box(args.TargetExpression)), typeof(IList));
        }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject>/*!*/ args, out MethodInfo postProcessor) {
            postProcessor = null;
            return context.MetaBinderFactory.InteropSplat();
        }
    }

    /// <summary>
    /// Returns result of "to_a" if the object responds to "to_a" or the target object itself otherwise.
    /// Throws an exception if "to_a" doesn't return IList.
    /// </summary>
    /// <seealso cref="http://redmine.ruby-lang.org/issues/show/3680"/>
    public sealed class ExplicitTrySplatAction : TrySplatAction<ExplicitTrySplatAction> {
        protected sealed override string/*!*/ ToMethodName { get { return Symbols.ToA; } }
        protected sealed override MethodInfo ConversionResultValidator { get { return Methods.ToAValidator; } }
    }

    /// <summary>
    /// Returns result of "to_ary" if the object responds to "to_ary" or the target object itself otherwise.
    /// Throws an exception if "to_ary" doesn't return IList.
    /// </summary>
    /// <seealso cref="http://redmine.ruby-lang.org/issues/show/3680"/>
    public sealed class ImplicitTrySplatAction : TrySplatAction<ImplicitTrySplatAction> {
        protected sealed override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected sealed override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
    }

    /// <summary>
    /// Returns result of "to_a" if the object responds to "to_a" or the target object itself otherwise.
    /// Throws an exception if "to_a" doesn't return IList.
    /// </summary>
    /// <seealso cref="http://redmine.ruby-lang.org/issues/show/3680"/>
    public sealed class ExplicitSplatAction : SplatAction<ExplicitSplatAction> {
        protected sealed override string/*!*/ ToMethodName { get { return Symbols.ToA; } }
        protected sealed override MethodInfo ConversionResultValidator { get { return Methods.ToAValidator; } }
    }

    /// <summary>
    /// Returns result of "to_ary" if the object responds to "to_ary" or the target object itself otherwise.
    /// Throws an exception if "to_ary" doesn't return IList.
    /// </summary>
    /// <seealso cref="http://redmine.ruby-lang.org/issues/show/3680"/>
    public sealed class ImplicitSplatAction : SplatAction<ImplicitSplatAction> {
        protected sealed override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected sealed override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
    }

    #endregion

    #region Integers

    public abstract class ConvertToIntegerAction<TSelf> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToIntegerAction<TSelf>, new() {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }

        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;

            if (target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }

            return (metaBuilder.Result = Convert(ReturnType, args)) != null;
        }
    }

    public class TryConvertToFixnumAction : ProtocolConversionAction<TryConvertToFixnumAction> {
        protected override string/*!*/ TargetTypeName { get { return "Fixnum"; } }
        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        public override Type/*!*/ ReturnType { get { return typeof(int?); } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFixnumValidator; } }
        
        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;
            return target != null && (metaBuilder.Result = Convert(ReturnType, args)) != null;
        }

        // return null if the object doesn't handle the conversion:
        protected override Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return AstUtils.Constant(null, resultType);
        }

        // return null if the object doesn't handle the conversion:
        protected override void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.Result = AstUtils.Constant(null, resultType);
        }
    }

    public sealed class ConvertToFixnumAction : ConvertToIntegerAction<ConvertToFixnumAction> {
        protected override string/*!*/ TargetTypeName { get { return "Fixnum"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFixnumValidator; } }
    }

    public sealed class ConvertToByteAction : ConvertToIntegerAction<ConvertToByteAction> {
        protected override string/*!*/ TargetTypeName { get { return "System::Byte"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToByteValidator; } }
    }

    public sealed class ConvertToSByteAction : ConvertToIntegerAction<ConvertToSByteAction> {
        protected override string/*!*/ TargetTypeName { get { return "System::SByte"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSByteValidator; } }
    }

    public sealed class ConvertToInt16Action : ConvertToIntegerAction<ConvertToInt16Action> {
        protected override string/*!*/ TargetTypeName { get { return "System::Int16"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToInt16Validator; } }
    }

    public sealed class ConvertToUInt16Action : ConvertToIntegerAction<ConvertToUInt16Action> {
        protected override string/*!*/ TargetTypeName { get { return "System::UInt16"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToUInt16Validator; } }
    }

    public sealed class ConvertToUInt32Action : ConvertToIntegerAction<ConvertToUInt32Action> {
        protected override string/*!*/ TargetTypeName { get { return "System::UInt32"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToUInt32Validator; } }
    }

    public sealed class ConvertToUInt64Action : ConvertToIntegerAction<ConvertToUInt64Action> {
        protected override string/*!*/ TargetTypeName { get { return "System::UInt64"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToUInt64Validator; } }
    }

    public sealed class ConvertToInt64Action : ConvertToIntegerAction<ConvertToInt64Action> {
        protected override string/*!*/ TargetTypeName { get { return "System::Int64"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToInt64Validator; } }
    }

    public sealed class ConvertToBignumAction : ConvertToIntegerAction<ConvertToBignumAction> {
        protected override string/*!*/ TargetTypeName { get { return "Bignum"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToBignumValidator; } }
    }

    #endregion

    #region IntegerValue

    public abstract class ConvertToIntegerValueAction<TSelf> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToIntegerValueAction<TSelf>, new() {

        protected override string/*!*/ TargetTypeName { get { return "Integer"; } }

        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }

            metaBuilder.Result = 
                ImplicitConvert(typeof(int), args) ??
                ImplicitConvert(typeof(BigInteger), args);

            return metaBuilder.Result != null;
        }
    }

    /// <summary>
    /// Calls to_int and wraps the result (Fixnum or Bignum) into IntegerValue.
    /// </summary>
    public sealed class ConvertToIntAction : ConvertToIntegerValueAction<ConvertToIntAction> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToIntegerValidator; } }
    }

    /// <summary>
    /// Calls to_i and wraps the result (Fixnum or Bignum) into IntegerValue.
    /// </summary>
    public sealed class ConvertToIAction : ConvertToIntegerValueAction<ConvertToIAction> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToI; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToIntegerValidator; } }
    }

    #endregion

    #region Floating Point

    /// <summary>
    /// Calls to_f (in most cases) and wraps the result into double. It directly calls Kernel.Float for String, Fixnum and Bignum.
    /// </summary>
    public abstract class ConvertToFloatingPointAction<TSelf> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToFloatingPointAction<TSelf>, new() {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToF; } }

        internal protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }

            metaBuilder.Result = Convert(ReturnType, args) ?? FromString(args);

            return metaBuilder.Result != null;
        }

        private static Expression FromString(CallArguments/*!*/ args) {
            var result = ImplicitConvert(typeof(MutableString), args);
            if (result != null) {
                return Ast.Call(Methods.ConvertMutableStringToFloat, args.MetaContext.Expression, result);
            }

            result = ImplicitConvert(typeof(string), args);
            if (result != null) {
                return Ast.Call(Methods.ConvertStringToFloat, args.MetaContext.Expression, result);
            }

            return null;
        }
    }

    public sealed class ConvertToFAction : ConvertToFloatingPointAction<ConvertToFAction> {
        protected override string/*!*/ TargetTypeName { get { return "Float"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToDoubleValidator; } }
    }

    public sealed class ConvertToSingleAction : ConvertToFloatingPointAction<ConvertToSingleAction> {
        protected override string/*!*/ TargetTypeName { get { return "System::Single"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSingleValidator; } }
    }

    #endregion
}
