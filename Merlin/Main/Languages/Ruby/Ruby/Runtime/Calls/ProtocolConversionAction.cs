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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Math;
using IronRuby.Compiler.Generation;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace IronRuby.Runtime.Calls {
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

        public override T BindDelegate<T>(System.Runtime.CompilerServices.CallSite<T> site, object[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Ruby: " + GetType().Name + ": BindDelegate");
            return base.BindDelegate<T>(site, args);
        }

        public static RubyConversionAction TryGetDefaultConversionAction(RubyContext/*!*/ context, Type/*!*/ parameterType) {
            var factory = context.MetaBinderFactory;

            if (parameterType == typeof(MutableString)) {
                return factory.Conversion<ConvertToStrAction>();
            }

            // TODO: nullable int (see Array#fill, Sockets:ConvertToSocketFlag, Kernel#open(perm=nil))
            if (parameterType == typeof(int)) {
                return factory.Conversion<ConvertToFixnumAction>();
            }

            if (parameterType == typeof(string)) {
                return factory.Conversion<ConvertToSymbolAction>();
            }

            if (parameterType == typeof(IntegerValue)) {
                return factory.Conversion<ConvertToIntAction>();
            }

            if (parameterType == typeof(double)) {
                return factory.Conversion<ConvertToFAction>();
            }

            if (parameterType == typeof(Union<int, MutableString>)) {
                return factory.CompositeConversion(CompositeConversion.ToFixnumToStr);
            }

            if (parameterType == typeof(Union<MutableString, int>)) {
                return factory.CompositeConversion(CompositeConversion.ToStrToFixnum);
            }

            if (parameterType == typeof(RubyRegex)) {
                return factory.Conversion<ConvertToRegexAction>();
            }

            if (parameterType == typeof(IList)) {
                return factory.Conversion<ConvertToArrayAction>();
            }

            if (parameterType == typeof(IDictionary<object, object>)) {
                return factory.Conversion<ConvertToHashAction>();
            }

            return null;
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

        protected abstract bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

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
            params ProtocolConversionAction[]/*!*/ conversions) {
            Assert.NotNull(metaBuilder, args, conversions);
            Debug.Assert(args.SimpleArgumentCount == 0 && !args.Signature.HasBlock && !args.Signature.HasSplattedArgument && !args.Signature.HasRhsArgument);
            Debug.Assert(!args.Signature.HasScope);

            // implicit conversions should only depend on the static type:
            foreach (var conversion in conversions) {
                if (conversion.TryImplicitConversion(metaBuilder, args)) {
                    if (args.Target == null) {
                        metaBuilder.AddRestriction(Ast.Equal(args.TargetExpression, AstUtils.Constant(null, args.TargetExpression.Type)));
                    } else {
                        metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
                    }

                    if (!metaBuilder.Error) {
                        metaBuilder.Result = ConvertResult(metaBuilder.Result, resultType);
                    }
                    return;
                }
            }

            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            Expression targetClassNameConstant = AstUtils.Constant(targetClass.GetNonSingletonClass().Name);
            MethodResolutionResult respondToMethod, methodMissing = MethodResolutionResult.NotFound;
            ProtocolConversionAction selectedConversion = null;
            RubyMemberInfo conversionMethod = null;

            using (targetClass.Context.ClassHierarchyLocker()) {
                // check for type version:
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext);

                // we can optimize if Kernel#respond_to? method is not overridden:
                respondToMethod = targetClass.ResolveMethodForSiteNoLock(Symbols.RespondTo, RubyClass.IgnoreVisibility);
                if (respondToMethod.Found && respondToMethod.Info.DeclaringModule == targetClass.Context.KernelModule && respondToMethod.Info is RubyLibraryMethodInfo) { // TODO: better override detection
                    respondToMethod = MethodResolutionResult.NotFound;

                    // get the first applicable conversion:
                    foreach (var conversion in conversions) {
                        selectedConversion = conversion;
                        conversionMethod = targetClass.ResolveMethodForSiteNoLock(conversion.ToMethodName, RubyClass.IgnoreVisibility).Info;
                        if (conversionMethod != null) {
                            break;
                        } else {
                            // find method_missing - we need to add "to_xxx" methods to the missing methods table:
                            if (!methodMissing.Found) {
                                methodMissing = targetClass.ResolveMethodNoLock(Symbols.MethodMissing, RubyClass.IgnoreVisibility);
                            }
                            methodMissing.InvalidateSitesOnMissingMethodAddition(conversion.ToMethodName, targetClass.Context);
                        }
                    }
                }
            }

            if (!respondToMethod.Found) {
                // TODO: Is MRI consistent on respond_to? visibility?
                //if (respondToMethod.IncompatibleVisibility != RubyMethodVisibility.None) {
                //    // respond_to? is not visible:
                //    conversions[conversions.Length - 1].SetError(metaBuilder, args, targetClassNameConstant, resultType);
                //    return;
                //} else 
                    
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
                MethodInfo validator = conversions[i].ConversionResultValidator;
                
                var conversionCallSite = Ast.Dynamic(
                    RubyCallAction.Make(args.RubyContext, toMethodName, RubyCallSignature.WithImplicitSelf(0)),
                    typeof(object),
                    args.TargetExpression
                );

                metaBuilder.Result = Ast.Condition(
                    // If

                    // respond_to?()
                    Methods.IsTrue.OpCall(
                        Ast.Dynamic(
                            RubyCallAction.Make(args.RubyContext, Symbols.RespondTo, RubyCallSignature.WithImplicitSelf(1)),
                            typeof(object),
                            args.TargetExpression, 
                            AstUtils.Constant(SymbolTable.StringToId(toMethodName))
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

        protected virtual Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return Ast.Throw(
                Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, AstUtils.Constant(TargetTypeName)),
                resultType
            );
        }

        protected virtual void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, AstUtils.Constant(TargetTypeName)));            
        }

        private Expression/*!*/ MakeValidatorCall(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Expression/*!*/ result) {
            var validator = ConversionResultValidator;
            return (validator != null) ? validator.OpCall(targetClassNameConstant, AstFactory.Box(result)) : result;
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

    public abstract class ProtocolConversionAction<TSelf> : ProtocolConversionAction, IExpressionSerializable
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

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Methods.GetMethod(GetType(), "MakeShared").OpCall();
        }
    }

    public abstract class ConvertToReferenceTypeAction<TSelf, TTargetType> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToReferenceTypeAction<TSelf, TTargetType>, new()
        where TTargetType : class {

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            return TryImplicitConversionInternal(metaBuilder, args);
        }

        internal static bool TryImplicitConversionInternal(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                metaBuilder.Result = AstUtils.Constant(null);
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

    public sealed class ConvertToStrAction : ConvertToReferenceTypeAction<ConvertToStrAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args, 
            out MethodInfo postConverter) {
            postConverter = Methods.StringToMutableString;
            return new InteropBinder.Convert(context, typeof(string), true);
        }
    }

    public sealed class TryConvertToStrAction : TryConvertToReferenceTypeAction<TryConvertToStrAction, MutableString> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }
    }

    // TODO: escaping vs. non-escaping?
    // This conversion escapes the regex. 
    public sealed class ConvertToRegexAction : ConvertToReferenceTypeAction<ConvertToRegexAction, RubyRegex> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Regexp"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToRegexValidator; } }
    }

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

    public sealed class ConvertToArraySplatAction : ConvertToReferenceTypeAction<ConvertToArraySplatAction, IList> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }

        // no validation of to_ary result:
        protected override MethodInfo ConversionResultValidator { get { return null; } }

        // return the target object on error:
        protected override Expression/*!*/ MakeErrorExpression(CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            return AstFactory.Box(args.TargetExpression);
        }

        // return the target object on error:
        protected override void SetError(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Expression/*!*/ targetClassNameConstant, Type/*!*/ resultType) {
            metaBuilder.Result = AstFactory.Box(args.TargetExpression);
        }
    }

    public sealed class ConvertToFixnumAction : ProtocolConversionAction<ConvertToFixnumAction> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        protected override string/*!*/ TargetTypeName { get { return "Fixnum"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFixnumValidator; } }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;

            if (target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }
            
            // TODO: other .NET primitive integer types
            if (target is int) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(int));
                return true;
            }

            var bignum = target as BigInteger;
            if ((object)bignum != null) {
                metaBuilder.Result = Methods.ConvertBignumToFixnum.OpCall(AstUtils.Convert(args.TargetExpression, typeof(BigInteger)));
                return true;
            }

            return false;
        }
    }

    public abstract class ConvertToIntegerActionBase<TSelf> : ProtocolConversionAction<TSelf>
        where TSelf : ConvertToIntegerActionBase<TSelf>, new() {

        protected override string/*!*/ TargetTypeName { get { return "Integer"; } }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;

            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }

            // TODO: other .NET primitive integer types
            if (target is int) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(int));
                return true;
            }

            var bignum = target as BigInteger;
            if ((object)bignum != null) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(BigInteger));
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Calls to_int and wraps the result (Fixnum or Bignum) into IntegerValue.
    /// </summary>
    public sealed class ConvertToIntAction : ConvertToIntegerActionBase<ConvertToIntAction> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToIntegerValidator; } }
    }

    /// <summary>
    /// Calls to_i and wraps the result (Fixnum or Bignum) into IntegerValue.
    /// </summary>
    public sealed class ConvertToIAction : ConvertToIntegerActionBase<ConvertToIAction> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToI; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToIntegerValidator; } }
    }

    /// <summary>
    /// Calls to_f (in most cases) and wraps the result into double. It directly calls Kernel.Float for String, Fixnum and Bignum.
    /// </summary>
    public sealed class ConvertToFAction : ProtocolConversionAction<ConvertToFAction> {
        protected override string/*!*/ TargetTypeName { get { return "Float"; } }
        protected override string/*!*/ ToMethodName { get { return Symbols.ToF; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFloatValidator; } }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;

            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(AstUtils.Constant("nil"), AstUtils.Constant(TargetTypeName)));
                return true;
            }

            if (target is double) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(double));
                return true;
            }

            if (target is int) {
                metaBuilder.Result = AstUtils.Convert(AstUtils.Convert(args.TargetExpression, typeof(int)), typeof(double));
                return true;
            }

            if (target is BigInteger) {
                Expression bigInt = AstUtils.Convert(args.TargetExpression, typeof(BigInteger));
                metaBuilder.Result = Ast.Call(Methods.ConvertBignumToFloat, bigInt);
                return true;
            }

            if (target is MutableString) {
                Expression str = AstUtils.Convert(args.TargetExpression, typeof(MutableString));
                metaBuilder.Result = Ast.Call(Methods.ConvertStringToFloat, str);
                return true;
            }

            return false;
        }
    }

    public sealed class ConvertToSymbolAction : ConvertToReferenceTypeAction<ConvertToSymbolAction, string> {
        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Symbol"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSymbolValidator; } }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (base.TryImplicitConversion(metaBuilder, args)) {
                return true;
            }

            object target = args.Target;
            var targetExpression = args.TargetExpression;
            
            var str = target as MutableString;
            if (str != null) {
                metaBuilder.Result = Methods.ConvertMutableStringToSymbol.OpCall(AstUtils.Convert(targetExpression, typeof(MutableString)));
                return true;
            }

            if (target is SymbolId) {
                metaBuilder.Result = Methods.ConvertSymbolIdToSymbol.OpCall(AstUtils.Convert(targetExpression, typeof(SymbolId)));
                return true;
            }

            if (target is int) {
                metaBuilder.Result = Methods.ConvertFixnumToSymbol.OpCall(
                    AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)), 
                    AstUtils.Convert(targetExpression, typeof(int))
                );
                return true;
            }

            return false;
        }
    }
}
