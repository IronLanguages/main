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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    // Exception
    // -- fatal
    // -- NoMemoryError                                 
    // -- ScriptError
    // ---- LoadError
    // ---- NotImplementedError
    // ---- SyntaxError
    // -- SignalException (not supported)
    // ---- Interrupt (not supported)
    // -- StandardError
    // ---- ArgumentError
    // ---- IOError
    // ------ EOFError
    // ---- IndexError
    // ---- LocalJumpError
    // ---- NameError
    // ------ NoMethodError
    // ---- RangeError
    // ------ FloatDomainError
    // ---- RegexpError
    // ---- RuntimeError
    // ---- SecurityError
    // ---- SystemCallError
    // ------ system-dependent-exceptions Errno::XXX
    // ---- SystemStackError
    // ---- ThreadError
    // ---- TypeError
    // ---- ZeroDivisionError
    // ---- EncodingError (1.9)
    // ------ Encoding::CompatibilityError (1.9)
    // ------ Encoding::UndefinedConversionError (1.9)
    // ------ Encoding::InvalidByteSequenceError (1.9)
    // ------ Encoding::ConverterNotFoundError (1.9)
    // -- SystemExit
    [RubyException("Exception", Extends = typeof(Exception))]
    public static class ExceptionOps {

        #region Construction

        [Emitted]
        public static string/*!*/ GetClrMessage(RubyClass/*!*/ exceptionClass, object message) {
            return RubyExceptionData.GetClrMessage(exceptionClass.Context, message);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Exception/*!*/ ReinitializeException(RubyContext/*!*/ context, Exception/*!*/ self, [DefaultParameterValue(null)]object message) {
            var instance = RubyExceptionData.GetInstance(self);
            instance.Backtrace = null;
            instance.Message = message ?? MutableString.Create(context.GetClassOf(self).Name, context.GetIdentifierEncoding());
            return self;
        }

        [RubyMethod("exception", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ CreateException() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("backtrace", RubyMethodAttributes.PublicInstance)]
        public static RubyArray GetBacktrace(Exception/*!*/ self) {
            return RubyExceptionData.GetInstance(self).Backtrace;
        }

        [RubyMethod("set_backtrace", RubyMethodAttributes.PublicInstance)]
        public static RubyArray/*!*/ SetBacktrace(Exception/*!*/ self, [NotNull]MutableString/*!*/ backtrace) {
            return RubyExceptionData.GetInstance(self).Backtrace = RubyArray.Create(backtrace);
        }
        
        [RubyMethod("set_backtrace", RubyMethodAttributes.PublicInstance)]
        public static RubyArray SetBacktrace(Exception/*!*/ self, RubyArray backtrace) {
            if (backtrace != null && !CollectionUtils.TrueForAll(backtrace, (item) => item is MutableString)) {
                throw RubyExceptions.CreateTypeError("backtrace must be Array of String");
            }

            return RubyExceptionData.GetInstance(self).Backtrace = backtrace;
        }

        // signature: (Exception! self, [Optional]object arg) : Exception!
        // arg is a message
        [RubyMethod("exception", RubyMethodAttributes.PublicInstance)]
        public static RuleGenerator/*!*/ GetException() {
            return new RuleGenerator((metaBuilder, args, name) => {
                Debug.Assert(args.Target is Exception);

                // 1 optional parameter (exceptionArg):
                var argsBuilder = new ArgsBuilder(0, 0, 0, 1, false);
                argsBuilder.AddCallArguments(metaBuilder, args);

                if (!metaBuilder.Error) {
                    if (argsBuilder.ActualArgumentCount == 0) {
                        metaBuilder.Result = args.TargetExpression;
                    } else {
                        RubyClass cls = args.RubyContext.GetClassOf(args.Target);
                        var classExpression = AstUtils.Constant(cls);
                        args.SetTarget(classExpression, cls);

                        ParameterExpression messageVariable = null;

                        // RubyOps.MarkException(new <exception-type>(GetClrMessage(<class>, #message = <message>)))
                        if (cls.BuildAllocatorCall(metaBuilder, args, () =>
                            Ast.Call(null, new Func<RubyClass, object, string>(GetClrMessage).Method,
                                classExpression,
                                Ast.Assign(messageVariable = metaBuilder.GetTemporary(typeof(object), "#message"), AstUtils.Box(argsBuilder[0]))
                            )
                        )) {
                            // ReinitializeException(<result>, #message)
                            metaBuilder.Result = Ast.Call(null, new Func<RubyContext, Exception, object, Exception>(ReinitializeException).Method,
                                AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)),
                                metaBuilder.Result,
                                messageVariable ?? AstUtils.Box(argsBuilder[0])
                            );
                        } else {
                            metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
                        }
                    }
                }
            });
        }

        [RubyMethod("message")]
        public static object GetMessage(UnaryOpStorage/*!*/ stringReprStorage, Exception/*!*/ self) {
            var site = stringReprStorage.GetCallSite("to_s");
            return site.Target(site, self);
        }

        [RubyMethod("to_s")]
        [RubyMethod("to_str")]
        public static object StringRepresentation(Exception/*!*/ self) {
            return RubyExceptionData.GetInstance(self).Message;
        }

        [RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Inspect(UnaryOpStorage/*!*/ inspectStorage, ConversionStorage<MutableString>/*!*/ tosConversion, Exception/*!*/ self) {
            object message = RubyExceptionData.GetInstance(self).Message;
            string className = inspectStorage.Context.GetClassDisplayName(self);

            MutableString result = MutableString.CreateMutable(inspectStorage.Context.GetIdentifierEncoding());
            result.Append("#<");
            result.Append(className);
            result.Append(": ");
            if (message != null) {
                result.Append(KernelOps.Inspect(inspectStorage, tosConversion, message));
            } else {
                result.Append(className);
            }
            result.Append('>');
            return result;
        }

        #endregion        
    }
}
