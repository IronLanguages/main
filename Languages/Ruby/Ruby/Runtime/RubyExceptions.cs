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

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using System.Globalization;
using System.Text;
using IronRuby.Compiler;

namespace IronRuby.Runtime {
    /// <summary>
    /// Helper class for creating the corresponding .NET exceptions from the Ruby error names
    /// </summary>
    public static class RubyExceptions {
        public static string FormatMessage(string/*!*/ message, params object[] args) {
            return args != null && args.Length > 0 ? String.Format(CultureInfo.InvariantCulture, message, args) : message;
        }

        #region TypeError (InvalidOperationException)

        public static Exception/*!*/ CreateTypeError(string/*!*/ message, params object[] args) {
            return CreateTypeError(null, message, args);
        }

        public static Exception/*!*/ CreateTypeError(Exception innerException, string/*!*/ message, params object[] args) {
            return new InvalidOperationException(FormatMessage(message, args), innerException);
        }

        public static Exception/*!*/ CreateObjectFrozenError() {
            return CreateRuntimeError("can't modify frozen object");
        }

        public static Exception/*!*/ CreateTypeConversionError(string/*!*/ fromType, string/*!*/ toType) {
            Assert.NotNull(fromType, toType);
            return CreateTypeError("can't convert {0} into {1}", fromType, toType);
        }

        public static Exception/*!*/ CreateUnexpectedTypeError(RubyContext/*!*/ context, object param, string/*!*/ type) {
            return CreateTypeError("wrong argument type {0} (expected {1})", context.GetClassDisplayName(param), type);
        }

        public static Exception/*!*/ CannotConvertTypeToTargetType(RubyContext/*!*/ context, object param, string/*!*/ toType) {
            Assert.NotNull(context, toType);
            return CreateTypeConversionError(context.GetClassName(param), toType);
        }

        public static Exception/*!*/ CreateAllocatorUndefinedError(RubyClass/*!*/ rubyClass) {
            return CreateTypeError("allocator undefined for {0}", rubyClass.Name);
        }

        public static Exception/*!*/ CreateNotClrTypeError(RubyClass/*!*/ rubyClass) {
            return CreateTypeError("`{0}' doesn't represent a CLR type", rubyClass.Name);
        }

        public static Exception/*!*/ CreateNotClrNamespaceError(RubyModule/*!*/ rubyModule) {
            return CreateTypeError("`{0}' doesn't represent a CLR namespace", rubyModule.Name);
        }

        public static Exception/*!*/ CreateMissingDefaultConstructorError(RubyClass/*!*/ rubyClass, string/*!*/ initializerOwnerName) {
            Debug.Assert(rubyClass.IsRubyClass);

            Type baseType = rubyClass.GetUnderlyingSystemType().BaseType;
            Debug.Assert(baseType != null);

            return CreateTypeError("can't allocate class `{1}' that derives from type `{0}' with no default constructor;" +
                " define {1}#new singleton method instead of {2}#initialize",
                rubyClass.Context.GetTypeName(baseType, true), rubyClass.Name, initializerOwnerName
            );
        }

        public static Exception/*!*/ MakeCoercionError(RubyContext/*!*/ context, object self, object other) {
            string selfClass = context.GetClassOf(self).Name;
            string otherClass = context.GetClassOf(other).Name;
            return CreateTypeError("{0} can't be coerced into {1}", selfClass, otherClass);
        }

        public static Exception/*!*/ CreateReturnTypeError(string/*!*/ className, string/*!*/ methodName, string/*!*/ returnTypeName) {
            return CreateTypeError("{0}#{1} should return {2}", className, methodName, returnTypeName);
        }

        #endregion

        #region NameError (MemberAccessException)

        public static Exception/*!*/ CreateNameError(string/*!*/ message, params object[] args) {
            return new MemberAccessException(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateUndefinedMethodError(RubyModule/*!*/ module, string/*!*/ methodName) {
            // MRI doesn't display the singleton's name:
            if (module.IsSingletonClass) {
                module = ((RubyClass)module).GetNonSingletonClass();
            }

            return CreateNameError("undefined method `{0}' for {2} `{1}'",
                methodName, module.Name, module.IsClass ? "class" : "module");
        }

        #endregion

        #region ArgumentError (ArgumentException)

        public static Exception/*!*/ CreateArgumentError(string/*!*/ message, params object[] args) {
            return CreateArgumentError(null, message, args);
        }

        public static Exception/*!*/ CreateArgumentError(Exception innerException, string/*!*/ message, params object[] args) {
            return new ArgumentException(FormatMessage(message, args), innerException);
        }

        public static Exception/*!*/ InvalidValueForType(RubyContext/*!*/ context, object obj, string type) {
            return CreateArgumentError("invalid value for {0}: {1}", type, context.Inspect(obj));
        }

        public static Exception/*!*/ MakeComparisonError(RubyContext/*!*/ context, object self, object other) {
            string selfClass = context.GetClassOf(self).Name;
            string otherClass = context.GetClassOf(other).Name;
            return CreateArgumentError("comparison of {0} with {1} failed", selfClass, otherClass);
        }

        #endregion

        #region Encoding Errors

        public static Exception/*!*/ CreateInvalidByteSequenceError(EncoderFallbackException/*!*/ e, RubyEncoding/*!*/ encoding) {
            return new InvalidByteSequenceError(
                FormatMessage(
                    "character U+{0:X4} can't be encoded in {1}",
                    e.CharUnknownHigh != '\0' ? Tokenizer.ToCodePoint(e.CharUnknownHigh, e.CharUnknownLow) : (int)e.CharUnknown, 
                    encoding
                )
            );
        }

        public static Exception/*!*/ CreateInvalidByteSequenceError(DecoderFallbackException/*!*/ e, RubyEncoding/*!*/ encoding) {
            return new InvalidByteSequenceError(
                FormatMessage("invalid byte sequence {0} on {1}", BitConverter.ToString(e.BytesUnknown), encoding)
            );
        }

        public static Exception/*!*/ CreateTranscodingError(EncoderFallbackException/*!*/ e, RubyEncoding/*!*/ fromEncoding, RubyEncoding/*!*/ toEncoding) {
            return new UndefinedConversionError(
                FormatMessage(
                    "\"{0}\" to UTF-8 in conversion from {1} to UTF-8 to {2}",
                    e.CharUnknown,
                    fromEncoding,
                    toEncoding
                )
            );
        }

        public static Exception/*!*/ CreateTranscodingError(DecoderFallbackException/*!*/ e, RubyEncoding/*!*/ fromEncoding, RubyEncoding/*!*/ toEncoding) {
            throw new UndefinedConversionError(
                FormatMessage(
                    "\"{0}\" to {2} in conversion from {1} to UTF-8 to {2}",
                    BitConverter.ToString(e.BytesUnknown),
                    fromEncoding,
                    toEncoding
                )
            );        
        }

        #endregion

        #region MissingMethodException

        private static Exception/*!*/ CreateMethodMissing(string/*!*/ message) {
            return new MissingMethodException(message);
        }

        public static Exception/*!*/ CreateMethodMissing(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return CreateMethodMissing(FormatMethodMissingMessage(context, self, name));
        }

        public static Exception/*!*/ CreatePrivateMethodCalled(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return CreateMethodMissing(FormatMethodMissingMessage(context, self, name, "private method `{0}' called for {1}"));
        }

        public static Exception/*!*/ CreateProtectedMethodCalled(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return CreateMethodMissing(FormatMethodMissingMessage(context, self, name, "protected method `{0}' called for {1}"));
        }

        public static string/*!*/ FormatMethodMissingMessage(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return FormatMethodMissingMessage(context, self, name, "undefined method `{0}' for {1}");
        }

        [ThreadStatic]
        private static bool _disableMethodMissingMessageFormatting;

        internal static string/*!*/ FormatMethodMissingMessage(RubyContext/*!*/ context, object obj, string/*!*/ name, string/*!*/ message) {
            Assert.NotNull(name);

            string str;
            if (obj == null) {
                str = "nil:NilClass";
            } else if (_disableMethodMissingMessageFormatting) {
                str = RubyUtils.ObjectToMutableString(context, obj).ToString();
            } else {
                _disableMethodMissingMessageFormatting = true;
                try {
                    str = context.Inspect(obj).ConvertToString();
                    if (!str.StartsWith("#", StringComparison.Ordinal)) {
                        str += ":" + context.GetClassName(obj);
                    }
                } catch (Exception) {
                    // MRI: swallows all exceptions
                    str = RubyUtils.ObjectToMutableString(context, obj).ToString();
                } finally {
                    _disableMethodMissingMessageFormatting = false;
                }
            }
            
            return FormatMessage(message, name, str);
        }

        #endregion

        #region LoadError

        public static Exception/*!*/ CreateLoadError(Exception/*!*/ innerException) {
            return new LoadError(innerException.Message, innerException);
        }

        public static Exception/*!*/ CreateLoadError(string/*!*/ message) {
            return new LoadError(message);
        }

        #endregion

        public static Exception/*!*/ CreateNotImplementedError(string/*!*/ message, params object[] args) {
            return new NotImplementedError(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateIndexError(string/*!*/ message, params object[] args) {
            return new IndexOutOfRangeException(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateRangeError(string/*!*/ message, params object[] args) {
            return CreateRangeError("", message, args);
        }

        public static Exception/*!*/ CreateRangeError(string/*!*/ paramName, string/*!*/ message, params object[] args) {
            return new ArgumentOutOfRangeException(paramName, FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateLocalJumpError(string/*!*/ message, params object[] args) {
            return new LocalJumpError(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateRuntimeError(string/*!*/ message, params object[] args) {
            return new RuntimeError(FormatMessage(message, args));
        }

        public static Exception/*!*/ NoBlockGiven() {
            return CreateLocalJumpError("no block given");
        }

        public static Exception/*!*/ CreateIOError(string/*!*/ message) {
            return new IOException(message);
        }

        public static Exception/*!*/ CreateSystemCallError(string/*!*/ message, params object[] args) {
            return new ExternalException(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateSecurityError(string/*!*/ message, params object[] args) {
            throw new SecurityException(FormatMessage(message, args));
        }

        public static Exception/*!*/ CreateEncodingCompatibilityError(RubyEncoding/*!*/ encoding1, RubyEncoding/*!*/ encoding2) {
            return new EncodingCompatibilityError(FormatMessage("incompatible character encodings: {0} and {1}", encoding1.Name, encoding2.Name));
        }

        #region Errno

        // TODO: initialize errno property

        public static string/*!*/ MakeMessage(string message, string/*!*/ baseMessage) {
            Assert.NotNull(baseMessage);
            return (message != null) ? String.Concat(baseMessage, " - ", message) : baseMessage;
        }

        public static string/*!*/ MakeMessage(ref MutableString message, string/*!*/ baseMessage) {
            Assert.NotNull(baseMessage);
            string result = MakeMessage(message != null ? message.ConvertToString() : null, baseMessage);
            message = MutableString.Create(result, message != null ? message.Encoding : RubyEncoding.UTF8);
            return result;
        }

        public static Exception/*!*/ CreateEEXIST() {
            return new ExistError();
        }

        public static Exception/*!*/ CreateEEXIST(string message, params object[] args) {
            return CreateEEXIST(null, message, args);
        }

        public static Exception/*!*/ CreateEEXIST(Exception inner, string/*!*/ message, params object[] args) {
            return new ExistError(FormatMessage(message, args), inner);
        }

        public static Exception/*!*/ CreateEINVAL() {
            return new InvalidError();
        }

        public static Exception/*!*/ CreateEINVAL(string/*!*/ message, params object[] args) {
            return CreateEINVAL(null, message, args);
        }

        public static Exception/*!*/ CreateEINVAL(Exception inner, string/*!*/ message, params object[] args) {
            return new InvalidError(FormatMessage(message, args), inner);
        }

        public static Exception/*!*/ CreateENOENT() {
            return new FileNotFoundException("No such file or directory");
        }

        public static Exception/*!*/ CreateENOENT(string/*!*/ message, params object[] args) {
            return CreateENOENT(null, message, args);
        }

        public static Exception/*!*/ CreateENOENT(Exception inner, string/*!*/ message, params object[] args) {
            return new FileNotFoundException(FormatMessage(message, args), inner);
        }

        public static Exception/*!*/ CreateEBADF() {
            return new BadFileDescriptorError();
        }

        public static Exception/*!*/ CreateEACCES() {
            return new UnauthorizedAccessException();
        }

        #endregion
    }
}
