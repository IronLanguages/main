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

using System.Dynamic.Utils;
using Microsoft.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace System.Linq.Expressions {

    /// <summary>
    /// Describes arguments.
    /// </summary>
    public sealed class CallInfo {
        //
        // all inclusive number of arguments.
        //
        // Requires 
        //    _argCount >= 0
        private readonly int _argCount;

        // argument names in left to right order when applied to metaobject args
        //
        // Foo(x, y, z, name1 = a, name2 = b, name3 = c)
        //
        // will correspond to:
        //    _argCount == 6
        //    _argnames == {"name1", "name2", "name3"} 
        //
        // Requires: 
        //   not null. 
        //   _argNames.Count <= _argCount
        private readonly ReadOnlyCollection<string> _argNames;

        internal CallInfo(int ArgumentCount, ReadOnlyCollection<string> ArgumentNames) {
            _argCount = ArgumentCount;
            _argNames = ArgumentNames;
        }

        /// <summary>
        /// The number of arguments.
        /// </summary>
        public int ArgumentCount {
            get { return _argCount; }
        }

        /// <summary>
        /// The argument names.
        /// </summary>
        public ReadOnlyCollection<string> ArgumentNames {
            get { return _argNames; }
        }

        /// <summary>
        /// Serves as a hash function for the current CallInfo.
        /// </summary>
        /// <returns>A hash code for the current CallInfo.</returns>
        public override int GetHashCode() {
            return _argCount ^ _argNames.ListHashCode();
        }

        /// <summary>
        /// Determines whether the specified CallInfo instance is considered equal to the current.
        /// </summary>
        /// <param name="obj">The instance of CallInfo to compare with the current instance.</param>
        /// <returns>true if the specified instance is equal to the current one otherwise, false.</returns>
        public override bool Equals(object obj) {
            var other = obj as CallInfo;
            return _argCount == other._argCount && _argNames.ListEquals(other._argNames);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Returns a new PositionalArgumentInfo that represents a positional argument in the dynamic binding process.
        /// </summary>
        /// <param name="argCount">The number of arguments.</param>
        /// <param name="argNames">The Argument names.</param>
        /// <returns>The new CallInfo</returns>
        public static CallInfo CallInfo(int argCount, params string[] argNames) {
            return CallInfo(argCount, (IEnumerable<string>)argNames);
        }

        /// <summary>
        /// Returns a new PositionalArgumentInfo that represents a positional argument in the dynamic binding process.
        /// </summary>
        /// <param name="argCount">The number of arguments.</param>
        /// <param name="argNames">The Argument names.</param>
        /// <returns>The new CallInfo</returns>
        public static CallInfo CallInfo(int argCount, IEnumerable<string> argNames) {
            ContractUtils.RequiresNotNull(argNames, "argNames");

            var argNameCol = argNames.ToReadOnly();

            ContractUtils.Requires(argCount >= argNameCol.Count, "argCount", Strings.ArgCntMustBeGreaterThanNameCnt);
            ContractUtils.RequiresNotNullItems(argNameCol, "argNames");

            return new CallInfo(argCount, argNameCol);
        }
    }
}
