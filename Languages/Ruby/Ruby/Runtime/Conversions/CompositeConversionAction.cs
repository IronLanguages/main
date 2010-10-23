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
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime.Conversions {
    using Ast = Expression;
    using System.Collections.Generic;

    public enum CompositeConversion {
        ToFixnumToStr,
        ToStrToFixnum,
        ToIntToI,
        ToAryToInt,
        ToPathToStr,
        ToHashToStr,
    }

    public sealed class CompositeConversionAction : RubyConversionAction {
        private readonly CompositeConversion _conversion;
        private readonly ProtocolConversionAction[]/*!*/ _conversions;
        private readonly Type/*!*/ _resultType;

        private CompositeConversionAction(CompositeConversion conversion, Type/*!*/ resultType, params ProtocolConversionAction[]/*!*/ conversions)
            : base(conversions[0].Context) {
            Assert.NotNullItems(conversions);
            Assert.NotEmpty(conversions);

            _conversions = conversions;
            _conversion = conversion;
            _resultType = resultType;
        }

        public override Type/*!*/ ReturnType {
            get { return _resultType; }
        }

        public static CompositeConversionAction/*!*/ Make(RubyContext/*!*/ context, CompositeConversion/*!*/ conversion) {
            switch (conversion) {
                case CompositeConversion.ToFixnumToStr:
                    return new CompositeConversionAction(conversion,
                        typeof(Union<int, MutableString>), ConvertToFixnumAction.Make(context), ConvertToStrAction.Make(context)
                    );

                case CompositeConversion.ToStrToFixnum:
                    return new CompositeConversionAction(conversion,
                        typeof(Union<MutableString, int>), ConvertToStrAction.Make(context), ConvertToFixnumAction.Make(context)
                    );

                case CompositeConversion.ToIntToI:
                    return new CompositeConversionAction(conversion,
                        typeof(IntegerValue), ConvertToIntAction.Make(context), ConvertToIAction.Make(context)
                    );

                case CompositeConversion.ToAryToInt:
                    return new CompositeConversionAction(conversion,
                        typeof(Union<IList, int>), ConvertToArrayAction.Make(context), ConvertToFixnumAction.Make(context)
                    );

                case CompositeConversion.ToPathToStr:
                    return new CompositeConversionAction(conversion,
                        typeof(MutableString), ConvertToPathAction.Make(context), ConvertToStrAction.Make(context)
                    );

                case CompositeConversion.ToHashToStr:
                    return new CompositeConversionAction(conversion,
                        typeof(Union<IDictionary<object, object>, MutableString>), ConvertToHashAction.Make(context), ConvertToStrAction.Make(context)
                    );

                default:
                    throw Assert.Unreachable;
            }
        }

        [Emitted]
        public static CompositeConversionAction/*!*/ MakeShared(CompositeConversion conversion) {
            return Make(null, conversion);
        }

        public override Expression/*!*/ CreateExpression() {
            return Methods.GetMethod(GetType(), "MakeShared").OpCall(Ast.Constant(_conversion));
        }

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            Debug.Assert(defaultFallback, "custom fallback not supported");
            ProtocolConversionAction.BuildConversion(metaBuilder, args, _resultType, _conversions);
            return true;
        }

        public override string/*!*/ ToString() {
            return _conversion.ToString() + (Context != null ? " @" + Context.RuntimeId.ToString() : null);
        }
    }
}
