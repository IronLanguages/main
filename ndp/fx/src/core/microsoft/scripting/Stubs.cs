/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Dynamic.Utils;

#if SILVERLIGHT // Stubs

namespace System {

    public class ApplicationException : Exception {
        private const int error = unchecked((int)0x80131600);
        // Methods
        public ApplicationException()
            : base("Application Exception") {
            HResult = error;
        }

        public ApplicationException(string message)
            : base(message) {
            HResult = error;
        }

        public ApplicationException(string message, Exception innerException)
            : base(message, innerException) {
            HResult = error;
        }
    }

    namespace Runtime.InteropServices {
        public sealed class DefaultParameterValueAttribute : Attribute {
            public DefaultParameterValueAttribute(object value) { }
        }
    }

    // We reference these namespaces via "using"
    // We don't actually use them because the code is #if !SILVERLIGHT
    // Rather than fix the usings all over the place, just define these here
    namespace Runtime.Remoting { class Dummy {} }
    namespace Security.Policy { class Dummy {} }
    namespace Xml.XPath { class Dummy {} }

    namespace Reflection {
        public enum PortableExecutableKinds {
            ILOnly = 0
        }

        public enum ImageFileMachine {
            I386 = 1
        }
    }

    namespace ComponentModel {

        public class WarningException : SystemException {
            public WarningException(string message) : base(message) { }
        }
    }

    public class SerializableAttribute : Attribute {
    }

    public class NonSerializedAttribute : Attribute {
    }

    namespace Runtime.Serialization {
        public interface ISerializable {
        }
    }

    public enum ConsoleColor {
        Black = 0,
        DarkBlue = 1,
        DarkGreen = 2,
        DarkCyan = 3,
        DarkRed = 4,
        DarkMagenta = 5,
        DarkYellow = 6,
        Gray = 7,
        DarkGray = 8,
        Blue = 9,
        Green = 10,
        Cyan = 11,
        Red = 12,
        Magenta = 13,
        Yellow = 14,
        White = 15,
    }

}

#endif

#if !SPECSHARP

namespace Microsoft.Contracts {
    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class StateIndependentAttribute : Attribute {
    }

#if MICROSOFT_SCRIPTING_CORE
    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class PureAttribute : Attribute {
    }
#endif

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class ConfinedAttribute : Attribute {
    }

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Field)]
    internal sealed class StrictReadonlyAttribute : Attribute {
    }

    internal static class NonNullType {
        [DebuggerStepThrough]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static void AssertInitialized<T>(T[] array) where T : class {
            Assert.NotNullItems<T>(array);
        }
    }
}

#endif