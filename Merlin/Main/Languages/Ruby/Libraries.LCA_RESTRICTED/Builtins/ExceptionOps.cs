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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;
using IronRuby.Compiler.Generation;

using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Linq.Expressions;

namespace IronRuby.Builtins {

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
    // -- SystemExit
    [RubyException("Exception", Extends = typeof(Exception))]
    public static class ExceptionOps {

        #region Construction

        [Emitted]
        public static string/*!*/ GetClrMessage(RubyClass/*!*/ exceptionClass, object message) {
            return RubyExceptionData.GetClrMessage(message, exceptionClass.Name);
        }

        [Emitted]
        public static Exception/*!*/ ReinitializeException(Exception/*!*/ self, object/*!*/ message) {
            var instance = RubyExceptionData.GetInstance(self);
            instance.Backtrace = null;
            instance.Message = message;
            return self;
        }
        
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Exception/*!*/ ReinitializeException(RubyContext/*!*/ context, Exception/*!*/ self, [DefaultParameterValue(null)]object message) {
            return ReinitializeException(self, message ?? context.GetClassOf(self).Name);
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

        // signature: (Exception! self, [Optional]object exceptionArg) : Exception!
        [RubyMethod("exception", RubyMethodAttributes.PublicInstance)]
        public static RuleGenerator/*!*/ GetException() {
            return new RuleGenerator((metaBuilder, args, name) => {
                Debug.Assert(args.Target is Exception);

                // 1 optional parameter (exceptionArg):
                var argsBuilder = new ArgsBuilder(0, 0, 1, false);
                argsBuilder.AddCallArguments(metaBuilder, args);

                if (!metaBuilder.Error) {
                    if (argsBuilder.ExplicitArgumentCount == 0) {
                        metaBuilder.Result = args.TargetExpression;
                    } else {
                        RubyClass cls = args.RubyContext.GetClassOf(args.Target);
                        var classExpression = AstUtils.Constant(cls);
                        args.SetTarget(classExpression, cls);

                        ParameterExpression messageVariable = null;

                        // ReinitializeException(new <exception-type>(GetClrMessage(<class>, #message = <message>)), #message)
                        metaBuilder.Result = Ast.Call(null, new Func<Exception, object, Exception>(ReinitializeException).Method, 
                            cls.MakeAllocatorCall(args, () => 
                                Ast.Call(null, new Func<RubyClass, object, string>(GetClrMessage).Method, 
                                    classExpression, 
                                    Ast.Assign(messageVariable = metaBuilder.GetTemporary(typeof(object), "#message"), AstFactory.Box(argsBuilder[0]))
                                )
                            ),
                            messageVariable ?? AstFactory.Box(argsBuilder[0])
                        );
                    }
                }
            });
        }

        [RubyMethod("message")]
        public static object GetMessage(Exception/*!*/ self) {
            return RubyExceptionData.GetInstance(self).Message;
        }

        [RubyMethod("to_s")]
        [RubyMethod("to_str")]
        public static MutableString/*!*/ GetMessage(ConversionStorage<MutableString>/*!*/ tosStorage, RubyContext/*!*/ context, Exception/*!*/ self) {
            return Protocols.ConvertToString(tosStorage, context, GetMessage(self));
        }

        [RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Inspect(UnaryOpStorage/*!*/ inspectStorage, ConversionStorage<MutableString>/*!*/ tosStorage,
            RubyContext/*!*/ context, Exception/*!*/ self) {

            object message = RubyExceptionData.GetInstance(self).Message;
            string className = RubyUtils.GetClassName(context, self);

            MutableString result = MutableString.CreateMutable();
            result.Append("#<");
            result.Append(className);
            result.Append(": ");
            if (message != null) {
                result.Append(KernelOps.Inspect(inspectStorage, tosStorage, context, message));
            } else {
                result.Append(className);
            }
            result.Append('>');
            return result;
        }

        #endregion        
    }
}
