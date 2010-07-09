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

using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Represents the reason why a call to a specific method could not be performed by the OverloadResolver.
    /// 
    /// The reason for the failure is specified by the CallFailureReason property.  Once this property
    /// has been consulted the other properties can be consulted for more detailed information regarding
    /// the failure.
    /// 
    /// If reason is ConversionFailure the ConversionResults property will be non-null.
    /// If reason is UnassignableKeyword the KeywordArguments property will be non-null and include
    ///     the keywords which could not be assigned.
    /// If reason is DuplicateKeyword the KeywordArguments property will be non-null and include
    ///     the keywords which were duplicated (either by the keywords themselves or by positional
    ///     arguments).
    ///     
    /// MethodTarget is always set and indicates the method which failed to bind.
    /// </summary>
    public sealed class CallFailure {
        private readonly CallFailureReason _reason;      // the reason the call failed
        private readonly MethodCandidate _candidate;
        private readonly ConversionResult[] _results;
        private readonly string[] _keywordArgs;

        internal CallFailure(MethodCandidate candidate, ConversionResult[] results) {
            _candidate = candidate;
            _results = results;
            _reason = CallFailureReason.ConversionFailure;
        }

        internal CallFailure(MethodCandidate candidate, string[] keywordArgs, bool unassignable) {
            _reason = unassignable ? CallFailureReason.UnassignableKeyword : CallFailureReason.DuplicateKeyword;
            _candidate = candidate;
            _keywordArgs = keywordArgs;
        }

        internal CallFailure(MethodCandidate candidate, CallFailureReason reason) {
            _candidate = candidate;
            _reason = reason;
        }

        /// <summary>
        /// Gets the MethodTarget which the call failed for.
        /// </summary>
        public MethodCandidate Candidate {
            get {
                return _candidate;
            }
        }

        /// <summary>
        /// Gets the reason for the call failure which determines the other 
        /// properties of the CallFailure which should be consulted.
        /// </summary>
        public CallFailureReason Reason {
            get{
                return _reason;
            }
        }

        /// <summary>
        /// Gets a list of ConversionResult's for each parameter indicating
        /// whether the conversion was successful or failed and the types
        /// being converted.
        /// </summary>
        public IList<ConversionResult> ConversionResults {
            get {
                return _results;
            }
        }

        /// <summary>
        /// Gets the list of keyword arguments that were either dupliated or
        /// unassignable.
        /// </summary>
        public IList<string> KeywordArguments {
            get {
                return _keywordArgs;
            }
        }
    }
}
