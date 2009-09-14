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

#if CLR2
using dynamic = System.Object;
#endif

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Dynamic;
using System.Security.Permissions;
using System.Text;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Hosting counterpart for <see cref="SourceUnit"/>.
    /// </summary>
    [DebuggerDisplay("{Path ?? \"<anonymous>\"}")]
    public sealed class ScriptSource
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
    {
        private readonly ScriptEngine _engine;
        private readonly SourceUnit _unit;

        internal SourceUnit SourceUnit {
            get { return _unit; }
        }

        /// <summary>
        /// Identification of the source unit. Assigned by the host. 
        /// The format and semantics is host dependent (could be a path on file system or URL).
        /// <c>null</c> for anonymous script source.
        /// Cannot be an empty string.
        /// </summary>
        public string Path {
            get { return _unit.Path; }
        }

        public SourceCodeKind Kind {
            get { return _unit.Kind; }
        }

        public ScriptEngine Engine {
            get { return _engine; }
        }

        internal ScriptSource(ScriptEngine engine, SourceUnit sourceUnit) {
            Assert.NotNull(engine, sourceUnit);
            _unit = sourceUnit;
            _engine = engine;
        }

        #region Compilation and Execution

        /// <summary>
        /// Compile the ScriptSource into CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public CompiledCode Compile() {
            return CompileInternal(null, null);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to errors.
        /// </remarks>
        public CompiledCode Compile(ErrorListener errorListener) {
            ContractUtils.RequiresNotNull(errorListener, "errorListener");

            return CompileInternal(null, errorListener);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </remarks>
        public CompiledCode Compile(CompilerOptions compilerOptions) {
            ContractUtils.RequiresNotNull(compilerOptions, "compilerOptions");

            return CompileInternal(compilerOptions, null);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </remarks>
        public CompiledCode Compile(CompilerOptions compilerOptions, ErrorListener errorListener) {
            ContractUtils.RequiresNotNull(errorListener, "errorListener");
            ContractUtils.RequiresNotNull(compilerOptions, "compilerOptions");

            return CompileInternal(compilerOptions, errorListener);
        }

        private CompiledCode CompileInternal(CompilerOptions compilerOptions, ErrorListener errorListener) {
            ErrorSink errorSink = new ErrorListenerProxySink(this, errorListener);
            ScriptCode code = compilerOptions != null ? _unit.Compile(compilerOptions, errorSink) : _unit.Compile(errorSink);

            return (code != null) ? new CompiledCode(_engine, code) : null;
        }

        /// <summary>
        /// Executes the code in the specified scope.
        /// Returns an object that is the resulting value of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public dynamic Execute(ScriptScope scope) {
            ContractUtils.RequiresNotNull(scope, "scope");

            return _unit.Execute(scope.Scope);
        }

        /// <summary>
        /// Executes the source code. The execution is not bound to any particular scope.
        /// </summary>
        public dynamic Execute() {
            // The host doesn't need the scope so do not create it here. 
            // The language can treat the code as not bound to a DLR scope and change global lookup semantics accordingly.
            return _unit.Execute();
        }

        /// <summary>
        /// Executes the code in a specified scope and converts the result to the specified type.
        /// The conversion is language specific.
        /// </summary>
        public T Execute<T>(ScriptScope scope) {
            return _engine.Operations.ConvertTo<T>((object)Execute(scope));
        }

        /// <summary>
        /// Executes the code in an empty scope and converts the result to the specified type.
        /// The conversion is language specific.
        /// </summary>
        public T Execute<T>() {
            return _engine.Operations.ConvertTo<T>((object)Execute());
        }

#if !SILVERLIGHT
        /// <summary>
        /// Executes the code in the specified scope and return a result.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// </summary>
        public ObjectHandle ExecuteAndWrap(ScriptScope scope) {
            return new ObjectHandle((object)Execute(scope));
        }

        /// <summary>
        /// Executes the code in an empty scope.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// </summary>
        public ObjectHandle ExecuteAndWrap() {
            return new ObjectHandle((object)Execute());
        }
#endif

        /// <summary>
        /// Runs a specified code as if it was a program launched from OS command shell. 
        /// and returns a process exit code indicating the success or error condition 
        /// of executing the code.
        /// 
        /// Exact behavior depends on the language. Some languages have a dedicated "exit" exception that 
        /// carries the exit code, in which case the exception is cought and the exit code is returned.
        /// The default behavior returns the result of program's execution converted to an integer 
        /// using a language specific conversion.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public int ExecuteProgram() {
            return _unit.LanguageContext.ExecuteProgram(_unit);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ScriptCodeParseResult GetCodeProperties() {
            return _unit.GetCodeProperties();
        }

        public ScriptCodeParseResult GetCodeProperties(CompilerOptions options) {
            return _unit.GetCodeProperties(options);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceCodeReader GetReader() {
            return _unit.GetReader();
        }

        /// <summary>
        /// Detects the encoding of the content.
        /// </summary>
        /// <returns>
        /// An encoding that is used by the reader of the script source to transcode its content to Unicode text.
        /// <c>Null</c> if the content is already textual and no transcoding is performed.
        /// </returns>
        /// <remarks>
        /// Note that the default encoding specified when the script source is created could be overridden by 
        /// an encoding that is found in the content preamble (Unicode BOM or a language specific encoding preamble).
        /// In that case the preamble encoding is returned. Otherwise, the default encoding is returned.
        /// </remarks>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        public Encoding DetectEncoding() {
            using (var reader = _unit.GetReader()) {
                return reader.Encoding;
            }
        }

        /// <summary>
        /// Reads specified range of lines (or less) from the source unit. 
        /// </summary>
        /// <param name="start">1-based number of the first line to fetch.</param>
        /// <param name="count">The number of lines to fetch.</param>
        /// <remarks>
        /// Which character sequences are considered line separators is language specific.
        /// If language doesn't specify otherwise "\r", "\n", "\r\n" are recognized line separators.
        /// </remarks>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        public string[] GetCodeLines(int start, int count) {
            return _unit.GetCodeLines(start, count);
        }

        /// <summary>
        /// Reads a specified line.
        /// </summary>
        /// <param name="line">1-based line number.</param>
        /// <returns>Line content. Line separator is not included.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <remarks>
        /// Which character sequences are considered line separators is language specific.
        /// If language doesn't specify otherwise "\r", "\n", "\r\n" are recognized line separators.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCodeLine(int line) {
            return _unit.GetCodeLine(line);
        }

        /// <summary>
        /// Gets script source content.
        /// </summary>
        /// <returns>Entire content.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <remarks>
        /// The result includes language specific preambles (e.g. "#coding:UTF-8" encoding preamble recognized by Ruby), 
        /// but not the preamble defined by the content encoding (e.g. BOM).
        /// The entire content of the source unit is encoded by single encoding (if it is read from binary stream).
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCode() {
            return _unit.GetCode();
        }

        // TODO: can this be removed? no one uses it
        #region line number mapping

        public int MapLine(int line) {
            return _unit.MapLine(line);
        }

        public SourceSpan MapLine(SourceSpan span) {
            return new SourceSpan(_unit.MakeLocation(span.Start), _unit.MakeLocation(span.End));
        }

        public SourceLocation MapLine(SourceLocation location) {
            return _unit.MakeLocation(location);
        }

        // TODO: remove this? we don't support file mapping
        // (but it's still in the hosting spec)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "line")]
        public string MapLinetoFile(int line) {
            return _unit.Path;
        }

        #endregion

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
