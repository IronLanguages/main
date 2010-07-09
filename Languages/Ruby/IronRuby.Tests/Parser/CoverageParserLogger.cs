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
using System.Collections.Generic;
using System.IO;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;

namespace IronRuby.Tests {
    /// <summary>
    /// Create a shadow stack, similar to the one used to keep track of which LALR state you are in
    /// 
    /// Whenever you encounter a 'shift' action push 0 on the shadow stack. Since you shift terimal symbols, we can ignore them.
    /// 
    /// Whenever you encounter a 'reduction' action, pop the top n elements off the shadow stack and log them. So, for example 
    /// if you encounter a production which is a sequence of four terminals you get "[ 0 0 0 0 ]".
    /// 
    /// Whenever you encounter a 'goto' action (typically immediately after a reduction), push the production ID on the shadow stack. 
    /// This way, the next time you reduce a production where the non terminal token was you have a production ID instead of 0.
    /// </summary>
    public class CoverageParserLogger : IParserLogger {
        private readonly Parser/*!*/ _parser;
        private readonly TextWriter/*!*/ _output;
        private readonly Stack<int>/*!*/ _rules;

        public CoverageParserLogger(Parser/*!*/ parser, TextWriter/*!*/ output) {
            Assert.NotNull(parser, output);
            _parser = parser;
            _output = output;
            _rules = new Stack<int>();
        }

        public void BeforeReduction(int ruleId, int rhsLength) {
            _output.Write(ruleId);
            _output.Write(" [");
            
            for (int i = 0; i < rhsLength; i++) {
                if (i > 0) _output.Write(' ');
                _output.Write(_rules.Pop());
            }

            _output.WriteLine("]");
        }

        public void BeforeShift(int stateId, int tokenId, bool isErrorShift) {
            _rules.Push(0);
        }

        public void BeforeGoto(int stateId, int ruleId) {
            _rules.Push(ruleId);
        }

        public void StateEntered() {
        }

        public void NextToken(int tokenId) {
        }
    }
}
#endif