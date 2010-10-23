/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SPECSHARP

using System;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Contracts {
    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class StateIndependentAttribute : Attribute {
    }

    [Conditional("SPECSHARP"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    internal sealed class PureAttribute : Attribute {
    }

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