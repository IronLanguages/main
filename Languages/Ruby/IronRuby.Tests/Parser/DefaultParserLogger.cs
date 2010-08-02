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

#if DEBUG
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;

namespace IronRuby.Tests {
    internal class DefaultParserLogger : IParserLogger {
        private readonly Parser/*!*/ _parser;
        private int _logVerbosity; // 0 means logging disabled
        private TextWriter _log;

        public DefaultParserLogger(Parser/*!*/ parser, int verbosity, TextWriter/*!*/ output) {
            Assert.NotNull(parser, output);
            Debug.Assert(verbosity > 0 && verbosity <= 2);

            _parser = parser;
            _logVerbosity = verbosity;
            _log = output;
        }

        public void BeforeReduction(int ruleId, int rhsLength) {
            LogRule("Reducing by rule", ruleId);
        }

        public void BeforeShift(int stateId, int tokenId, bool isErrorShift) {
            LogToken(isErrorShift ? "Error shift" : "Shifting token", tokenId);
        }

        public void BeforeGoto(int stateId, int ruleId) {
        }

        public void StateEntered() {
            LogState("Entering state", _parser.CurrentState.Id);
        }

        public void NextToken(int tokenId) {
            LogToken("Next token", tokenId);
        }


        private void LogRule(string message, int ruleId) {
            Log("{0} {1}: {2}", message, ruleId, _parser.RuleToString(ruleId));
        }

        private void LogToken(string message, int tokenId) {
            Log("{0}: {1}", message, Parser.GetTerminalName(tokenId));
        }

        private void LogState(string/*!*/ action, int stateId) {
            Log("{0} {1} ", action, stateId);
        }

        private void Log(string/*!*/ format, params object[] args) {
            if (_logVerbosity > 0) {
                _log.WriteLine(format, args);
            }
        }

        private void DumpStack() {
            if (_logVerbosity > 1) {
                _log.WriteLine("State stack:");
                foreach (State state in _parser.Stack.GetStates()) {
                    _log.WriteLine(state.Id);
                }
                _log.WriteLine();
            }
        }

        public static void Attach(Parser/*!*/ parser) {
            Attach(parser, Console.Out);
        }

        public static void Attach(Parser/*!*/ parser, TextWriter/*!*/ output) {
            parser.EnableLogging(new DefaultParserLogger(parser, 1, output));
        }
    }
}
#endif