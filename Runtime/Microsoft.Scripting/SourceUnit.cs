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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Scripting {
    [DebuggerDisplay("{_path ?? \"<anonymous>\"}")]
    public sealed class SourceUnit {
        private readonly SourceCodeKind _kind;
        private readonly string _path;
        private readonly LanguageContext _language;
        private readonly TextContentProvider _contentProvider;

        // SourceUnit is serializable => updated parse result is transmitted
        // back to the host unless the unit is passed by-ref
        private ScriptCodeParseResult? _parseResult;
        private KeyValuePair<int, int>[] _lineMap;

        /// <summary>
        /// Identification of the source unit. Assigned by the host. 
        /// The format and semantics is host dependent (could be a path on file system or URL).
        /// Empty string for anonymous source units.
        /// </summary>
        public string Path {
            get { return _path; }
        }

        public bool HasPath {
            get { return _path != null; }
        }

        public SourceCodeKind Kind {
            get { return _kind; }
        }

        public SymbolDocumentInfo Document {
            get {
                // _path is valid to be null. In that case we cannot create a valid SymbolDocumentInfo.
                return _path == null ? null : Expression.SymbolDocument(_path, _language.LanguageGuid, _language.VendorGuid);
            }
        }

        /// <summary>
        /// LanguageContext of the language of the unit.
        /// </summary>
        public LanguageContext LanguageContext {
            get { return _language; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ScriptCodeParseResult GetCodeProperties() {
            return GetCodeProperties(_language.GetCompilerOptions());
        }

        public ScriptCodeParseResult GetCodeProperties(CompilerOptions options) {
            ContractUtils.RequiresNotNull(options, "options");

            _language.CompileSourceCode(this, options, ErrorSink.Null);
            return _parseResult ?? ScriptCodeParseResult.Complete;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")] // TODO: fix
        public ScriptCodeParseResult? CodeProperties {
            get { return _parseResult; }
            set { _parseResult = value; }
        }

        public SourceUnit(LanguageContext context, TextContentProvider contentProvider, string path, SourceCodeKind kind) {
            Assert.NotNull(context, contentProvider);
            Debug.Assert(context.CanCreateSourceCode);

            _language = context;
            _contentProvider = contentProvider;
            _kind = kind;
            _path = path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceCodeReader GetReader() {
            return _contentProvider.GetReader();
        }

        /// <summary>
        /// Reads specified range of lines (or less) from the source unit. 
        /// Line numbers starts with 1.
        /// </summary>
        public string[] GetCodeLines(int start, int count) {
            ContractUtils.Requires(start > 0, "start");
            ContractUtils.Requires(count > 0, "count");

            List<string> result = new List<string>(count);

            using (SourceCodeReader reader = GetReader()) {
                reader.SeekLine(start);
                while (count > 0) {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    result.Add(line);
                    count--;
                }
            }

            return result.ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCodeLine(int line) {
            string[] lines = GetCodeLines(line, 1);
            return (lines.Length > 0) ? lines[0] : null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCode() {
            using (SourceCodeReader reader = GetReader()) {
                return reader.ReadToEnd();
            }
        }

        #region Line/File mapping

        public SourceLocation MakeLocation(int index, int line, int column) {
            return new SourceLocation(index, MapLine(line), column);
        }
        public SourceLocation MakeLocation(SourceLocation loc) {
            return new SourceLocation(loc.Index, MapLine(loc.Line), loc.Column);
        }

        public int MapLine(int line) {
            if (_lineMap != null) {
                int match = BinarySearch(_lineMap, line);
                int delta = line - _lineMap[match].Key;
                line = _lineMap[match].Value + delta;
                if (line < 1) {
                    line = 1; // this is the minimum value
                }
            }

            return line;
        }

        private static int BinarySearch<T>(KeyValuePair<int, T>[] array, int line) {
            int match = Array.BinarySearch(array, new KeyValuePair<int, T>(line, default(T)), new KeyComparer<T>());
            if (match < 0) {
                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;

                // If our index = -1, it means that this line is before any line numbers that
                // we know about. If that's the case, use the first entry in the list
                if (match == -1) {
                    match = 0;
                }
            }
            return match;
        }


        private class KeyComparer<T1> : IComparer<KeyValuePair<int, T1>> {
            public int Compare(KeyValuePair<int, T1> x, KeyValuePair<int, T1> y) {
                return x.Key - y.Key;
            }
        }

        #endregion

        #region Parsing, Compilation, Execution

        public bool EmitDebugSymbols {
            get {
                return HasPath && LanguageContext.DomainManager.Configuration.DebugMode;
            }
        }

        public ScriptCode Compile() {
            return Compile(ErrorSink.Default);
        }

        public ScriptCode Compile(ErrorSink errorSink) {
            return Compile(_language.GetCompilerOptions(), errorSink);
        }

        /// <summary>
        /// Errors are reported to the specified sink. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </summary>
        public ScriptCode Compile(CompilerOptions options, ErrorSink errorSink) {
            ContractUtils.RequiresNotNull(errorSink, "errorSink");
            ContractUtils.RequiresNotNull(options, "options");

            return _language.CompileSourceCode(this, options, errorSink);
        }

        /// <summary>
        /// Executes against a specified scope.
        /// </summary>
        public object Execute(Scope scope) {
            return Execute(scope, ErrorSink.Default);
        }

        /// <summary>
        /// Executes against a specified scope and reports errors to the given error sink.
        /// </summary>
        public object Execute(Scope scope, ErrorSink errorSink) {
            ContractUtils.RequiresNotNull(scope, "scope");

            ScriptCode compiledCode = Compile(_language.GetCompilerOptions(scope), errorSink);

            if (compiledCode == null) {
                throw new SyntaxErrorException();
            }

            return compiledCode.Run(scope);
        }

        /// <summary>
        /// Executes in a new scope created by the language.
        /// </summary>
        public object Execute() {
            return Compile().Run();
        }

        /// <summary>
        /// Executes in a new scope created by the language.
        /// </summary>
        public object Execute(ErrorSink errorSink) {
            return Compile(errorSink).Run();
        }

        /// <summary>
        /// Executes in a new scope created by the language.
        /// </summary>
        public object Execute(CompilerOptions options, ErrorSink errorSink) {
            return Compile(options, errorSink).Run();
        }

        public int ExecuteProgram() {
            return _language.ExecuteProgram(this);
        }

        #endregion

        public void SetLineMapping(KeyValuePair<int, int>[] lineMap) {
            _lineMap = (lineMap == null || lineMap.Length == 0) ? null : lineMap;
        }
    }
}
