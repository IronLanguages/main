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
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Security;
using IronRuby.Builtins;
using IronRuby.Compiler;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using System.Diagnostics;

namespace IronRuby.Runtime {
    /// <summary>
    /// Helper class for creating the corresponding .NET exceptions from the Ruby error names
    /// </summary>
    public static class RubyExceptions {
        public static Exception/*!*/ CreateTypeError(string/*!*/ message) {
            return new InvalidOperationException(message);
        }

        public static Exception/*!*/ CreateTypeError(string/*!*/ message, Exception innerException) {
            return new InvalidOperationException(message, innerException);
        }

        public static Exception/*!*/ CreateTypeConversionError(string/*!*/ fromType, string/*!*/ toType) {
            Assert.NotNull(fromType, toType);
            return CreateTypeError(String.Format("can't convert {0} into {1}", fromType, toType));
        }

        public static Exception/*!*/ CreateUnexpectedTypeError(RubyContext/*!*/ context, object param, string/*!*/ type) {
            return CreateTypeError(String.Format("wrong argument type {0} (expected {1})", context.GetClassDisplayName(param), type));
        }

        public static Exception/*!*/ CannotConvertTypeToTargetType(RubyContext/*!*/ context, object param, string/*!*/ toType) {
            Assert.NotNull(context, toType);
            return CreateTypeConversionError(context.GetClassName(param), toType);
        }

        public static Exception/*!*/ MethodShouldReturnType(RubyContext/*!*/ context, object param, string/*!*/ method, string/*!*/ targetType) {
            Assert.NotNull(context, method, targetType);
            return new InvalidOperationException(String.Format("{0}#{1} should return {2}",
                context.GetClassName(param), method, targetType
            ));
        }

        public static Exception/*!*/ CreateAllocatorUndefinedError(RubyClass/*!*/ rubyClass) {
            return CreateTypeError(String.Format("allocator undefined for {0}", rubyClass.Name));
        }

        public static Exception/*!*/ CreateMissingDefaultConstructorError(RubyClass/*!*/ rubyClass, string/*!*/ initializerOwnerName) {
            Debug.Assert(rubyClass.IsRubyClass);

            Type baseType = rubyClass.GetUnderlyingSystemType().BaseType;
            Debug.Assert(baseType != null);

            return CreateTypeError(String.Format("can't allocate class `{1}' that derives from type `{0}' with no default constructor;" +
                " define {1}#new singleton method instead of {2}#initialize",
                rubyClass.Context.GetTypeName(baseType, true), rubyClass.Name, initializerOwnerName
            ));
        }

        public static Exception/*!*/ CreateArgumentError(string/*!*/ message) {
            return new ArgumentException(message);
        }

        public static Exception/*!*/ CreateArgumentError(string/*!*/ message, Exception innerException) {
            return new ArgumentException(message, innerException);
        }

        public static Exception/*!*/ CreateNotImplementedError(string/*!*/ message) {
            return new NotImplementedError(message);
        }

        public static Exception/*!*/ CreateNotImplementedError(string/*!*/ message, Exception innerException) {
            return new NotImplementedError(message, innerException);
        }

        public static Exception/*!*/ CreateIndexError(string/*!*/ message) {
            return new IndexOutOfRangeException(message);
        }

        public static Exception/*!*/ CreateIndexError(string/*!*/ message, Exception innerException) {
            return new IndexOutOfRangeException(message, innerException);
        }

        public static Exception/*!*/ CreateRangeError(string/*!*/ message) {
            return new ArgumentOutOfRangeException(String.Empty, message);
        }

        public static Exception/*!*/ CreateNameError(string/*!*/ message) {
            return new MemberAccessException(message);
        }

        public static Exception/*!*/ CreateLocalJumpError(string/*!*/ message) {
            return new LocalJumpError(message);
        }

        public static Exception/*!*/ NoBlockGiven() {
            return new LocalJumpError("no block given");
        }
        
        public static Exception/*!*/ CreateIOError(string/*!*/ message) {
            return new IOException(message);
        }

        public static Exception/*!*/ CreateSystemCallError(string/*!*/ message) {
            return new ExternalException(message);
        }

        public static Exception/*!*/ InvalidValueForType(RubyContext/*!*/ context, object obj, string type) {
            return CreateArgumentError(String.Format("invalid value for {0}: {1}", type, context.Inspect(obj)));
        }

        public static Exception/*!*/ CreateUndefinedMethodError(RubyModule/*!*/ module, string/*!*/ methodName) {
            return RubyExceptions.CreateNameError(String.Format("undefined method `{0}' for {2} `{1}'",
                methodName, module.Name, module.IsClass ? "class" : "module"));
        }

        public static Exception/*!*/ MakeCoercionError(RubyContext/*!*/ context, object self, object other) {
            string selfClass = context.GetClassOf(self).Name;
            string otherClass = context.GetClassOf(other).Name;
            return RubyExceptions.CreateTypeError(String.Format("{0} can't be coerced into {1}", selfClass, otherClass));
        }

        public static Exception/*!*/ MakeComparisonError(RubyContext/*!*/ context, object self, object other) {
            string selfClass = context.GetClassOf(self).Name;
            string otherClass = context.GetClassOf(other).Name;
            return RubyExceptions.CreateArgumentError(String.Format("comparison of {0} with {1} failed", selfClass, otherClass));
        }

        public static Exception/*!*/ CreateSecurityError(string/*!*/ message) {
            throw new SecurityException(message);
        }

        public static string/*!*/ FormatMethodMissingMessage(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return FormatMethodMissingMessage(context, self, name, "undefined method `{0}' for {1}");
        }

        internal static string/*!*/ FormatMethodMissingMessage(RubyContext/*!*/ context, object self, string/*!*/ name, string/*!*/ message) {
            Assert.NotNull(name);
            string strObject = context.InspectEnsuringClassName(self);
            return String.Format(message, name, strObject);
        }

        public static Exception/*!*/ CreateMethodMissing(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return new MissingMethodException(FormatMethodMissingMessage(context, self, name));
        }

        public static Exception/*!*/ CreatePrivateMethodCalled(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return new MissingMethodException(FormatMethodMissingMessage(context, self, name, "private method `{0}' called for {1}"));
        }

        public static Exception/*!*/ CreateProtectedMethodCalled(RubyContext/*!*/ context, object self, string/*!*/ name) {
            return new MissingMethodException(FormatMethodMissingMessage(context, self, name, "protected method `{0}' called for {1}"));
        }

        public static Exception/*!*/ CreateEncodingCompatibilityError(RubyEncoding/*!*/ encoding1, RubyEncoding/*!*/ encoding2) {
            return new EncodingCompatibilityError(String.Format("incompatible character encodings: {0}{1} and {2}{3}",
                encoding1.Name, encoding1.IsKCoding ? " (KCODE)" : null, encoding2.Name, encoding2.IsKCoding ? " (KCODE)" : null));
        }
    }
}
