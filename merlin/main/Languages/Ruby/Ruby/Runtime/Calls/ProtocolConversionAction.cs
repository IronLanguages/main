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

namespace IronRuby.Runtime.Calls {

    public abstract class ProtocolConversionAction : MetaObjectBinder, IExpressionSerializable {
        internal static readonly RubyCallSignature Signature = RubyCallSignature.WithScope(0);

        protected ProtocolConversionAction() {
        }

        public static ProtocolConversionAction TryGetConversionAction(Type/*!*/ parameterType) {
            if (parameterType == typeof(MutableString)) {
                return ConvertToStrAction.Instance;
            }

            if (parameterType == typeof(int)) {
                return ConvertToFixnumAction.Instance;
            }

            if (parameterType == typeof(string)) {
                return ConvertToSymbolAction.Instance;
            }

            if (parameterType == typeof(RubyRegex)) {
                return ConvertToRegexAction.Instance;
            }

            if (parameterType == typeof(bool)) {
                return ConvertToBooleanAction.Instance;
            }

            if (parameterType == typeof(IList)) {
                return ConvertToArrayAction.Instance;
            }

            return null;
        }

        public override object/*!*/ CacheIdentity {
            get { return this; }
        }

        public override MetaObject/*!*/ Bind(MetaObject/*!*/ context, MetaObject/*!*/[]/*!*/ args) {
            var mo = new MetaObjectBuilder();
            SetRule(mo, new CallArguments(context, args, Signature));
            return mo.CreateMetaObject(this, context, args);
        }

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Ast.Call(GetType().GetMethod("Make"));
        }

        protected abstract string/*!*/ ToMethodName { get; }
        protected abstract MethodInfo ConversionResultValidator { get; }
        protected abstract string/*!*/ TargetTypeName { get; }
        
        protected abstract bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

        internal void SetRule(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            Assert.NotNull(metaBuilder, args);
            Debug.Assert(args.SimpleArgumentCount == 0 && !args.Signature.HasBlock && !args.Signature.HasSplattedArgument && !args.Signature.HasRhsArgument);
            Debug.Assert(args.Signature.HasScope);

            var ec = args.RubyContext;

            // implicit conversions should only depend on a static type:
            if (TryImplicitConversion(metaBuilder, args)) {
                if (args.Target == null) {
                    metaBuilder.AddRestriction(Ast.Equal(args.TargetExpression, Ast.Constant(null, args.TargetExpression.Type)));
                } else {
                    metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
                }
                return;
            }

            // check for type version:
            metaBuilder.AddTargetTypeTest(args);

            string toMethodName = ToMethodName;
            Expression targetClassNameConstant = Ast.Constant(ec.GetClassOf(args.Target).Name);

            // Kernel#respond_to? method is not overridden => we can optimize
            RubyMemberInfo respondToMethod = ec.ResolveMethod(args.Target, Symbols.RespondTo, true).InvalidateSitesOnOverride();
            if (respondToMethod == null ||
                // the method is defined in library, hasn't been replaced by user defined method (TODO: maybe we should make this check better)
                (respondToMethod.DeclaringModule == ec.KernelModule && respondToMethod is RubyMethodGroupInfo)) {

                RubyMemberInfo conversionMethod = ec.ResolveMethod(args.Target, toMethodName, false).InvalidateSitesOnOverride();
                if (conversionMethod == null) {
                    // error:
                    SetError(metaBuilder, targetClassNameConstant, args);
                    return;
                } else {
                    // invoke target.to_xxx() and validate it; returns an instance of TTargetType:
                    conversionMethod.BuildCall(metaBuilder, args, toMethodName);

                    if (!metaBuilder.Error && ConversionResultValidator != null) {
                        metaBuilder.Result = ConversionResultValidator.OpCall(targetClassNameConstant, AstFactory.Box(metaBuilder.Result));
                    }
                    return;
                }
            } else if (!RubyModule.IsMethodVisible(respondToMethod, false)) {
                // respond_to? is private:
                SetError(metaBuilder, targetClassNameConstant, args);
                return;
            }

            // slow path: invoke respond_to?, to_xxx and result validation:

            var conversionCallSite = Ast.Dynamic(
                RubyCallAction.Make(toMethodName, RubyCallSignature.WithScope(0)),
                typeof(object),
                args.ScopeExpression, args.TargetExpression
            );

            Expression opCall;
            metaBuilder.Result = Ast.Condition(
                // If

                // respond_to?()
                Methods.IsTrue.OpCall(
                    Ast.Dynamic(
                        RubyCallAction.Make(Symbols.RespondTo, RubyCallSignature.WithScope(1)),
                        typeof(object),
                        args.ScopeExpression, args.TargetExpression, Ast.Constant(SymbolTable.StringToId(toMethodName))
                    )
                ),

                // Then

                // to_xxx():
                opCall = (ConversionResultValidator == null) ? conversionCallSite : 
                    ConversionResultValidator.OpCall(targetClassNameConstant, conversionCallSite),

                // Else

                AstUtils.Convert(
                    (ConversionResultValidator == null) ? args.TargetExpression :
                        AstUtils.Convert(
                            Ast.Throw(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, Ast.Constant(TargetTypeName))),
                            typeof(object)
                        ), 
                    opCall.Type
                )
            );
        }

        private void SetError(MetaObjectBuilder/*!*/ metaBuilder, Expression/*!*/ targetClassNameConstant, CallArguments/*!*/ args) {
            if (ConversionResultValidator != null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, Ast.Constant(TargetTypeName)));
            } else {
                metaBuilder.Result = args.TargetExpression;
            }
        }
    }

    public abstract class ConvertToReferenceTypeAction<TTargetType> : ProtocolConversionAction where TTargetType : class {
        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                metaBuilder.Result = Ast.Constant(null);
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

    public sealed class ConvertToProcAction : ConvertToReferenceTypeAction<Proc>, IEquatable<ConvertToProcAction> {
        public static readonly ConvertToProcAction Instance = new ConvertToProcAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToProc; } }
        protected override string/*!*/ TargetTypeName { get { return "Proc"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToProcValidator; } }

        private ConvertToProcAction() {
        }

        [Emitted]
        public static ConvertToProcAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToProcAction other) {
            return other != null;
        }
    }

    public sealed class ConvertToStrAction : ConvertToReferenceTypeAction<MutableString>, IEquatable<ConvertToStrAction> {
        public static readonly ConvertToStrAction Instance = new ConvertToStrAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToStringValidator; } }

        private ConvertToStrAction() {
        }

        [Emitted]
        public static ConvertToStrAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToStrAction other) {
            return other != null;
        }
    }

    // TODO: escaping vs. non-escaping?
    // This conversion escapes the regex. 
    public sealed class ConvertToRegexAction : ConvertToReferenceTypeAction<RubyRegex>, IEquatable<ConvertToRegexAction> {
        public static readonly ConvertToRegexAction Instance = new ConvertToRegexAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Regexp"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToRegexValidator; } }

        private ConvertToRegexAction() {
        }

        [Emitted]
        public static ConvertToRegexAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToRegexAction other) {
            return other != null;
        }
    }

    public sealed class ConvertToArrayAction : ConvertToReferenceTypeAction<IList>, IEquatable<ConvertToArrayAction> {
        public static readonly ConvertToArrayAction Instance = new ConvertToArrayAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToArrayValidator; } }
        
        private ConvertToArrayAction() {
        }

        [Emitted]
        public static ConvertToArrayAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToArrayAction other) {
            return other != null;
        }
    }

    public sealed class TryConvertToArrayAction : ConvertToReferenceTypeAction<IList>, IEquatable<TryConvertToArrayAction> {
        public static readonly TryConvertToArrayAction Instance = new TryConvertToArrayAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToAry; } }
        protected override string/*!*/ TargetTypeName { get { return "Array"; } }
        protected override MethodInfo ConversionResultValidator { get { return null; } }

        private TryConvertToArrayAction() {
        }

        [Emitted]
        public static TryConvertToArrayAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(TryConvertToArrayAction other) {
            return other != null;
        }
    }

    public sealed class ConvertToSAction : ConvertToReferenceTypeAction<MutableString>, IEquatable<ConvertToSAction> {
        public static readonly ConvertToSAction Instance = new ConvertToSAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToS; } }
        protected override string/*!*/ TargetTypeName { get { return "String"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSValidator; } }

        private ConvertToSAction() {
        }

        [Emitted]
        public static ConvertToSAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToSAction other) {
            return other != null;
        }
    }

    public sealed class ConvertToFixnumAction : ProtocolConversionAction, IEquatable<ConvertToFixnumAction> {
        public static readonly ConvertToFixnumAction Instance = new ConvertToFixnumAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        protected override string/*!*/ TargetTypeName { get { return "Fixnum"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFixnumValidator; } }

        private ConvertToFixnumAction() {
        }

        [Emitted]
        public static ConvertToFixnumAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToFixnumAction other) {
            return other != null;
        }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;
            
            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(Ast.Constant("nil"), Ast.Constant(TargetTypeName)));
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

    public sealed class ConvertToBooleanAction : ProtocolConversionAction, IEquatable<ConvertToBooleanAction> {
        public static readonly ConvertToBooleanAction Instance = new ConvertToBooleanAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToInt; } }
        protected override string/*!*/ TargetTypeName { get { return "Fixnum"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToFixnumValidator; } }

        private ConvertToBooleanAction() {
        }

        [Emitted]
        public static ConvertToBooleanAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToBooleanAction other) {
            return other != null;
        }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;

            if (args.Target == null) {
                metaBuilder.Result = Ast.Constant(false);
                return true;
            } 
            
            if (target is bool) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(bool));
                return true;
            }

            return true;
        }
    }


    public sealed class ConvertToSymbolAction : ProtocolConversionAction, IEquatable<ConvertToSymbolAction> {
        public static readonly ConvertToSymbolAction Instance = new ConvertToSymbolAction();

        protected override string/*!*/ ToMethodName { get { return Symbols.ToStr; } }
        protected override string/*!*/ TargetTypeName { get { return "Symbol"; } }
        protected override MethodInfo ConversionResultValidator { get { return Methods.ToSymbolValidator; } }

        private ConvertToSymbolAction() {
        }

        [Emitted]
        public static ConvertToSymbolAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToSymbolAction other) {
            return other != null;
        }

        protected override bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            object target = args.Target;
            var targetExpression = args.TargetExpression;
            
            if (args.Target == null) {
                metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(Ast.Constant("nil"), Ast.Constant(TargetTypeName)));
                return true;
            }

            var str = target as MutableString;
            if (str != null) {
                metaBuilder.Result = Methods.ConvertMutableStringToSymbol.OpCall(AstUtils.Convert(targetExpression, typeof(MutableString)));
                return true;
            }

            var sym = target as string;
            if (sym != null) {
                metaBuilder.Result = AstUtils.Convert(targetExpression, typeof(string));
                return true;
            }

            if (target is SymbolId) {
                metaBuilder.Result = Methods.ConvertSymbolIdToSymbol.OpCall(AstUtils.Convert(targetExpression, typeof(SymbolId)));
                return true;
            }

            if (target is int) {
                metaBuilder.Result = Methods.ConvertFixnumToSymbol.OpCall(args.ContextExpression, AstUtils.Convert(targetExpression, typeof(int)));
                return true;
            }

            return false;
        }
    }
}
