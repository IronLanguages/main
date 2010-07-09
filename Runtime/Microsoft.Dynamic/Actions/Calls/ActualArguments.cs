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

using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using System.Collections;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.Calls {
    public sealed class ActualArguments {
        private readonly IList<DynamicMetaObject> _args;     
        private readonly IList<DynamicMetaObject> _namedArgs;
        private readonly IList<string> _names;             
        
        // Index into _args array indicating the first post-splat argument or -1 of there are no splatted arguments.
        // For call site f(a,b,*c,d) and preSplatLimit == 1 and postSplatLimit == 2
        // args would be (a,b,c[0],c[n-2],c[n-1],d) with splat index 3, where n = c.Count.
        private readonly int _splatIndex;
        private readonly int _firstSplattedArg;
        private readonly int _collapsedCount;

        // The number of hidden arguments (used for error reporting).
        private readonly int _hiddenCount;

        public ActualArguments(IList<DynamicMetaObject> args, IList<DynamicMetaObject> namedArgs, IList<string> argNames,
            int hiddenCount, int collapsedCount, int firstSplattedArg, int splatIndex) {

            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.RequiresNotNullItems(namedArgs, "namedArgs");
            ContractUtils.RequiresNotNullItems(argNames, "argNames");
            ContractUtils.Requires(namedArgs.Count == argNames.Count);

            ContractUtils.Requires(splatIndex == -1 || firstSplattedArg == -1 || firstSplattedArg >= 0 && firstSplattedArg <= splatIndex);
            ContractUtils.Requires(splatIndex == -1 || splatIndex >= 0);
            ContractUtils.Requires(collapsedCount >= 0);
            ContractUtils.Requires(hiddenCount >= 0);

            _args = args;
            _namedArgs = namedArgs;
            _names = argNames;
            _collapsedCount = collapsedCount;
            _splatIndex = collapsedCount > 0 ? splatIndex : -1;
            _firstSplattedArg = firstSplattedArg;
            _hiddenCount = hiddenCount;
        }

        public int CollapsedCount {
            get { return _collapsedCount; }
        }

        public int SplatIndex {
            get { return _splatIndex; }
        }

        public int FirstSplattedArg {
            get { return _firstSplattedArg; }
        }

        public IList<string> ArgNames {
            get { return _names; }
        }

        public IList<DynamicMetaObject> NamedArguments {
            get { return _namedArgs; }
        }

        public IList<DynamicMetaObject> Arguments {
            get { return _args; }
        }

        internal int ToSplattedItemIndex(int collapsedArgIndex) {
            return _splatIndex - _firstSplattedArg + collapsedArgIndex;
        }

        /// <summary>
        /// The number of arguments not counting the collapsed ones.
        /// </summary>
        public int Count {
            get { return _args.Count + _namedArgs.Count; }
        }

        public int HiddenCount {
            get { return _hiddenCount; }
        }

        /// <summary>
        /// Gets the total number of visible arguments passed to the call site including collapsed ones.
        /// </summary>
        public int VisibleCount {
            get { return Count + _collapsedCount - _hiddenCount; }
        }

        public DynamicMetaObject this[int index] {
            get {
                return (index < _args.Count) ? _args[index] : _namedArgs[index - _args.Count];
            }
        }

        /// <summary>
        /// Binds named arguments to the parameters. Returns a permutation of indices that captures the relationship between 
        /// named arguments and their corresponding parameters. Checks for duplicate and unbound named arguments.
        /// 
        /// Ensures that for all i: namedArgs[i] binds to parameters[args.Length + bindingPermutation[i]] 
        /// </summary>
        internal bool TryBindNamedArguments(MethodCandidate method, out ArgumentBinding binding, out CallFailure failure) {
            if (_namedArgs.Count == 0) {
                binding = new ArgumentBinding(_args.Count);
                failure = null;
                return true;
            }

            var permutation = new int[_namedArgs.Count];
            var boundParameters = new BitArray(_namedArgs.Count);

            for (int i = 0; i < permutation.Length; i++) {
                permutation[i] = -1;
            }

            List<string> unboundNames = null;
            List<string> duppedNames = null;

            int positionalArgCount = _args.Count;

            for (int i = 0; i < _names.Count; i++) {
                int paramIndex = method.IndexOfParameter(_names[i]);
                if (paramIndex >= 0) {
                    int nameIndex = paramIndex - positionalArgCount;

                    // argument maps to already bound parameter:
                    if (paramIndex < positionalArgCount || boundParameters[nameIndex]) {
                        if (duppedNames == null) {
                            duppedNames = new List<string>();
                        }
                        duppedNames.Add(_names[i]);
                    } else {
                        permutation[i] = nameIndex;
                        boundParameters[nameIndex] = true;
                    }
                } else {
                    if (unboundNames == null) {
                        unboundNames = new List<string>();
                    }
                    unboundNames.Add(_names[i]);
                }
            }

            binding = new ArgumentBinding(positionalArgCount, permutation);

            if (unboundNames != null) {
                failure = new CallFailure(method, unboundNames.ToArray(), true);
                return false;
            } else if (duppedNames != null) {
                failure = new CallFailure(method, duppedNames.ToArray(), false);
                return false;
            }

            failure = null;
            return true;
        }
    }
}
