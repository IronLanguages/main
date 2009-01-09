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
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Interpretation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Provides language specific facilities which are typicalled called by the runtime.
    /// </summary>
    public abstract class LanguageContext {
        private readonly ScriptDomainManager _domainManager;
        private static readonly ModuleGlobalCache _noCache = new ModuleGlobalCache(ModuleGlobalCache.NotCaching);
        private ActionBinder _binder;
        private readonly ContextId _id;

        protected LanguageContext(ScriptDomainManager domainManager) {
            ContractUtils.RequiresNotNull(domainManager, "domainManager");

            _domainManager = domainManager;
            _id = domainManager.GenerateContextId();
        }

        public ActionBinder Binder {
            get {
                return _binder;
            }
            protected set {
                _binder = value;
            }
        }

        /// <summary>
        /// Provides the ContextId which includes members that should only be shown for this LanguageContext.
        /// 
        /// ContextId's are used for filtering by Scope's.
        /// </summary>
        public ContextId ContextId {
            get { return _id; }
        }

        /// <summary>
        /// Gets the ScriptDomainManager that this LanguageContext is running within.
        /// </summary>
        public ScriptDomainManager DomainManager {
            get { return _domainManager; }
        }

        /// <summary>
        /// Whether the language can parse code and create source units.
        /// </summary>
        public virtual bool CanCreateSourceCode {
            get { return true; }
        }

        #region Scope

        public virtual Scope GetScope(string path) {
            return null;
        }

        // TODO: remove
        public ScopeExtension EnsureScopeExtension(Scope scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            ScopeExtension extension = scope.GetExtension(ContextId);

            if (extension == null) {
                extension = CreateScopeExtension(scope);
                if (extension == null) {
                    throw Error.MustReturnScopeExtension();
                }
                return scope.SetExtension(ContextId, extension);
            }

            return extension;
        }

        // TODO: remove
        public virtual ScopeExtension CreateScopeExtension(Scope scope) {
            return new ScopeExtension(scope);
        }

        #endregion

        #region Source Code Parsing & Compilation

        /// <summary>
        /// Provides a text reader for source code that is to be read from a given stream.
        /// </summary>
        /// <param name="stream">The stream open for reading. The stream must also allow seeking.</param>
        /// <param name="defaultEncoding">An encoding that should be used if the stream doesn't have Unicode or language specific preamble.</param>
        /// <returns>The reader.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        public virtual SourceCodeReader GetSourceReader(Stream stream, Encoding defaultEncoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
            ContractUtils.Requires(stream.CanRead && stream.CanSeek, "stream", "The stream must support reading and seeking");

            var result = new StreamReader(stream, defaultEncoding, true);
            result.Peek();
            return new SourceCodeReader(result, result.CurrentEncoding);
        }

        /// <summary>
        /// Creates the language specific CompilerOptions object for compilation of code not bound to any particular scope.
        /// The language should flow any relevant options from LanguageContext to the newly created options instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual CompilerOptions GetCompilerOptions() {
            return new CompilerOptions();
        }

        /// <summary>
        /// Creates the language specific CompilerOptions object for compilation of code bound to a given scope.
        /// </summary>
        public virtual CompilerOptions GetCompilerOptions(Scope scope) {
            return GetCompilerOptions();
        }

        /// <summary>
        /// Parses the source code within a specified compiler context. 
        /// The source unit to parse is held on by the context.
        /// </summary>
        /// <returns><b>null</b> on failure.</returns>
        /// <remarks>Could also set the code properties and line/file mappings on the source unit.</remarks>
        internal protected abstract ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink);

        internal protected virtual ScriptCode LoadCompiledCode(DlrMainCallTarget method, string path) {
            return ScriptCode.Load(method, this, path);
        }

        #endregion


        /// <summary>
        /// Looks up the name in the provided Scope using the current language's semantics.
        /// </summary>
        public virtual bool TryLookupName(Scope scope, SymbolId name, out object value) {
            if (scope.TryLookupName(this, name, out value) && value != Uninitialized.Instance) {
                return true;
            }

            return TryLookupGlobal(scope, name, out value);
        }

        /// <summary>
        /// Looks up the name in the provided scope using the current language's semantics.
        /// 
        /// If the name cannot be found throws the language appropriate exception or returns
        /// the language's appropriate default value.
        /// </summary>
        public virtual object LookupName(Scope scope, SymbolId name) {
            object value;
            if (!TryLookupName(scope, name, out value) || value == Uninitialized.Instance) {
                throw MissingName(name);
            }

            return value;
        }

        /// <summary>
        /// Attempts to set the name in the provided scope using the current language's semantics.
        /// </summary>
        public virtual void SetName(Scope scope, SymbolId name, object value) {
            scope.SetName(name, value);
        }

        /// <summary>
        /// Attempts to remove the name from the provided scope using the current language's semantics.
        /// </summary>
        public virtual bool RemoveName(Scope scope, SymbolId name) {
            return scope.RemoveName(this, name);
        }

        /// <summary>
        /// Attemps to lookup a global variable using the language's semantics called from
        /// the provided Scope.  The default implementation will attempt to lookup the variable
        /// at the host level.
        /// </summary>
        public virtual bool TryLookupGlobal(Scope scope, SymbolId name, out object value) {
            value = null;
            return false;
        }

        /// <summary>
        /// Called when a lookup has failed and an exception should be thrown.  Enables the 
        /// language context to throw the appropriate exception for their language when
        /// name lookup fails.
        /// </summary>
        protected internal virtual Exception MissingName(SymbolId name) {
            return Error.NameNotDefined(SymbolTable.IdToString(name));
        }

        /// <summary>
        /// Returns a ModuleGlobalCache for the given name.  
        /// 
        /// This cache enables fast access to global values when a SymbolId is not defined after searching the Scope's.  Usually
        /// a language implements lookup of the global value via TryLookupGlobal.  When GetModuleCache returns a ModuleGlobalCache
        /// a cached value can be used instead of calling TryLookupGlobal avoiding a possibly more expensive lookup from the 
        /// LanguageContext.  The ModuleGlobalCache can be held onto and have its value updated when the cache is invalidated.
        /// 
        /// By default this returns a cache which indicates no caching should occur and the LanguageContext will be 
        /// consulted when a module value is not available. If a LanguageContext only caches some values it can return 
        /// the value from the base method when the value should not be cached.
        /// </summary>
        protected internal virtual ModuleGlobalCache GetModuleCache(SymbolId name) {
            return _noCache;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public virtual Assembly LoadAssemblyFromFile(string file) {
#if SILVERLIGHT
            return null;
#else
            return Assembly.LoadFile(file);
#endif
        }

        #region ScriptEngine API

        public virtual Version LanguageVersion {
            get {
                return new Version(0, 0);
            }
        }

        public virtual void SetSearchPaths(ICollection<string> paths) {
            throw new NotSupportedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual ICollection<string> GetSearchPaths() {
            return Options.SearchPaths;
        }

#if !SILVERLIGHT
        // Convert a CodeDom to source code, and output the generated code and the line number mappings (if any)
        public virtual SourceUnit GenerateSourceCode(System.CodeDom.CodeObject codeDom, string path, SourceCodeKind kind) {
            throw new NotImplementedException();
        }
#endif

        public virtual TService GetService<TService>(params object[] args) where TService : class {
            return null;
        }

        //TODO these three properties should become abstract and updated for all implementations
        public virtual Guid LanguageGuid {
            get {
                return Guid.Empty;
            }
        }

        public virtual Guid VendorGuid {
            get {
                return Guid.Empty;
            }
        }

        public virtual void Shutdown() {
        }

        public virtual string FormatException(Exception exception) {
            return exception.ToString();
        }

        public virtual Microsoft.Scripting.LanguageOptions Options {
            get {
                return new Microsoft.Scripting.LanguageOptions();
            }
        }

        #region Source Units

        public SourceUnit CreateSnippet(string code, SourceCodeKind kind) {
            return CreateSnippet(code, null, kind);
        }

        public SourceUnit CreateSnippet(string code, string id, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(code, "code");

            return CreateSourceUnit(new SourceStringContentProvider(code), id, kind);
        }

        public SourceUnit CreateFileUnit(string path) {
            return CreateFileUnit(path, StringUtils.DefaultEncoding);
        }

        public SourceUnit CreateFileUnit(string path, Encoding encoding) {
            return CreateFileUnit(path, encoding, SourceCodeKind.File);
        }

        public SourceUnit CreateFileUnit(string path, Encoding encoding, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(encoding, "encoding");

            TextContentProvider provider = new LanguageBoundTextContentProvider(this, new FileStreamContentProvider(DomainManager.Platform, path), encoding);
            return CreateSourceUnit(provider, path, kind);
        }

        public SourceUnit CreateFileUnit(string path, string content) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(content, "content");

            TextContentProvider provider = new SourceStringContentProvider(content);
            return CreateSourceUnit(provider, path, SourceCodeKind.File);
        }

        public SourceUnit CreateSourceUnit(StreamContentProvider contentProvider, string path, Encoding encoding, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(contentProvider, "contentProvider");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            ContractUtils.Requires(path == null || path.Length > 0, "path", Strings.EmptyStringIsInvalidPath);
            ContractUtils.Requires(EnumBounds.IsValid(kind), "kind");
            ContractUtils.Requires(CanCreateSourceCode);

            return new SourceUnit(this, new LanguageBoundTextContentProvider(this, contentProvider, encoding), path, kind);
        }

        public SourceUnit CreateSourceUnit(TextContentProvider contentProvider, string path, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(contentProvider, "contentProvider");
            ContractUtils.Requires(path == null || path.Length > 0, "path", Strings.EmptyStringIsInvalidPath);
            ContractUtils.Requires(EnumBounds.IsValid(kind), "kind");
            ContractUtils.Requires(CanCreateSourceCode);

            return new SourceUnit(this, contentProvider, path, kind);
        }

        #endregion

        #endregion

        private static T GetArg<T>(object[] arg, int index, bool optional) {
            if (!optional && index >= arg.Length) {
                throw Error.InvalidParamNumForService();
            }

            if (!(arg[index] is T)) {
                throw Error.InvalidArgumentType(String.Format("arg[{0}]", index), typeof(T));
            }

            return (T)arg[index];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual ErrorSink GetCompilerErrorSink() {
            return ErrorSink.Null;
        }

        public virtual void GetExceptionMessage(Exception exception, out string message, out string errorTypeName) {
            message = exception.Message;
            errorTypeName = exception.GetType().Name;
        }

        public virtual int ExecuteProgram(SourceUnit program) {
            ContractUtils.RequiresNotNull(program, "program");

            object returnValue = program.Execute();

            CodeContext context = new CodeContext(new Scope(), this);

            CallSite<Func<CallSite, CodeContext, object, object>> site =
                CallSite<Func<CallSite, CodeContext, object, object>>.Create(OldConvertToAction.Make(Binder, typeof(int), ConversionResultKind.ExplicitTry));

            object exitCode = site.Target(site, context, returnValue);
            return (exitCode != null) ? (int)exitCode : 0;
        }

        #region Object Operations Support

        internal static DynamicMetaObject ErrorMetaObject(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            return onBindingError ?? DynamicMetaObject.CreateThrow(target, args, typeof(NotImplementedException), ArrayUtils.EmptyObjects);
        }

        public virtual UnaryOperationBinder CreateUnaryOperationBinder(ExpressionType operation) {
            return new DefaultUnaryOperationBinder(operation);
        }

        private sealed class DefaultUnaryOperationBinder : UnaryOperationBinder {
            internal DefaultUnaryOperationBinder(ExpressionType operation)
                : base(operation) {
            }

            public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                return ErrorMetaObject(target, new[] { target }, errorSuggestion);
            }
            
            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual BinaryOperationBinder CreateBinaryOperationBinder(ExpressionType operation) {
            return new DefaultBinaryOperationBinder(operation);
        }

        private sealed class DefaultBinaryOperationBinder : BinaryOperationBinder {
            internal DefaultBinaryOperationBinder(ExpressionType operation)
                : base(operation) {
            }

            public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
                return ErrorMetaObject(target, new[] { target, arg }, errorSuggestion);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        [Obsolete("Use UnaryOperation or BinaryOperation")]
        private class DefaultOperationAction : OperationBinder {
            internal DefaultOperationAction(string operation)
                : base(operation) {
            }

            public override DynamicMetaObject FallbackOperation(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(target, args, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        [Obsolete("Use UnaryOperation or BinaryOperation")]
        public virtual OperationBinder CreateOperationBinder(string operation) {
            return new DefaultOperationAction(operation);
        }

        private class DefaultConvertAction : ConvertBinder {
            internal DefaultConvertAction(Type type, bool @explicit)
                : base(type, @explicit) {
            }

            public override DynamicMetaObject FallbackConvert(DynamicMetaObject self, DynamicMetaObject onBindingError) {
                if (Type.IsAssignableFrom(self.LimitType)) {
                    return new DynamicMetaObject(
                        self.Expression,
                        BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType)
                    );
                }

                return onBindingError ??
                    DynamicMetaObject.CreateThrow(
                        self,
                        DynamicMetaObject.EmptyMetaObjects,
                        typeof(ArgumentTypeException),
                        String.Format("Expected {0}, got {1}", Type.FullName, self.LimitType.FullName)
                    );
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual ConvertBinder CreateConvertBinder(Type toType, bool explicitCast) {
            return new DefaultConvertAction(toType, explicitCast);
        }

        private class DefaultGetMemberAction : GetMemberBinder {
            internal DefaultGetMemberAction(string name, bool ignoreCase)
                : base(name, ignoreCase) {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(self, DynamicMetaObject.EmptyMetaObjects, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase) {
            return new DefaultGetMemberAction(name, ignoreCase);
        }

        private class DefaultSetMemberAction : SetMemberBinder {
            internal DefaultSetMemberAction(string name, bool ignoreCase)
                : base(name, ignoreCase) {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(self, new DynamicMetaObject[] { value }, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase) {
            return new DefaultSetMemberAction(name, ignoreCase);
        }

        private class DefaultDeleteMemberAction : DeleteMemberBinder {
            internal DefaultDeleteMemberAction(string name, bool ignoreCase)
                : base(name, ignoreCase) {
            }

            public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(self, DynamicMetaObject.EmptyMetaObjects, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual DeleteMemberBinder CreateDeleteMemberBinder(string name, bool ignoreCase) {
            return new DefaultDeleteMemberAction(name, ignoreCase);
        }

        private class DefaultCallAction : InvokeMemberBinder {
            private LanguageContext _context;

            internal DefaultCallAction(LanguageContext context, string name, bool ignoreCase, params ArgumentInfo[] arguments)
                : base(name, ignoreCase, arguments) {
                _context = context;
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(target, args.AddFirst(target), onBindingError);
            }

            private static Expression[] GetArgs(DynamicMetaObject target, DynamicMetaObject[] args) {
                Expression[] res = new Expression[args.Length + 1];
                res[0] = target.Expression;
                for (int i = 0; i < args.Length; i++) {
                    res[1 + i] = args[i].Expression;
                }

                return res;
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                target.Restrictions.Merge(BindingRestrictions.Combine(args));
                return new DynamicMetaObject(
                    Expression.Dynamic(
                        _context.CreateInvokeBinder(Arguments.ToArray()),
                        typeof(object),
                        GetArgs(target, args)
                    ),
                    target.Restrictions
                );
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual InvokeMemberBinder CreateCallBinder(string name, bool ignoreCase, params ArgumentInfo[] arguments) {
            return new DefaultCallAction(this, name, ignoreCase, arguments);
        }

        private class DefaultInvokeAction : InvokeBinder {
            internal DefaultInvokeAction(params ArgumentInfo[] arguments)
                : base(arguments) {
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(target, args, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual InvokeBinder CreateInvokeBinder(params ArgumentInfo[] arguments) {
            return new DefaultInvokeAction(arguments);
        }

        private class DefaultCreateAction : CreateInstanceBinder {
            internal DefaultCreateAction(params ArgumentInfo[] arguments)
                : base(arguments) {
            }

            public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return ErrorMetaObject(target, args, onBindingError);
            }

            public override object CacheIdentity {
                get { return this; }
            }
        }

        public virtual CreateInstanceBinder CreateCreateBinder(params ArgumentInfo[] arguments) {
            return new DefaultCreateAction(arguments);
        }

        #endregion

        /// <summary>
        /// Called by an interpreter when an exception is about to be thrown by an interpreted or
        /// when a CLR method is called that threw an exception.
        /// </summary>
        /// <param name="state">
        /// The current interpreted frame state. The frame is either throwing the exception or 
        /// is the interpreted frame that is calling a CLR method that threw or propagated the exception. 
        /// </param>
        /// <param name="exception">The exception to be (re)thrown.</param>
        /// <param name="isInterpretedThrow">Whether the exception is thrown by an interpreted code.</param>
        /// <remarks>
        /// The method can be called multiple times for a single exception if the interpreted code calls some CLR code that
        /// calls an interpreted code that throws an exception. The method is called at each interpeted/non-interpreted frame boundary
        /// and in the frame that raised the exception.
        /// </remarks>
        internal protected virtual void InterpretExceptionThrow(InterpreterState state, Exception exception, bool isInterpretedThrow) {
            Assert.NotNull(state, exception);
            // nop
        }

        /// <summary>
        /// Gets the member names associated with the object
        /// By default, only returns IDO names
        /// </summary>
        internal protected virtual IList<string> GetMemberNames(object obj) {
            var ido = obj as IDynamicObject;
            if (ido != null) {
                var mo = ido.GetMetaObject(Expression.Parameter(typeof(object), null));
                return mo.GetDynamicMemberNames().ToReadOnly();
            }
            return EmptyArray<string>.Instance;
        }
    }
}
