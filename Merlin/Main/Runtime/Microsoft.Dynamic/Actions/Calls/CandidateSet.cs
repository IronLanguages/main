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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Represents a collection of MethodCandidate's which all accept the
    /// same number of logical parameters.  For example a params method
    /// and a method with 3 parameters would both be a CandidateSet for 3 parameters.
    /// </summary>
    internal sealed class CandidateSet {
        private readonly int _arity;
        private readonly List<MethodCandidate> _candidates;

        internal CandidateSet(int count) {
            _arity = count;
            _candidates = new List<MethodCandidate>();
        }

        internal List<MethodCandidate> Candidates {
            get { return _candidates; }
        }

        internal int Arity {
            get { return _arity; }
        }

        internal bool IsParamsDictionaryOnly() {
            foreach (MethodCandidate candidate in _candidates) {
                if (!candidate.HasParamsDictionary) {
                    return false;
                }
            }
            return true;
        }

        internal void Add(MethodCandidate target) {
            Debug.Assert(target.ParameterCount == _arity);
            _candidates.Add(target);
        }

        [Confined]
        public override string ToString() {
            return string.Format("{0}: ({1} on {2})", _arity, _candidates[0].Overload.Name, _candidates[0].Overload.DeclaringType.FullName);
        }
    }
}
