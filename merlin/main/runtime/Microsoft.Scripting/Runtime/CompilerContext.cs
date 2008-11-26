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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Represents the context that is flowed for doing Compiler.  Languages can derive
    /// from this class to provide additional contextual information.
    /// </summary>
    public sealed class CompilerContext {

        /// <summary>
        /// Source unit currently being compiled in the CompilerContext
        /// </summary>
        private readonly SourceUnit _sourceUnit;

        /// <summary>
        /// Current error sink.
        /// </summary>
        private readonly ErrorSink _errors;

        /// <summary>
        /// Sink for parser callbacks (e.g. brace matching, etc.).
        /// </summary>
        private readonly ParserSink _parserSink;

        /// <summary>
        /// Compiler specific options.
        /// </summary>
        private readonly CompilerOptions _options;

        public SourceUnit SourceUnit {
            get {
                return _sourceUnit;
            }
        }

        public ParserSink ParserSink {
            get {
                return _parserSink;
            }
        }

        public ErrorSink Errors {
            get { return _errors; }
        }

        public CompilerOptions Options {
            get { return _options; }
        }

        public CompilerContext(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
            : this(sourceUnit, options, errorSink, ParserSink.Null) {
        }

        public CompilerContext(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink, ParserSink parserSink) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
            ContractUtils.RequiresNotNull(errorSink, "errorSink");
            ContractUtils.RequiresNotNull(parserSink, "parserSink");
            ContractUtils.RequiresNotNull(options, "options");

            _sourceUnit = sourceUnit;
            _options = options;
            _errors = errorSink;
            _parserSink = parserSink;
        }
    }
}
