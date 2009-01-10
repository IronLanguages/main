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

using System.Collections.Generic;
using System.Threading;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    public sealed class RubyInputProvider {
        private readonly RubyContext/*!*/ _context;

        // $<. ARGF
        private object/*!*/ _singleton; 
        
        // $*, ARGV
        private readonly RubyArray/*!*/ _commandLineArguments;

        // $.
        private int _lastInputLineNumber;

        internal RubyInputProvider(RubyContext/*!*/ context, ICollection<string>/*!*/ arguments) {
            Assert.NotNull(context);
            Assert.NotNullItems(arguments);
            _context = context;

            var args = new RubyArray();
            foreach (var arg in arguments) {
                ExpandArgument(args, arg);
            }

            _commandLineArguments = args;
            _lastInputLineNumber = 1;
            _singleton = new object();
        }
        
        public RubyContext/*!*/ Context {
            get { return _context; }
        }

        public object/*!*/ Singleton {
            get { return _singleton; }
            // set by environment initializer:
            internal set {
                Assert.NotNull(value);
                _singleton = value; 
            }
        }

        public RubyArray/*!*/ CommandLineArguments {
            get { return _commandLineArguments; }
        }

        public int LastInputLineNumber {
            get { return _lastInputLineNumber; }
            set { _lastInputLineNumber = value; }
        }

        public MutableString/*!*/ CurrentFileName {
            get {
                // TODO:
                return MutableString.Create("-");
            }
        }

        public void IncrementLastInputLineNumber() {
            Interlocked.Increment(ref _lastInputLineNumber);
        }

        private void ExpandArgument(RubyArray/*!*/ args, string/*!*/ arg) {
            if (arg.IndexOf('*') != -1 || arg.IndexOf('?') != -1) {
                bool added = false;
                foreach (string path in Glob.GlobResults(_context, arg, 0)) {
                    args.Add(MutableString.Create(path));
                    added = true;
                }

                if (!added) {
                    args.Add(MutableString.Create(arg));
                }
            } else {
                args.Add(MutableString.Create(arg));
            }
        }
    }
}
