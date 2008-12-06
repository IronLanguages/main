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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    //
    // Ruby call targets
    //
    
    /// <summary>
    /// The delegate representing the Ruby code entry point
    /// </summary>
    public delegate object RubyMainDelegate();

    /// <summary>
    /// Generic delegate type for block with >RubyCallTargets.MaxSignatureSize parameters
    /// </summary>
    public delegate object RubyCallTargetN(params object[] args);

    static class RubyCallTargets {

        internal const int MaxSignatureSize = 10;

        internal static Type GetDelegateType(Type[] arguments, Type returnType) {
            Assert.NotNull(arguments, returnType);
            Type result;

            if (returnType == typeof(void)) {
                switch (arguments.Length) {
                    case 0: return typeof(Action);
                    case 1: result = typeof(Action<>); break;
                    case 2: result = typeof(Action<,>); break;
                    case 3: result = typeof(Action<,,>); break;
                    case 4: result = typeof(Action<,,,>); break;
                    case 5: result = typeof(Action<,,,,>); break;
                    case 6: result = typeof(Action<,,,,,>); break;
                    case 7: result = typeof(Action<,,,,,,>); break;
                    case 8: result = typeof(Action<,,,,,,,>); break;
                    case 9: result = typeof(Action<,,,,,,,,>); break;
                    case 10: result = typeof(Action<,,,,,,,,,>); break;
                    default:
                        throw new NotImplementedException("Action delegate not implemented for " + arguments.Length + " arguments.");
                }
            } else {
                arguments = ArrayUtils.Append(arguments, returnType);
                switch (arguments.Length) {
                    case 0: throw Assert.Unreachable;
                    case 1: result = typeof(Func<>); break;
                    case 2: result = typeof(Func<,>); break;
                    case 3: result = typeof(Func<,,>); break;
                    case 4: result = typeof(Func<,,,>); break;
                    case 5: result = typeof(Func<,,,,>); break;
                    case 6: result = typeof(Func<,,,,,>); break;
                    case 7: result = typeof(Func<,,,,,,>); break;
                    case 8: result = typeof(Func<,,,,,,,>); break;
                    case 9: result = typeof(Func<,,,,,,,,>); break;
                    case 10: result = typeof(Func<,,,,,,,,,>); break;
                    case 11: result = typeof(Func<,,,,,,,,,,>); break;
                    default:
                        throw new NotImplementedException("Function delegate not implemented for " + arguments.Length + " arguments.");
                }
            }

            return result.MakeGenericType(arguments);
        }

    }
}
