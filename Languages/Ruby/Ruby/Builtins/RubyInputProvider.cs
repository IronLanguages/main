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

using System.Collections.Generic;
using System.Threading;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.IO;

namespace IronRuby.Builtins {
    public sealed class RubyInputProvider {
        private readonly RubyContext/*!*/ _context;

        // $<. ARGF
        private object/*!*/ _singleton;
        //TODO: thread safety
        private RubyIO _singletonStream;
        private IOMode _defaultMode;
        
        // $*, ARGV
        private readonly RubyArray/*!*/ _commandLineArguments;

        //TODO: thread safety
        private int _currentFileIndex;
        // $.
        private int _lastInputLineNumber;

        internal RubyInputProvider(RubyContext/*!*/ context, ICollection<string>/*!*/ arguments, RubyEncoding/*!*/ encoding) {
            Assert.NotNull(context, encoding);
            Assert.NotNullItems(arguments);
            _context = context;

            var args = new RubyArray();
            foreach (var arg in arguments) {
                ExpandArgument(args, arg, encoding);
            }

            _commandLineArguments = args;
            _lastInputLineNumber = 1;
            _currentFileIndex = -1;
            _singleton = new object();
            _defaultMode = IOMode.ReadOnly;
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

        public RubyIO SingletonStream {
            get { return _singletonStream; }
            private set {
                Assert.NotNull(value);
                _singletonStream = value;
            }
        }

        public IOMode DefaultMode {
            get { return _defaultMode; }
            set { _defaultMode = value; }
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
                if (CommandLineArguments.Count == 0) {
                    return MutableString.CreateAscii("-");
                } else {
                    //TODO: convert any non-string
                    return (MutableString)CommandLineArguments[_currentFileIndex];
                }
            }
        }


        public RubyIO GetCurrentStream() {
            return GetCurrentStream(false);
        }

        public RubyIO GetCurrentStream(bool reset) {
            if (null == SingletonStream || (reset && (SingletonStream.Closed || SingletonStream.IsEndOfStream()))){
                IncrementCurrentFileIndex();
                ResetCurrentStream();
            } 
            return SingletonStream;
        }

        public RubyIO GetOrResetCurrentStream() {
            return GetCurrentStream(true);
        }

        public void ResetCurrentStream() {
            string file = CurrentFileName.ToString();
            Stream stream = RubyFile.OpenFileStream(_context, file, _defaultMode);
            SingletonStream = new RubyIO(_context, stream, _defaultMode);
        }


        public void IncrementLastInputLineNumber() {
            Interlocked.Increment(ref _lastInputLineNumber);
        }

        public void IncrementCurrentFileIndex() {
            Interlocked.Increment(ref _currentFileIndex);
        }

        private void ExpandArgument(RubyArray/*!*/ args, string/*!*/ arg, RubyEncoding/*!*/ encoding) {
            if (arg.IndexOf('*') != -1 || arg.IndexOf('?') != -1) {
                bool added = false;
                foreach (string path in Glob.GetMatches(_context.DomainManager.Platform, arg, 0)) {
                    args.Add(MutableString.Create(path, encoding));
                    added = true;
                }

                if (!added) {
                    args.Add(MutableString.Create(arg, encoding));
                }
            } else {
                args.Add(MutableString.Create(arg, encoding));
            }
        }

        public bool HasMoreFiles() {
            return !Interlocked.Equals(_currentFileIndex, _commandLineArguments.Count - 1);
        }
    }
}
