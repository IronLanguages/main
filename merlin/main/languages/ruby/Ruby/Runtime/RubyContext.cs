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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Security;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpretation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// An isolation context: could be per thread/request or shared accross certain threads.
    /// </summary>
    public sealed class RubyContext : LanguageContext {
        internal static readonly Guid RubyLanguageGuid = new Guid("F03C4640-DABA-473f-96F1-391400714DAB");
        private static int _RuntimeIdGenerator = 0;

        // MRI compliance:
        public static readonly string/*!*/ MriVersion = "1.8.6";
        public static readonly string/*!*/ MriReleaseDate = "2008-05-28";

        // IronRuby:
        public const string/*!*/ IronRubyVersionString = "1.0.0.0";
        public static readonly Version IronRubyVersion = new Version(1, 0, 0, 0);
        internal const string/*!*/ IronRubyDisplayName = "IronRuby 1.0 Alpha";
        internal const string/*!*/ IronRubyNames = "IronRuby;Ruby;rb";
        internal const string/*!*/ IronRubyFileExtensions = ".rb";

        // TODO: remove
        internal static RubyContext _Default;

        private readonly int _runtimeId;
        private readonly RubyScope/*!*/ _emptyScope;

        private RubyOptions/*!*/ _options;
        private Dictionary<object, object> _libraryData;

        private readonly Stopwatch _upTime;

        /// <summary>
        /// $!
        /// </summary>
        [ThreadStatic]
        private static Exception _currentException;

        /// <summary>
        /// $? of type Process::Status
        /// </summary>
        [ThreadStatic]
        private static object _childProcessExitStatus;

        /// <summary>
        /// $SAFE
        /// </summary>
        [ThreadStatic]
        private static int _currentSafeLevel;

        /// <summary>
        /// $KCODE
        /// </summary>
        private static KCode _kcode;

        /// <summary>
        /// $/, $-O
        /// </summary>
        private MutableString _inputSeparator;

        /// <summary>
        /// $\
        /// </summary>
        private MutableString _outputSeparator;

        /// <summary>
        /// $;, $-F
        /// </summary>
        private object _stringSeparator;

        /// <summary>
        /// $,
        /// </summary>
        private MutableString _itemSeparator;

        private readonly RuntimeErrorSink/*!*/ _runtimeErrorSink;
        private readonly RubyInputProvider/*!*/ _inputProvider;
        private readonly Dictionary<string/*!*/, GlobalVariable>/*!*/ _globalVariables;
        private Proc _traceListener;

        [ThreadStatic]
        private bool _traceListenerSuspended;

        private EqualityComparer _equalityComparer;

        /// <summary>
        /// Maps CLR types to Ruby classes/modules.
        /// Doesn't contain classes defined in Ruby.
        /// </summary>
        private readonly Dictionary<Type, RubyModule>/*!*/ _moduleCache;
        private object ModuleCacheSyncRoot { get { return _moduleCache; } }

        /// <summary>
        /// Maps CLR namespace trackers to Ruby modules.
        /// </summary>
        private readonly Dictionary<NamespaceTracker, RubyModule>/*!*/ _namespaceCache;
        private object NamespaceCacheSyncRoot { get { return _namespaceCache; } }

        private readonly Loader/*!*/ _loader;
        private Scope/*!*/ _globalScope;
        private readonly List<RubyIO>/*!*/ _fileDescriptors = new List<RubyIO>(10);

        // classes used by runtime (we need to update initialization generator if any of these are added):
        private RubyModule/*!*/ _kernelModule;
        private RubyClass/*!*/ _objectClass;
        private RubyClass/*!*/ _classClass;
        private RubyClass/*!*/ _moduleClass;
        private RubyClass/*!*/ _nilClass;
        private RubyClass/*!*/ _trueClass;
        private RubyClass/*!*/ _falseClass;
        private RubyClass/*!*/ _exceptionClass;
        private RubyClass _standardErrorClass;

        private Action<RubyModule>/*!*/ _classSingletonTrait;
        private Action<RubyModule>/*!*/ _singletonSingletonTrait;
        private Action<RubyModule>/*!*/ _mainSingletonTrait;

        // internally set by Initializer:
        public RubyModule/*!*/ KernelModule { get { return _kernelModule; } }
        public RubyClass/*!*/ ObjectClass { get { return _objectClass; } }
        public RubyClass/*!*/ ClassClass { get { return _classClass; } set { _classClass = value; } }
        public RubyClass/*!*/ ModuleClass { get { return _moduleClass; } set { _moduleClass = value; } }
        public RubyClass/*!*/ NilClass { get { return _nilClass; } set { _nilClass = value; } }
        public RubyClass/*!*/ TrueClass { get { return _trueClass; } set { _trueClass = value; } }
        public RubyClass/*!*/ FalseClass { get { return _falseClass; } set { _falseClass = value; } }
        public RubyClass ExceptionClass { get { return _exceptionClass; } set { _exceptionClass = value; } }
        public RubyClass StandardErrorClass { get { return _standardErrorClass; } set { _standardErrorClass = value; } }

        internal Action<RubyModule>/*!*/ ClassSingletonTrait { get { return _classSingletonTrait; } }

        // Maps objects to InstanceData. The keys store weak references to the objects.
        // Objects are compared by reference (identity). 
        // An entry can be removed as soon as the key object becomes unreachable.
        private readonly InstanceDataWeakTable/*!*/ _referenceTypeInstanceData;

        // Maps values to InstanceData. The keys store value representatives. 
        // All objects that has the same value (value-equality) map to the same InstanceData.
        // Entries cannot be ever freed since anytime in future one may create a new object whose value has already been mapped to InstanceData.
        private readonly Dictionary<object, RubyInstanceData>/*!*/ _valueTypeInstanceData;
        private object/*!*/ ValueTypeInstanceDataSyncRoot { get { return _valueTypeInstanceData; } }

        private RubyInstanceData/*!*/ _nilInstanceData = new RubyInstanceData(RubyUtils.NilObjectId);

        #region Dynamic Sites

        private CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> _methodAddedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> _methodRemovedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> _methodUndefinedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> _singletonMethodAddedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> _singletonMethodRemovedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> _singletonMethodUndefinedCallbackSite;
        private CallSite<Func<CallSite, RubyContext, RubyClass, RubyClass, object>> _classInheritedCallbackSite;

        private void MethodEvent(ref CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> site, string/*!*/ eventName,
            RubyContext/*!*/ context, RubyModule/*!*/ module, string/*!*/ methodName) {

            if (site == null) {
                Interlocked.CompareExchange(
                    ref site,
                    CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>>.Create(RubyCallAction.Make(eventName, RubyCallSignature.WithImplicitSelf(1))),
                    null
                );
            }

            site.Target(site, context, module, SymbolTable.StringToId(methodName));
        }

        private void SingletonMethodEvent(ref CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> site, string/*!*/ eventName,
            RubyContext/*!*/ context, object obj, string/*!*/ methodName) {

            if (site == null) {
                Interlocked.CompareExchange(
                    ref site,
                    CallSite<Func<CallSite, RubyContext, object, SymbolId, object>>.Create(RubyCallAction.Make(eventName, RubyCallSignature.WithImplicitSelf(1))),
                    null
                );
            }

            site.Target(site, context, obj, SymbolTable.StringToId(methodName));
        }

        private void ClassInheritedEvent(RubyClass/*!*/ superClass, RubyClass/*!*/ subClass) {
            if (_classInheritedCallbackSite == null) {
                Interlocked.CompareExchange(
                    ref _classInheritedCallbackSite,
                    CallSite<Func<CallSite, RubyContext, RubyClass, RubyClass, object>>.Create(RubyCallAction.Make(Symbols.Inherited, RubyCallSignature.WithImplicitSelf(1)
                    )),
                    null
                );
            }

            _classInheritedCallbackSite.Target(_classInheritedCallbackSite, this, superClass, subClass);
        }

        #endregion

        public override LanguageOptions Options {
            get { return _options; }
        }

        public RubyOptions RubyOptions {
            get { return _options; }
        }

        internal RubyScope/*!*/ EmptyScope {
            get { return _emptyScope; }
        }

        // TODO:
        internal Scope/*!*/ DefaultGlobalScope {
            get { return DomainManager.Globals; }
        }

        public Exception CurrentException {
            get { return _currentException; }
            set { _currentException = value; }
        }

        public int CurrentSafeLevel {
            get { return _currentSafeLevel; }
        }

        public KCode KCode {
            get { return _kcode; }
        }

        public MutableString InputSeparator {
            get { return _inputSeparator; }
            set { _inputSeparator = value; }
        }

        public MutableString OutputSeparator {
            get { return _outputSeparator; }
            set { _outputSeparator = value; }
        }

        public object StringSeparator {
            get { return _stringSeparator; }
            set { _stringSeparator = value; }
        }

        public MutableString ItemSeparator {
            get { return _itemSeparator; }
            set { _itemSeparator = value; }
        }

        public Proc TraceListener {
            get { return _traceListener; }
            set { _traceListener = value; }
        }

        public IEnumerable<KeyValuePair<string, GlobalVariable>>/*!*/ GlobalVariables {
            get { return _globalVariables; }
        }

        public object/*!*/ GlobalVariablesSyncRoot {
            get { return _globalVariables; }
        }

        public RubyInputProvider/*!*/ InputProvider {
            get { return _inputProvider; }
        }

        public RuntimeErrorSink/*!*/ RuntimeErrorSink {
            get { return _runtimeErrorSink; }
        }

        public object ChildProcessExitStatus {
            get { return _childProcessExitStatus; }
            set { _childProcessExitStatus = value; }
        }

        public Scope/*!*/ TopGlobalScope {
            get { return _globalScope; }
        }

        public Loader/*!*/ Loader {
            get { return _loader; }
        }

        public bool ShowCls {
            get { return false; }
        }

        public EqualityComparer EqualityComparer {
            get {
                if (_equalityComparer == null) {
                    Interlocked.CompareExchange(ref _equalityComparer, new EqualityComparer(this), null);
                }
                return _equalityComparer;
            }
        }

        public const int StandardInputDescriptor = 0;
        public const int StandardOutputDescriptor = 1;
        public const int StandardErrorOutputDescriptor = 2;

        public object StandardInput { get; set; }
        public object StandardOutput { get; set; }
        public object StandardErrorOutput { get; set; }

        public object Verbose { get; set; }

        public override Version LanguageVersion {
            get { return IronRubyVersion; }
        }

        public override Guid LanguageGuid {
            get { return RubyLanguageGuid; }
        }

        public int RuntimeId {
            get { return _runtimeId; }
        }

        #region Initialization

        public RubyContext(ScriptDomainManager/*!*/ manager, IDictionary<string, object> options)
            : base(manager) {
            ContractUtils.RequiresNotNull(manager, "manager");
            _options = new RubyOptions(options);

            _runtimeId = Interlocked.Increment(ref _RuntimeIdGenerator);
            _upTime = new Stopwatch();
            _upTime.Start();

            Binder = new RubyBinder(manager);

            _runtimeErrorSink = new RuntimeErrorSink(this);
            _globalVariables = new Dictionary<string, GlobalVariable>();
            _moduleCache = new Dictionary<Type, RubyModule>();
            _namespaceCache = new Dictionary<NamespaceTracker, RubyModule>();
            _referenceTypeInstanceData = new InstanceDataWeakTable();
            _valueTypeInstanceData = new Dictionary<object, RubyInstanceData>();
            _inputProvider = new RubyInputProvider(this, _options.Arguments);
            _globalScope = DomainManager.Globals;
            _loader = new Loader(this);
            _emptyScope = new RubyTopLevelScope(this);

            _currentException = null;
            _currentSafeLevel = 0;
            _childProcessExitStatus = null;
            _inputSeparator = MutableString.Create("\n");
            _outputSeparator = null;
            _stringSeparator = null;
            _itemSeparator = null;
            _kcode = KCode.Default;
            
            if (_options.Verbosity <= 0) {
                Verbose = null;
            } else if (_options.Verbosity == 1) {
                Verbose = ScriptingRuntimeHelpers.False; 
            } else {
                Verbose = ScriptingRuntimeHelpers.True;
            }

            // TODO:
            Interlocked.CompareExchange(ref _Default, this, null);

            _loader.LoadBuiltins();
            Debug.Assert(_exceptionClass != null && _standardErrorClass != null && _nilClass != null);

            Debug.Assert(_classClass != null && _moduleClass != null);

            // needs to run before globals and constants are initialized:
            InitializeFileDescriptors(DomainManager.SharedIO);

            InitializeGlobalConstants();
            InitializeGlobalVariables();
        }

        /// <summary>
        /// Clears thread static variables.
        /// </summary>
        internal static void ClearThreadStatics() {
            _currentException = null;
        }

        private void InitializeGlobalVariables() {
            // special variables:
            Runtime.GlobalVariables.DefineVariablesNoLock(this);

            // TODO:
            // $-a
            // $F
            // $-i
            // $-l
            // $-p

            // $?


            // $0
            if (_options.MainFile != null) {
                DefineGlobalVariableNoLock(Symbols.CommandLineProgramPath, new GlobalVariableInfo(MutableString.Create(_options.MainFile)));
            }

            DefineGlobalVariableNoLock("stdin", Runtime.GlobalVariables.InputStream);
            DefineGlobalVariableNoLock("stdout", Runtime.GlobalVariables.OutputStream);
            DefineGlobalVariableNoLock("defout", Runtime.GlobalVariables.OutputStream);
            DefineGlobalVariableNoLock("stderr", Runtime.GlobalVariables.ErrorOutputStream);

            DefineGlobalVariableNoLock("LOADED_FEATURES", Runtime.GlobalVariables.LoadedFiles);
            DefineGlobalVariableNoLock("LOAD_PATH", Runtime.GlobalVariables.LoadPath);
            DefineGlobalVariableNoLock("-I", Runtime.GlobalVariables.LoadPath);
            DefineGlobalVariableNoLock("-O", Runtime.GlobalVariables.InputSeparator);
            DefineGlobalVariableNoLock("-F", Runtime.GlobalVariables.StringSeparator);
            DefineGlobalVariableNoLock("FILENAME", Runtime.GlobalVariables.InputFileName);

            // TODO:
            GlobalVariableInfo debug = new GlobalVariableInfo(DomainManager.Configuration.DebugMode);

            DefineGlobalVariableNoLock("VERBOSE", Runtime.GlobalVariables.Verbose);
            DefineGlobalVariableNoLock("-v", Runtime.GlobalVariables.Verbose);
            DefineGlobalVariableNoLock("-w", Runtime.GlobalVariables.Verbose);
            DefineGlobalVariableNoLock("DEBUG", debug);
            DefineGlobalVariableNoLock("-d", debug);

#if !SILVERLIGHT
            DefineGlobalVariableNoLock("KCODE", Runtime.GlobalVariables.KCode);
            DefineGlobalVariableNoLock("-K", Runtime.GlobalVariables.KCode);
            DefineGlobalVariableNoLock("SAFE", Runtime.GlobalVariables.SafeLevel);

            try {
                TrySetCurrentProcessVariables();
            } catch (SecurityException) {
                // nop
            }
#endif
        }

#if !SILVERLIGHT
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void TrySetCurrentProcessVariables() {
            Process process = Process.GetCurrentProcess();
            DefineGlobalVariableNoLock(Symbols.CurrentProcessId, new ReadOnlyGlobalVariableInfo(process.Id));
        }
#endif

        private void InitializeGlobalConstants() {
            Debug.Assert(_objectClass != null);

            MutableString version = MutableString.Create(RubyContext.MriVersion);
            MutableString platform = MutableString.Create("i386-mswin32");   // TODO: make this the correct string for MAC OS X in Silverlight
            MutableString releaseDate = MutableString.Create(RubyContext.MriReleaseDate);
            MutableString rubyEngine = MutableString.Create("ironruby");

            _objectClass.SetConstant("RUBY_ENGINE", rubyEngine);
            _objectClass.SetConstant("RUBY_VERSION", version);
            _objectClass.SetConstant("RUBY_PATCHLEVEL", 0);
            _objectClass.SetConstant("RUBY_PLATFORM", platform);
            _objectClass.SetConstant("RUBY_RELEASE_DATE", releaseDate);

            _objectClass.SetConstant("VERSION", version);
            _objectClass.SetConstant("PLATFORM", platform);
            _objectClass.SetConstant("RELEASE_DATE", releaseDate);

            _objectClass.SetConstant("IRONRUBY_VERSION", MutableString.Create(RubyContext.IronRubyVersionString));

            _objectClass.SetConstant("STDIN", StandardInput);
            _objectClass.SetConstant("STDOUT", StandardOutput);
            _objectClass.SetConstant("STDERR", StandardErrorOutput);

            object ARGF;
            if (_objectClass.TryGetConstantNoAutoload("ARGF", out ARGF)) {
                _inputProvider.Singleton = ARGF;
            }

            _objectClass.SetConstant("ARGV", _inputProvider.CommandLineArguments);

            // File object
            //_objectClass.SetConstant("DATA", null);

            // Binding 
            // TOPLEVEL_BINDING

            // Hash
            // SCRIPT_LINES__
        }

        private void InitializeFileDescriptors(SharedIO/*!*/ io) {
            Debug.Assert(_fileDescriptors.Count == 0);
            StandardInput = new RubyIO(this, new ConsoleStream(io, ConsoleStreamType.Input), "r");
            StandardOutput = new RubyIO(this, new ConsoleStream(io, ConsoleStreamType.Output), "a");
            StandardErrorOutput = new RubyIO(this, new ConsoleStream(io, ConsoleStreamType.ErrorOutput), "a");
        }

        // TODO: internal
        public void RegisterPrimitives(
            Action<RubyModule>/*!*/ classSingletonTrait,
            Action<RubyModule>/*!*/ singletonSingletonTrait,
            Action<RubyModule>/*!*/ mainSingletonTrait,

            Action<RubyModule>/*!*/ kernelInstanceTrait,
            Action<RubyModule>/*!*/ objectInstanceTrait,
            Action<RubyModule>/*!*/ moduleInstanceTrait,
            Action<RubyModule>/*!*/ classInstanceTrait,

            Action<RubyModule>/*!*/ kernelClassTrait,
            Action<RubyModule>/*!*/ objectClassTrait,
            Action<RubyModule>/*!*/ moduleClassTrait,
            Action<RubyModule>/*!*/ classClassTrait) {

            Assert.NotNull(classSingletonTrait, singletonSingletonTrait, mainSingletonTrait);
            Assert.NotNull(objectInstanceTrait, kernelInstanceTrait, moduleInstanceTrait, classInstanceTrait);
            Assert.NotNull(objectClassTrait, kernelClassTrait, moduleClassTrait, classClassTrait);

            _classSingletonTrait = classSingletonTrait;
            _singletonSingletonTrait = singletonSingletonTrait;
            _mainSingletonTrait = mainSingletonTrait;

            // inheritance hierarchy:
            //
            //           Class
            //             ^
            // Object -> Object'  
            //   ^         ^
            // Module -> Module'
            //   ^         ^
            // Class  -> Class'
            //   ^
            // Object'
            //

            // only Object should expose CLR methods:
            TypeTracker objectTracker = ReflectionCache.GetTypeTracker(typeof(object));

            // we need to create object class before any other since constants are added in its dictionary:
            _kernelModule = new RubyModule(this, Symbols.Kernel, kernelInstanceTrait, null, null);
            _objectClass = new RubyClass(this, Symbols.Object, objectTracker.Type, null, objectInstanceTrait, null, objectTracker, false, false);
            _moduleClass = new RubyClass(this, Symbols.Module, typeof(RubyModule), null, moduleInstanceTrait, _objectClass, null, false, false);
            _classClass = new RubyClass(this, Symbols.Class, typeof(RubyClass), null, classInstanceTrait, _moduleClass, null, false, false);

            CreateDummySingletonClassFor(_kernelModule, _moduleClass, kernelClassTrait);
            CreateDummySingletonClassFor(_objectClass, _classClass, objectClassTrait);
            CreateDummySingletonClassFor(_moduleClass, _objectClass.SingletonClass, moduleClassTrait);
            CreateDummySingletonClassFor(_classClass, _moduleClass.SingletonClass, classClassTrait);

            _objectClass.SetMixins(new RubyModule[] { _kernelModule });

            AddModuleToCacheNoLock(typeof(Kernel), _kernelModule);
            AddModuleToCacheNoLock(objectTracker.Type, _objectClass);
            AddModuleToCacheNoLock(_moduleClass.GetUnderlyingSystemType(), _moduleClass);
            AddModuleToCacheNoLock(_classClass.GetUnderlyingSystemType(), _classClass);

            _objectClass.SetConstant(_moduleClass.Name, _moduleClass);
            _objectClass.SetConstant(_classClass.Name, _classClass);
            _objectClass.SetConstant(_objectClass.Name, _objectClass);
            _objectClass.SetConstant(_kernelModule.Name, _kernelModule);

            _moduleClass.Factories = new Delegate[] {
                new Func<RubyScope, BlockParam, RubyClass, object>(RubyModule.CreateAnonymousModule),
            };

            _classClass.Factories = new Delegate[] {
                new Func<RubyScope, BlockParam, RubyClass, RubyClass, object>(RubyClass.CreateAnonymousClass),
            };
        }

        #endregion

        #region CLR Type and Namespaces caching

        internal void AddModuleToCacheNoLock(Type/*!*/ type, RubyModule/*!*/ module) {
            Assert.NotNull(type, module);
            _moduleCache.Add(type, module);
        }

        internal void AddNamespaceToCacheNoLock(NamespaceTracker/*!*/ namespaceTracker, RubyModule/*!*/ module) {
            Assert.NotNull(namespaceTracker, module);

            _namespaceCache.Add(namespaceTracker, module);
        }

        internal RubyModule/*!*/ GetOrCreateModule(NamespaceTracker/*!*/ tracker) {
            Assert.NotNull(tracker);

            lock (ModuleCacheSyncRoot) {
                return GetOrCreateModuleNoLock(tracker);
            }
        }

        internal bool TryGetModule(NamespaceTracker/*!*/ namespaceTracker, out RubyModule result) {
            lock (NamespaceCacheSyncRoot) {
                return _namespaceCache.TryGetValue(namespaceTracker, out result);
            }
        }

        internal RubyModule/*!*/ GetOrCreateModule(Type/*!*/ interfaceType) {
            Debug.Assert(interfaceType != null && interfaceType.IsInterface);

            lock (ModuleCacheSyncRoot) {
                return GetOrCreateModuleNoLock(interfaceType);
            }
        }

        internal bool TryGetModuleNoLock(Type/*!*/ type, out RubyModule result) {
            return _moduleCache.TryGetValue(type, out result);
        }

        internal bool TryGetClassNoLock(Type/*!*/ type, out RubyClass result) {
            RubyModule module;
            if (_moduleCache.TryGetValue(type, out module)) {
                result = module as RubyClass;
                if (result == null) {
                    throw new InvalidOperationException("Specified type doesn't represent a class");
                }
                return true;
            } else {
                result = null;
                return false;
            }
        }

        internal RubyClass/*!*/ GetOrCreateClass(Type/*!*/ type) {
            Debug.Assert(type != null && !type.IsInterface);

            lock (ModuleCacheSyncRoot) {
                return GetOrCreateClassNoLock(type);
            }
        }

        private RubyModule/*!*/ GetOrCreateModuleNoLock(NamespaceTracker/*!*/ tracker) {
            Assert.NotNull(tracker);

            RubyModule result;
            if (_namespaceCache.TryGetValue(tracker, out result)) {
                return result;
            }

            result = CreateModule(RubyUtils.GetQualifiedName(tracker), null, null, tracker, null);
            _namespaceCache[tracker] = result;
            return result;
        }

        private RubyModule/*!*/ GetOrCreateModuleNoLock(Type/*!*/ interfaceType) {
            Debug.Assert(interfaceType != null && interfaceType.IsInterface);

            RubyModule result;
            if (_moduleCache.TryGetValue(interfaceType, out result)) {
                return result;
            }

            TypeTracker tracker = (TypeTracker)TypeTracker.FromMemberInfo(interfaceType);
            result = CreateModule(RubyUtils.GetQualifiedName(interfaceType), null, null, null, tracker);
            _moduleCache[interfaceType] = result;
            return result;
        }

        private RubyClass/*!*/ GetOrCreateClassNoLock(Type/*!*/ type) {
            Debug.Assert(type != null && !type.IsInterface);

            RubyClass result;
            if (TryGetClassNoLock(type, out result)) {
                return result;
            }

            RubyClass baseClass = GetOrCreateClassNoLock(type.BaseType);
            TypeTracker tracker = (TypeTracker)TypeTracker.FromMemberInfo(type);

            result = CreateClass(RubyUtils.GetQualifiedName(type), type, null, null, null, baseClass, tracker, false, false);

            List<RubyModule> interfaceMixins = GetDeclaredInterfaceModules(type);
            if (interfaceMixins != null) {
                result.SetMixins(interfaceMixins);
            }

            _moduleCache[type] = result;
            return result;
        }

        private List<RubyModule> GetDeclaredInterfaceModules(Type/*!*/ type) {
            // TODO:
            if (type.IsGenericTypeDefinition) {
                return null;
            }

            List<RubyModule> interfaces = new List<RubyModule>();
            foreach (Type iface in ReflectionUtils.GetDeclaredInterfaces(type)) {
                interfaces.Add(GetOrCreateModuleNoLock(iface));
            }

            return interfaces.Count > 0 ? interfaces : null;
        }

        #endregion

        #region Class and Module Factories

        internal RubyClass/*!*/ CreateClass(string name, Type type, object classSingletonOf, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait,
            RubyClass/*!*/ superClass, TypeTracker tracker, bool isRubyClass, bool isSingletonClass) {
            Debug.Assert(superClass != null);

            RubyClass result = new RubyClass(this, name, type, classSingletonOf, instanceTrait, superClass, tracker, isRubyClass, isSingletonClass);
            CreateDummySingletonClassFor(result, superClass.SingletonClass, classTrait);
            return result;
        }

        internal RubyModule/*!*/ CreateModule(string name, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait,
            NamespaceTracker namespaceTracker, TypeTracker typeTracker) {
            RubyModule result = new RubyModule(this, name, instanceTrait, namespaceTracker, typeTracker);
            CreateDummySingletonClassFor(result, _moduleClass, classTrait);
            return result;
        }

        internal RubyClass/*!*/ CreateDummySingletonClassFor(RubyModule/*!*/ module, RubyClass/*!*/ superClass, Action<RubyModule>/*!*/ trait) {
            // Note that in MRI, member tables of dummy singleton are shared with the class the dummy is singleton for
            // This is obviously an implementation detail leaking to the language and we don't support that for code clarity.

            // real class object and it's singleton share the tracker:
            TypeTracker tracker = (module.IsSingletonClass) ? null : module.Tracker;

            RubyClass result = new RubyClass(this, null, null, module, trait, superClass, tracker, false, true);
            result.SingletonClass = result;
            module.SingletonClass = result;
#if DEBUG
            result.DebugName = "S(" + module.DebugName + ")";
#endif
            return result;
        }

        internal RubyClass/*!*/ AppendDummySingleton(RubyClass/*!*/ singleton) {
            Debug.Assert(singleton.IsDummySingletonClass);
            RubyClass super = ((RubyModule)singleton.SingletonClassOf).IsClass ? _classClass.SingletonClass : _moduleClass.SingletonClass;
            return CreateDummySingletonClassFor(singleton, super, _singletonSingletonTrait);
        }

        /// <summary>
        /// Creates a singleton class for specified object unless it already exists. 
        /// </summary>
        public RubyClass/*!*/ CreateSingletonClass(object obj) {
            // TODO: maybe more general interface like IRubyObject:
            RubyModule module = obj as RubyModule;
            if (module != null) {
                return module.CreateSingletonClass();
            }

            return CreateInstanceSingleton(obj, null, null);
        }

        internal RubyClass/*!*/ CreateMainSingleton(object obj) {
            return CreateInstanceSingleton(obj, _mainSingletonTrait, null);
        }

        internal RubyClass/*!*/ CreateInstanceSingleton(object obj, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait)
            //^ ensures result.IsSingletonClass && !result.IsDummySingletonClass;
        {
            Debug.Assert(!(obj is RubyModule));
            Debug.Assert(RubyUtils.CanCreateSingleton(obj));

            if (obj == null) {
                return _nilClass;
            }

            if (obj is bool) {
                return (bool)obj ? _trueClass : _falseClass;
            }

            RubyInstanceData data;
            RubyClass result = TryGetInstanceSingletonOf(obj, out data);
            if (result != null) {
                Debug.Assert(!result.IsDummySingletonClass);
                return result;
            }

            RubyClass c = GetClassOf(obj, data);

            result = CreateClass(null, null, obj, instanceTrait, classTrait ?? _classSingletonTrait, c, null, true, true);
            c.Updated("CreateInstanceSingleton");
            SetInstanceSingletonOf(obj, ref data, result);
#if DEBUG
            result.DebugName = (instanceTrait != null) ? instanceTrait.Method.DeclaringType.Name : "S(" + data.ObjectId + ")";
            result.SingletonClass.DebugName = "S(" + result.DebugName + ")";
#endif
            return result;
        }

        public RubyModule/*!*/ DefineModule(RubyModule/*!*/ owner, string name) {
            ContractUtils.RequiresNotNull(owner, "owner");

            RubyModule result = CreateModule(owner.MakeNestedModuleName(name), null, null, null, null);
            if (name != null) {
                owner.SetConstant(name, result);
            }
            return result;
        }

        // triggers "inherited" event:
        internal RubyClass/*!*/ DefineClass(RubyModule/*!*/ owner, string name, RubyClass/*!*/ superClass) {
            ContractUtils.RequiresNotNull(owner, "owner");
            ContractUtils.RequiresNotNull(superClass, "superClass");

            if (superClass.Tracker != null && superClass.Tracker.Type.ContainsGenericParameters) {
                throw RubyExceptions.CreateTypeError(String.Format(
                    "{0}: cannot inherit from open generic instantiation {1}. Only closed instantiations are supported.",
                    name, superClass.Name
                ));
            }

            string qualifiedName = owner.MakeNestedModuleName(name);
            RubyClass result = CreateClass(qualifiedName, null, null, null, null, superClass, null, true, false);

            if (name != null) {
                owner.SetConstant(name, result);
            }

            ClassInheritedEvent(superClass, result);

            return result;
        }

        #endregion

        #region Libraries

        internal RubyModule/*!*/ DefineLibraryModule(string name, Type/*!*/ type, Action<RubyModule> instanceTrait,
            Action<RubyModule> classTrait, RubyModule[]/*!*/ mixins, bool isSelfContained) {
            Assert.NotNull(type);
            Assert.NotNullItems(mixins);

            lock (ModuleCacheSyncRoot) {
                RubyModule module;

                if (TryGetModuleNoLock(type, out module)) {
                    module.IncludeLibraryModule(instanceTrait, classTrait, mixins);
                    return module;
                }

                if (name == null) {
                    name = RubyUtils.GetQualifiedName(type);
                }

                // Setting tracker on the module makes CLR methods visible.
                // Hide CLR methods if the type itself defines RubyMethods and is not an extension of another type.
                TypeTracker tracker = isSelfContained ? null : ReflectionCache.GetTypeTracker(type);

                module = CreateModule(name, instanceTrait, classTrait, null, tracker);
                module.SetMixins(mixins);

                AddModuleToCacheNoLock(type, module);
                return module;
            }
        }

        // isSelfContained: The traits are defined on type (public static methods marked by RubyMethod attribute).
        internal RubyClass/*!*/ DefineLibraryClass(string name, Type/*!*/ type, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait,
            RubyClass super, RubyModule[]/*!*/ mixins, Delegate[] factories, bool isSelfContained, bool builtin) {

            Assert.NotNull(type);

            RubyClass result;
            lock (ModuleCacheSyncRoot) {
                if (TryGetClassNoLock(type, out result)) {
                    if (super != null && super != result.SuperClass) {
                        // TODO: better message
                        throw new InvalidOperationException("Cannot change super class");
                    }

                    result.IncludeLibraryModule(instanceTrait, classTrait, mixins);
                    if (factories != null) {
                        result.Factories = ArrayUtils.AppendRange(result.Factories, factories);
                    }

                    return result;
                }

                if (name == null) {
                    name = RubyUtils.GetQualifiedName(type);
                }

                if (super == null) {
                    super = GetOrCreateClassNoLock(type.BaseType);
                }

                // Setting tracker on the class makes CLR methods visible.
                // Hide CLR methods if the type itself defines RubyMethods and is not an extension of another type.
                TypeTracker tracker = isSelfContained ? null : ReflectionCache.GetTypeTracker(type);

                result = CreateClass(name, type, null, instanceTrait, classTrait, super, tracker, false, false);
                result.SetMixins(mixins);
                result.Factories = factories;

                AddModuleToCacheNoLock(type, result);
            }

            if (!builtin) {
                ClassInheritedEvent(super, result);
            }

            return result;
        }

        #endregion

        #region Getting Modules and Classes from objects, CLR types and CLR namespaces.

        public RubyModule/*!*/ GetModule(Type/*!*/ type) {
            if (type.IsInterface) {
                return GetOrCreateModule(type);
            } else {
                return GetOrCreateClass(type);
            }
        }

        public RubyModule/*!*/ GetModule(NamespaceTracker/*!*/ namespaceTracker) {
            return GetOrCreateModule(namespaceTracker);
        }

        public RubyClass/*!*/ GetClass(Type/*!*/ type) {
            ContractUtils.Requires(!type.IsInterface);
            return GetOrCreateClass(type);
        }

        /// <summary>
        /// Gets a class of the specified object (skips any singletons).
        /// </summary>
        public RubyClass/*!*/ GetClassOf(object obj)
            //^ ensures !result.IsSingletonClass;
        {
            if (obj == null) {
                return _nilClass;
            }

            if (obj is bool) {
                return (bool)obj ? _trueClass : _falseClass;
            }

            IRubyObject rubyObj = obj as IRubyObject;
            if (rubyObj != null) {
                var result = rubyObj.Class;
                Debug.Assert(result != null, "Invalid IRubyObject implementation: Class should not be null");
                return result;
            }

            return GetOrCreateClass(obj.GetType());
        }

        /// <summary>
        /// Gets a singleton or class for <c>obj</c>.
        /// </summary>
        public RubyClass/*!*/ GetImmediateClassOf(object obj) {
            RubyModule module = obj as RubyModule;
            if (module != null) {
                return module.SingletonClass;
            }

            RubyInstanceData data;
            RubyClass result = TryGetInstanceSingletonOf(obj, out data);
            if (result != null) {
                return result;
            }

            return GetClassOf(obj, data);
        }

        private RubyClass TryGetInstanceSingletonOf(object obj, out RubyInstanceData data) {
            //^ ensures return != null ==> return.IsSingletonClass
            Debug.Assert(!(obj is RubyModule));

            data = TryGetInstanceData(obj);
            if (data != null) {
                return data.InstanceSingleton;
            }

            return null;
        }

        private void SetInstanceSingletonOf(object obj, ref RubyInstanceData data, RubyClass/*!*/ singleton) {
            Debug.Assert(!(obj is RubyModule) && singleton != null);

            if (data == null) {
                data = GetInstanceData(obj);
            }

            data.ImmediateClass = singleton;
        }

        private RubyClass/*!*/ GetClassOf(object obj, RubyInstanceData data) {
            Debug.Assert(!(obj is RubyModule));
            RubyClass result;

            if (data != null) {
                result = data.ImmediateClass;
                if (result != null) {
                    return result.IsSingletonClass ? result.SuperClass : result;
                }

                result = this.GetClassOf(obj);
                data.ImmediateClass = result;
            } else {
                result = this.GetClassOf(obj);
            }

            return result;
        }

        public bool IsInstanceOf(object value, object classObject) {
            RubyClass c = classObject as RubyClass;
            if (c != null) {
                return GetClassOf(value).IsSubclassOf(c);
            }

            return false;
        }

        #endregion

        #region Member Resolution

        public RubyMemberInfo ResolveMethod(object target, string/*!*/ name, bool includePrivate) {
            return GetImmediateClassOf(target).ResolveMethod(name, includePrivate);
        }

        public RubyMemberInfo ResolveSuperMethod(object target, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            return GetImmediateClassOf(target).ResolveSuperMethod(name, declaringModule);
        }

        public bool TryGetModule(Scope autoloadScope, string/*!*/ moduleName, out RubyModule result) {
            result = _objectClass;
            int pos = 0;
            while (true) {
                int pos2 = moduleName.IndexOf("::", pos);
                string partialName;
                if (pos2 < 0) {
                    partialName = moduleName.Substring(pos);
                } else {
                    partialName = moduleName.Substring(pos, pos2 - pos);
                    pos = pos2 + 2;
                }
                object tmp;
                if (!result.TryResolveConstant(autoloadScope, partialName, out tmp)) {
                    result = null;
                    return false;
                }
                result = (tmp as RubyModule);
                if (result == null) {
                    return false;
                } else if (pos2 < 0) {
                    return true;
                }
            }
        }

        #endregion

        #region Object Operations

        internal RubyInstanceData TryGetInstanceData(object obj) {
            IRubyObject rubyObject = obj as IRubyObject;
            if (rubyObject != null) {
                return rubyObject.TryGetInstanceData();
            }

            if (obj == null) {
                return _nilInstanceData;
            }

            RubyInstanceData result;
            if (RubyUtils.IsRubyValueType(obj)) {
                lock (ValueTypeInstanceDataSyncRoot) {
                    _valueTypeInstanceData.TryGetValue(obj, out result);
                }
                return result;
            }

            _referenceTypeInstanceData.TryGetValue(obj, out result);
            return result;
        }

        internal RubyInstanceData/*!*/ GetInstanceData(object obj) {
            IRubyObject rubyObject = obj as IRubyObject;
            if (rubyObject != null) {
                return rubyObject.GetInstanceData();
            }

            if (obj == null) {
                return _nilInstanceData;
            }

            RubyInstanceData result;
            if (RubyUtils.IsRubyValueType(obj)) {
                lock (ValueTypeInstanceDataSyncRoot) {
                    if (!_valueTypeInstanceData.TryGetValue(obj, out result)) {
                        _valueTypeInstanceData.Add(obj, result = new RubyInstanceData());
                    }
                }
                return result;
            }

            return _referenceTypeInstanceData.GetValue(obj);
        }

        public bool HasInstanceVariables(object obj) {
            return TryGetInstanceData(obj) != null;
        }

        public string[]/*!*/ GetInstanceVariableNames(object obj) {
            RubyInstanceData data = TryGetInstanceData(obj);
            return (data != null) ? data.GetInstanceVariableNames() : ArrayUtils.EmptyStrings;
        }

        public bool TryGetInstanceVariable(object obj, string/*!*/ name, out object value) {
            RubyInstanceData data = TryGetInstanceData(obj);
            if (data == null || !data.TryGetInstanceVariable(name, out value)) {
                value = null;
                return false;
            }
            return true;
        }

        public void SetInstanceVariable(object obj, string/*!*/ name, object value) {
            GetInstanceData(obj).SetInstanceVariable(name, value);
        }

        public bool TryRemoveInstanceVariable(object obj, string/*!*/ name, out object value) {
            RubyInstanceData data = TryGetInstanceData(obj);
            if (data == null || !data.TryRemoveInstanceVariable(name, out value)) {
                value = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Copies instance data from source to target object (i.e. instance variables, tainted, frozen flags).
        /// If the source has a singleton class it's members are copied to the target as well.
        /// Assumes a fresh instance of target, with no instance data.
        /// </summary>
        public void CopyInstanceData(object source, object target, bool copySingletonMembers) {
            CopyInstanceData(source, target, false, true, copySingletonMembers);
        }

        public void CopyInstanceData(object source, object target, bool copyFrozenState, bool copyTaint, bool copySingletonMembers) {
            Debug.Assert(!copySingletonMembers || !(source is RubyModule));
            Debug.Assert(TryGetInstanceData(target) == null);

            var sourceData = TryGetInstanceData(source);
            if (sourceData != null) {
                RubyInstanceData targetData = null;

                if (copyTaint) {
                    if (targetData == null) targetData = GetInstanceData(target);
                    targetData.Tainted = sourceData.Tainted;
                }

                if (copyFrozenState && sourceData.Frozen) {
                    if (targetData == null) targetData = GetInstanceData(target);
                    targetData.Freeze();
                }

                if (sourceData.HasInstanceVariables) {
                    if (targetData == null) targetData = GetInstanceData(target);
                    sourceData.CopyInstanceVariablesTo(targetData);
                }

                RubyClass singleton;
                if (copySingletonMembers && (singleton = sourceData.InstanceSingleton) != null) {
                    if (targetData == null) targetData = GetInstanceData(target);
                    
                    var dup = singleton.Duplicate(target);
                    dup.InitializeMembersFrom(singleton);

                    SetInstanceSingletonOf(target, ref targetData, dup);
                }
            }
        }

        public bool IsObjectFrozen(object obj) {
            var state = obj as IRubyObjectState;
            if (state != null) {
                return state.IsFrozen;
            }

            RubyInstanceData data = TryGetInstanceData(obj);
            return data != null ? data.Frozen : false;
        }

        public bool IsObjectTainted(object obj) {
            var state = obj as IRubyObjectState;
            if (state != null) {
                return state.IsTainted;
            }

            RubyInstanceData data = TryGetInstanceData(obj);
            return data != null ? data.Tainted : false;
        }

        public void FreezeObject(object obj) {
            var state = obj as IRubyObjectState;
            if (state != null) {
                state.Freeze();
            } else {
                GetInstanceData(obj).Freeze();
            }
        }

        public void SetObjectTaint(object obj, bool taint) {
            var state = obj as IRubyObjectState;
            if (state != null) {
                state.IsTainted = taint;
            } else {
                GetInstanceData(obj).Tainted = taint;
            }
        }

        public T TaintObjectBy<T>(T obj, object taintSource) {
            if (IsObjectTainted(taintSource)) {
                SetObjectTaint(obj, true);
            }
            return obj;
        }

        public T FreezeObjectBy<T>(T obj, object frozenStateSource) {
            if (IsObjectFrozen(frozenStateSource)) {
                FreezeObject(obj);
            }
            return obj;
        }

        #endregion

        #region Global Variables

        public object GetGlobalVariable(string/*!*/ name) {
            object value;
            TryGetGlobalVariable(null, name, out value);
            return value;
        }

        public void DefineGlobalVariable(string/*!*/ name, object value) {
            lock (_globalVariables) {
                _globalVariables[name] = new GlobalVariableInfo(value);
            }
        }

        public void DefineReadOnlyGlobalVariable(string/*!*/ name, object value) {
            lock (_globalVariables) {
                _globalVariables[name] = new ReadOnlyGlobalVariableInfo(value);
            }
        }

        public void DefineGlobalVariable(string/*!*/ name, GlobalVariable/*!*/ variable) {
            ContractUtils.RequiresNotNull(variable, "variable");
            lock (_globalVariables) {
                _globalVariables[name] = variable;
            }
        }

        internal void DefineGlobalVariableNoLock(string/*!*/ name, GlobalVariable/*!*/ variable) {
            _globalVariables[name] = variable;
        }

        public bool DeleteGlobalVariable(string/*!*/ name) {
            lock (_globalVariables) {
                return _globalVariables.Remove(name);
            }
        }

        public void AliasGlobalVariable(string/*!*/ newName, string/*!*/ oldName) {
            lock (_globalVariables) {
                GlobalVariable existing;
                if (!_globalVariables.TryGetValue(oldName, out existing)) {
                    DefineGlobalVariableNoLock(oldName, existing = new GlobalVariableInfo(null, false));
                }
                _globalVariables[newName] = existing;
            }
        }

        // null scope should be used only when accessed outside Ruby code; in that case no scoped variables are available
        // we need scope here, bacause the variable might be an alias for scoped variable (regex matches):
        public void SetGlobalVariable(RubyScope scope, string/*!*/ name, object value) {
            lock (_globalVariables) {
                GlobalVariable global;
                if (_globalVariables.TryGetValue(name, out global)) {
                    global.SetValue(this, scope, name, value);
                    return;
                }

                _globalVariables[name] = new GlobalVariableInfo(value);
            }
        }

        // null scope should be used only when accessed outside Ruby code; in that case no scoped variables are available
        // we need scope here, bacause the variable might be an alias for scoped variable (regex matches):
        public bool TryGetGlobalVariable(RubyScope scope, string/*!*/ name, out object value) {
            lock (_globalVariables) {
                GlobalVariable global;
                if (_globalVariables.TryGetValue(name, out global)) {
                    value = global.GetValue(this, scope);
                    return true;
                }
            }
            value = null;
            return false;
        }

        internal bool TryGetGlobalVariable(string/*!*/ name, out GlobalVariable variable) {
            lock (_globalVariables) {
                return _globalVariables.TryGetValue(name, out variable);
            }
        }

        // special global accessors:

        internal Exception SetCurrentException(object value) {
            Exception e = value as Exception;

            // "$! = nil" is allowed
            if (value != null && e == null) {
                throw RubyExceptions.CreateTypeError("assigning non-exception to $!");
            }

            return _currentException = e;
        }

        internal RubyArray GetCurrentExceptionBacktrace() {
            // Under certain circumstances MRI invokes "backtrace" method, but it is quite unstable (crashes sometimes).
            // Therefore we don't call the method and return the backtrace immediately.
            Exception e = _currentException;
            return (e != null) ? RubyExceptionData.GetInstance(e).Backtrace : null;
        }

        internal RubyArray SetCurrentExceptionBacktrace(object value) {
            // first check availability of the current exception:
            Exception e = _currentException;
            if (e == null) {
                throw RubyExceptions.CreateArgumentError("$! not set");
            }

            // check assigned value:
            RubyArray array = RubyUtils.AsArrayOfStrings(value);
            if (value != null && array == null) {
                throw RubyExceptions.CreateTypeError("backtrace must be Array of String");
            }

            RubyExceptionData.GetInstance(e).Backtrace = array;
            return array;
        }

        public void SetSafeLevel(int value) {
            if (_currentSafeLevel <= value) {
                _currentSafeLevel = value;
            } else {
                throw RubyExceptions.CreateSecurityError(String.Format("tried to downgrade safe level from {0} to {1}",
                    _currentSafeLevel, value));
            }
        }

        public void SetKCode(MutableString encodingName) {
            // TODO: Ruby 1.9 reports a warning:

            // we allow nil as Ruby 1.9 does (Ruby 1.8 doesn't):
            if (encodingName == null) {
                _kcode = KCode.Default;
            } else if (encodingName.IsEmpty) {
                _kcode = KCode.Binary;
            } else {
                switch (encodingName.GetChar(0)) {
                    case 'E':
                    case 'e':
                        _kcode = KCode.Euc;
                        break;

                    case 'S':
                    case 's':
                        _kcode = KCode.Sjis;
                        break;

                    case 'U':
                    case 'u':
                        _kcode = KCode.Utf8;
                        break;

                    default:
                        _kcode = KCode.Binary;
                        break;
                }
            }
        }

        public string GetKCodeName() {
            switch (_kcode) {
                case KCode.Default:
                case KCode.Binary: return "NONE";
                case KCode.Euc: return "EUC";
                case KCode.Sjis: return "SJIS";
                case KCode.Utf8: return "UTF8";
                default: throw Assert.Unreachable;
            }
        }

        public Encoding/*!*/ GetKCodeEncoding() {
            switch (_kcode) {
                case KCode.Default:
                case KCode.Binary: return BinaryEncoding.Instance;
                case KCode.Euc: return Encoding.GetEncoding("EUC-JP");
                case KCode.Sjis: return Encoding.GetEncoding("SJIS");
                case KCode.Utf8: return Encoding.UTF8;
                default: throw Assert.Unreachable;
            }
        }

        #endregion

        #region IO

        public void ReportWarning(string/*!*/ message) {
            ReportWarning(message, false);
        }

        public void ReportWarning(string/*!*/ message, bool isVerbose) {
            _runtimeErrorSink.Add(null, message, SourceSpan.None, isVerbose ? Errors.RuntimeVerboseWarning : Errors.RuntimeWarning, Severity.Warning);
        }

        public RubyIO GetDescriptor(int fileDescriptor) {
            lock (_fileDescriptors) {
                if (fileDescriptor < 0 || fileDescriptor >= _fileDescriptors.Count) {
                    return null;
                } else {
                    return _fileDescriptors[fileDescriptor];
                }
            }
        }

        public int AddDescriptor(RubyIO/*!*/ descriptor) {
            ContractUtils.RequiresNotNull(descriptor, "descriptor");

            lock (_fileDescriptors) {
                for (int i = 0; i < _fileDescriptors.Count; ++i) {
                    if (_fileDescriptors[i] == null) {
                        _fileDescriptors[i] = descriptor;
                        return i;
                    }
                }
                _fileDescriptors.Add(descriptor);
                return _fileDescriptors.Count - 1;
            }
        }

        public void RemoveDescriptor(int descriptor) {
            ContractUtils.Requires(!RubyIO.IsConsoleDescriptor(descriptor));

            lock (_fileDescriptors) {
                if (descriptor < _fileDescriptors.Count) {
                    throw new ArgumentException("Invalid file descriptor", "descriptor");
                }

                _fileDescriptors[descriptor] = null;
            }
        }

        #endregion

        #region Callbacks

        // Ruby 1.8: called after method is added, except for alias_method which calls it before
        // Ruby 1.9: called before method is added
        internal void MethodAdded(RubyModule/*!*/ module, string/*!*/ name) {
            Assert.NotNull(module, name);

            // not called on singleton classes:
            if (!module.IsSingletonClass) {
                MethodEvent(ref _methodAddedCallbackSite, Symbols.MethodAdded,
                    this, module, name);
            } else {
                SingletonMethodEvent(ref _singletonMethodAddedCallbackSite, Symbols.SingletonMethodAdded,
                    this, ((RubyClass)module).SingletonClassOf, name);
            }
        }

        internal void MethodRemoved(RubyModule/*!*/ module, string/*!*/ name) {
            Assert.NotNull(module, name);

            // not called on singleton classes:
            if (!module.IsSingletonClass) {
                MethodEvent(ref _methodRemovedCallbackSite, Symbols.MethodRemoved,
                    this, module, name);
            } else {
                SingletonMethodEvent(ref _singletonMethodRemovedCallbackSite, Symbols.SingletonMethodRemoved,
                    this, ((RubyClass)module).SingletonClassOf, name);
            }
        }

        internal void MethodUndefined(RubyModule/*!*/ module, string/*!*/ name) {
            Assert.NotNull(module, name);

            // not called on singleton classes:
            if (!module.IsSingletonClass) {
                MethodEvent(ref _methodUndefinedCallbackSite, Symbols.MethodUndefined,
                    this, module, name);
            } else {
                SingletonMethodEvent(ref _singletonMethodUndefinedCallbackSite, Symbols.SingletonMethodUndefined,
                    this, ((RubyClass)module).SingletonClassOf, name);
            }
        }

        internal void ReportTraceEvent(string/*!*/ operation, RubyScope/*!*/ scope, RubyModule/*!*/ module, string/*!*/ name, string fileName, int lineNumber) {
            if (_traceListener != null && !_traceListenerSuspended) {
                
                try {
                    _traceListenerSuspended = true;

                    _traceListener.Call(new[] {
                        MutableString.Create(operation),                                          // event
                        fileName != null ? MutableString.Create(fileName) : null,                 // file
                        ScriptingRuntimeHelpers.Int32ToObject(lineNumber),                        // line
                        SymbolTable.StringToId(name),                                             // TODO: alias
                        new Binding(scope),                                                       // binding
                        module.IsSingletonClass ? ((RubyClass)module).SingletonClassOf : module   // module
                    });
                } finally {
                    _traceListenerSuspended = false;
                }
            }
        }

        #endregion

        #region Library Data

        private void EnsureLibraryData() {
            if (_libraryData == null) {
                Interlocked.CompareExchange(ref _libraryData, new Dictionary<object, object>(), null);
            }
        }

        public bool TryGetLibraryData(object key, out object value) {
            EnsureLibraryData();

            lock (_libraryData) {
                return _libraryData.TryGetValue(key, out value);
            }
        }

        public object GetOrCreateLibraryData(object key, Func<object> valueFactory) {
            object value;
            if (TryGetLibraryData(key, out value)) {
                return value;
            }

            value = valueFactory();

            object actualResult;
            TryAddLibraryData(key, value, out actualResult);
            return actualResult;
        }

        public bool TryAddLibraryData(object key, object value, out object actualValue) {
            EnsureLibraryData();

            lock (_libraryData) {
                if (_libraryData.TryGetValue(key, out actualValue)) {
                    return false;
                }

                _libraryData.Add(key, actualValue = value);
                return true;
            }
        }

        #endregion

        #region Parsing, Compilation

        protected override ScriptCode CompileSourceCode(SourceUnit/*!*/ sourceUnit, CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
            ContractUtils.RequiresNotNull(options, "options");
            ContractUtils.RequiresNotNull(errorSink, "errorSink");
            ContractUtils.Requires(sourceUnit.LanguageContext == this, "Language mismatch.");

#if DEBUG
            if (RubyOptions.LoadFromDisk) {
                string code;
                Utils.Log(String.Format("{0} {1}", Options.InterpretedMode ? "interpreting" : "compiling", sourceUnit.Path ??
                    ((code = sourceUnit.GetCode()).Length < 100 ? code : code.Substring(0, 100))
                    .Replace('\r', ' ').Replace('\n', ' ')
                ), "COMPILER");
            }
#endif

            Expression<DlrMainCallTarget> lambda = ParseSourceCode<DlrMainCallTarget>(sourceUnit, (RubyCompilerOptions)options, errorSink);
            if (lambda == null) {
                return null;
            }

            if (Options.InterpretedMode) {
                return new InterpretedScriptCode(lambda, sourceUnit);
            } else {
                return new ScriptCode(lambda, sourceUnit);
            }
        }

#if MEASURE_AST
        private static readonly object _TransformationLock = new object();
        private static readonly Dictionary<ExpressionType, int> _TransformationHistogram = new Dictionary<ExpressionType,int>();
#endif

        private static long _ParseTimeTicks;
        private static long _AstGenerationTimeTicks;

        internal Expression<T> ParseSourceCode<T>(SourceUnit/*!*/ sourceUnit, RubyCompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            long ts1, ts2;

            ts1 = Stopwatch.GetTimestamp();
            SourceUnitTree ast = new Parser().Parse(sourceUnit, options, errorSink);
            ts2 = Stopwatch.GetTimestamp();

            Interlocked.Add(ref _ParseTimeTicks, ts2 - ts1);

            if (ast == null) {
                return null;
            }

            Expression<T> lambda;
#if MEASURE_AST
            lock (_TransformationLock) {
                var oldHistogram = System.Linq.Expressions.Expression.Histogram;
                System.Linq.Expressions.Expression.Histogram = _TransformationHistogram;
                try {
#endif
            ts1 = Stopwatch.GetTimestamp();
            lambda = TransformTree<T>(ast, sourceUnit, options);
            ts2 = Stopwatch.GetTimestamp();
            Interlocked.Add(ref _AstGenerationTimeTicks, ts2 - ts1);

#if MEASURE_AST
                } finally {
                    System.Linq.Expressions.Expression.Histogram = oldHistogram;
                }
            }
#endif

            return lambda;
        }

        internal Expression<T>/*!*/ TransformTree<T>(SourceUnitTree/*!*/ ast, SourceUnit/*!*/ sourceUnit, RubyCompilerOptions/*!*/ options) {
            return ast.Transform<T>(
                new AstGenerator(
                    (RubyBinder)Binder,
                    options,
                    sourceUnit,
                    ast.Encoding,
                    Snippets.Shared.SaveSnippets,
                    DomainManager.Configuration.DebugMode,
                    RubyOptions.EnableTracing,
                    RubyOptions.Profile,
                    RubyOptions.SavePath != null
                )
            );
        }

        public override CompilerOptions/*!*/ GetCompilerOptions() {
            return new RubyCompilerOptions(_options);
        }

        public override ErrorSink GetCompilerErrorSink() {
            return _runtimeErrorSink;
        }

        #endregion

        #region Global Scope

        /// <summary>
        /// Creates a scope extension for DLR scopes that haven't been created by Ruby.
        /// These scopes adds method_missing and const_missing methods to handle lookups to the DLR scope.
        /// </summary>
        internal GlobalScopeExtension/*!*/ InitializeGlobalScope(Scope/*!*/ globalScope) {
            Assert.NotNull(globalScope);

            var scopeExtension = globalScope.GetExtension(ContextId);
            if (scopeExtension != null) {
                return (GlobalScopeExtension)scopeExtension;
            }

            object mainObject = new Object();
            RubyClass singletonClass = CreateMainSingleton(mainObject);

            // method_missing:
            singletonClass.SetMethodNoEvent(this, Symbols.MethodMissing, new RubyMethodGroupInfo(new Delegate[] {
                new Func<RubyScope, BlockParam, object, SymbolId, object[], object>(RubyTopLevelScope.TopMethodMissing)
            }, RubyMemberFlags.Private, singletonClass));

            // TODO:
            // TOPLEVEL_BINDING:
            //singletonClass.SetMethod(Symbols.MethodMissing, new RubyMethodGroupInfo(new Delegate[] {
            //    new Func<CodeContext>(RubyScope.GetTopLevelBinding)
            //}, RubyMethodVisibility.Private, singletonClass, false));

            GlobalScopeExtension result = new GlobalScopeExtension(this, globalScope, mainObject, true);
            singletonClass.SetGlobalScope(result);

            globalScope.SetExtension(ContextId, result);
            return result;
        }

        //
        // Create a scope extension for Ruby program/file.
        //
        internal GlobalScopeExtension/*!*/ CreateScopeExtensionForProgram(Scope/*!*/ globalScope) {
            Assert.NotNull(globalScope);

            object mainObject = new Object();
            CreateMainSingleton(mainObject);

            return new GlobalScopeExtension(this, globalScope, mainObject, false);
        }

        public override int ExecuteProgram(SourceUnit/*!*/ program) {
            try {
                RubyCompilerOptions options = new RubyCompilerOptions(_options);
                ScriptCode compiledCode = CompileSourceCode(program, options, _runtimeErrorSink);

                Scope scope = new Scope();
                scope.SetExtension(ContextId, CreateScopeExtensionForProgram(scope));

                compiledCode.Run(scope);
            } catch (SystemExit e) {
                return e.Status;
            }

            return 0;
        }

        #endregion

        #region Shutdown

        private List<BlockParam> _shutdownHandlers = new List<BlockParam>();

        public void RegisterShutdownHandler(BlockParam proc) {
            lock (_shutdownHandlers) {
                _shutdownHandlers.Add(proc);
            }
        }

        public override void Shutdown() {
#if !SILVERLIGHT
            _upTime.Stop();

            if (RubyOptions.Profile) {
                var profile = Profiler.Instance.GetProfile();
                using (TextWriter writer = File.CreateText("profile.log")) {
                    int maxLength = 0;
                    long totalTicks = 0L;
                    var keys = new string[profile.Count];
                    var values = new long[profile.Count];

                    int i = 0;
                    foreach (var counter in profile) {
                        if (counter.Key.Length > maxLength) {
                            maxLength = counter.Key.Length;
                        }

                        totalTicks += counter.Value;

                        keys[i] = counter.Key;
                        values[i] = counter.Value;
                        i++;
                    }

                    Array.Sort(values, keys);

                    for (int j = keys.Length - 1; j >= 0; j--) {
                        long ticks = values[j];

                        writer.WriteLine("{0,-" + (maxLength + 4) + "} {1,8:F0} ms {2,5:F1}%", keys[j],
                            new TimeSpan(Utils.DateTimeTicksFromStopwatch(ticks)).TotalMilliseconds,
                            (((double)ticks) / totalTicks * 100)
                        );
                    }

                    writer.WriteLine("{0,-" + (maxLength + 4) + "} {1,8:F0} ms", "total",
                        new TimeSpan(Utils.DateTimeTicksFromStopwatch(totalTicks)).TotalMilliseconds
                    );
                }
            }

            if (Options.PerfStats) {
                Console.WriteLine(String.Format(@"
  total:         {0}
  parse:         {1}
  ast transform: {2}
  script code:   {3}
  il:            {4}
  binding:       {5} ({6} calls)
",
                    _upTime.Elapsed,
                    new TimeSpan(_ParseTimeTicks),
                    new TimeSpan(_AstGenerationTimeTicks),
                    new TimeSpan(Loader._ScriptCodeGenerationTimeTicks),
                    new TimeSpan(Loader._ILGenerationTimeTicks),
#if MEASURE
                    new TimeSpan(MetaAction.BindingTimeTicks), 
                    MetaAction.BindCallCount
#else
 "N/A", "N/A"
#endif
));

#if MEASURE_BINDING
                Console.WriteLine();
                Console.WriteLine("---- MetaAction kinds ----");
                Console.WriteLine();

                PerfTrack.DumpHistogram(MetaAction.HistogramOfKinds);

                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("---- MetaAction instances ----");
                Console.WriteLine();

                PerfTrack.DumpHistogram(MetaAction.HistogramOfInstances);

                Console.WriteLine();
#endif

#if MEASURE_AST
                Console.WriteLine();
                Console.WriteLine("---- Ruby Parser generated Expression Trees ----");
                Console.WriteLine();
                
                PerfTrack.DumpHistogram(_TransformationHistogram);

                Console.WriteLine();
#endif
                PerfTrack.DumpStats();
            }
#endif
            _loader.SaveCompiledCode();

            List<BlockParam> handlers = new List<BlockParam>();

            while (_shutdownHandlers.Count > 0) {
                lock (_shutdownHandlers) {
                    handlers.AddRange(_shutdownHandlers);
                    _shutdownHandlers.Clear();
                }

                for (int i = handlers.Count - 1; i >= 0; --i) {
                    try {
                        object result;
                        handlers[i].Yield(out result);
                    } catch (SystemExit) {
                        // ignored
                    }
                }

                handlers.Clear();
            }
        }

        #endregion

        #region Exceptions

        // Formats exceptions like Ruby does, for example:
        //
        //repro.rb:2:in `fetch': wrong number of arguments (0 for 1) (ArgumentError)
        //        from repro.rb:2:in `test'
        //        from repro.rb:5
        public override string/*!*/ FormatException(Exception/*!*/ exception) {
            var syntaxError = exception as SyntaxError;
            if (syntaxError != null && syntaxError.HasLineInfo) {
                return FormatErrorMessage(syntaxError.Message, null, syntaxError.File, syntaxError.Line, syntaxError.Column, syntaxError.LineSourceCode);
            }

            var exceptionClass = GetClassOf(exception);
            RubyExceptionData data = RubyExceptionData.GetInstance(exception);
            var message = RubyExceptionData.GetClrMessage(data.Message, exceptionClass.Name);

            RubyArray backtrace = data.Backtrace;

            StringBuilder sb = new StringBuilder();
            if (backtrace != null && backtrace.Count > 0) {
                sb.AppendFormat("{0}: {1} ({2})", backtrace[0], message, exceptionClass.Name);
                sb.AppendLine();

                for (int i = 1; i < backtrace.Count; i++) {
                    sb.Append("\tfrom ").Append(backtrace[i]).AppendLine();
                }
            } else {
                sb.AppendFormat("unknown: {0} ({1})", message, exceptionClass.Name).AppendLine();
            }

            // display the raw CLR exception & strack trace if requested
            if (Options.ShowClrExceptions) {
                sb.AppendLine().AppendLine();
                sb.AppendLine("CLR exception:");
                sb.Append(base.FormatException(exception));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        internal static string/*!*/ FormatErrorMessage(string/*!*/ message, string prefix, string file, int line, int column, string lineSource) {
            var sb = new StringBuilder();
            sb.Append(file ?? "unknown");
            sb.Append(':');
            sb.Append(file != null ? line : 0);
            sb.Append(": ");
            if (prefix != null) {
                sb.Append(prefix);
                sb.Append(": ");
            }
            sb.Append(message);
            sb.AppendLine();
            if (lineSource != null) {
                sb.Append(lineSource);
                sb.AppendLine();
                sb.Append(' ', column);
                sb.Append('^');
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #region Language Context Overrides

        public override TService GetService<TService>(params object[] args) {
            if (typeof(TService) == typeof(TokenizerService)) {
                return (TService)(object)new Tokenizer();
            }

            return base.GetService<TService>(args);
        }

        public override void SetSearchPaths(ICollection<string/*!*/>/*!*/ paths) {
            ContractUtils.RequiresNotNullItems(paths, "paths");
            _loader.SetLoadPaths(paths);
        }

        // Might run an arbitrary user code.
        public override ICollection<string>/*!*/ GetSearchPaths() {
            return _loader.GetLoadPathStrings();
        }

        public override SourceCodeReader/*!*/ GetSourceReader(Stream/*!*/ stream, Encoding/*!*/ defaultEncoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
            ContractUtils.Requires(stream.CanRead && stream.CanSeek, "stream", "The stream must support seeking and reading");

#if SILVERLIGHT
            return base.GetSourceReader(stream, defaultEncoding);
#else
            if (_options.Compatibility <= RubyCompatibility.Ruby18) {
                return new SourceCodeReader(new StreamReader(stream, BinaryEncoding.Instance, false), BinaryEncoding.Instance);
            }

            long initialPosition = stream.Position;
            var reader = new StreamReader(stream, BinaryEncoding.Instance, true);

            // reads preamble, if present:
            reader.Peek();

            Encoding preambleEncoding = (reader.CurrentEncoding != BinaryEncoding.Instance) ? reader.CurrentEncoding : null;
            Encoding rubyPreambleEncoding = null;

            // header:
            string encodingName;
            if (Tokenizer.TryParseEncodingHeader(reader, out encodingName)) {
                rubyPreambleEncoding = RubyEncoding.GetEncodingByRubyName(encodingName);

                // Check if the preamble encoding is an identity on preamble bytes.
                // If not we shouldn't allow such encoding since the encoding of the preamble would be different from the encoding of the file.
                if (!RubyEncoding.IsAsciiIdentity(rubyPreambleEncoding)) {
                    throw new IOException(String.Format("Encoding '{0}' is not allowed in preamble.", rubyPreambleEncoding.WebName));
                }
            }

            // skip the encoding preamble, the resulting stream shouldn't detect the preamble again
            // (this is necessary to override the preamble by Ruby specific one):
            if (preambleEncoding != null) {
                initialPosition += preambleEncoding.GetPreamble().Length;
            }

            stream.Seek(initialPosition, SeekOrigin.Begin);

            var encoding = rubyPreambleEncoding ?? preambleEncoding ?? defaultEncoding;
            return new SourceCodeReader(new StreamReader(stream, encoding, false), encoding);
#endif
        }

        #endregion

        #region MetaObject binding

        public override GetMemberBinder/*!*/ CreateGetMemberBinder(string/*!*/ name, bool ignoreCase) {
            // TODO:
            if (ignoreCase) {
                throw new NotSupportedException("Ignore-case lookup not supported");
            }
            return new RubyGetMemberBinder(this, name);
        }

        public override InvokeMemberBinder/*!*/ CreateCallBinder(string name, bool ignoreCase, params ArgumentInfo[] arguments) {
            if (RubyCallSignature.HasNamedArgument(arguments)) {
                return base.CreateCallBinder(name, ignoreCase, arguments);
            }
            // TODO:
            if (ignoreCase) {
                throw new NotSupportedException("Ignore-case lookup not supported");
            }
            return new RubyInvokeMemberBinder(this, name, arguments);
        }

        public override CreateInstanceBinder/*!*/ CreateCreateBinder(params ArgumentInfo[]/*!*/ arguments) {
            if (RubyCallSignature.HasNamedArgument(arguments)) {
                return base.CreateCreateBinder(arguments);
            }

            return new RubyCreateInstanceBinder(this, arguments);
        }

        #endregion

        #region Special Call Sites

        private readonly Dictionary<KeyValuePair<string, RubyCallSignature>, CallSite> _sendSites =
            new Dictionary<KeyValuePair<string, RubyCallSignature>, CallSite>();

        public CallSite<TSiteFunc>/*!*/ GetOrCreateSendSite<TSiteFunc>(string/*!*/ methodName, RubyCallSignature callSignature)
            where TSiteFunc : class {

            lock (_sendSites) {
                CallSite site;
                if (_sendSites.TryGetValue(new KeyValuePair<string, RubyCallSignature>(methodName, callSignature), out site)) {
                    return (CallSite<TSiteFunc>)site;
                }

                var newSite = CallSite<TSiteFunc>.Create(RubyCallAction.Make(methodName, callSignature));
                _sendSites.Add(new KeyValuePair<string, RubyCallSignature>(methodName, callSignature), newSite);
                return newSite;
            }
        }

        #endregion

        #region Interpretation

        protected override void InterpretExceptionThrow(InterpreterState state, Exception exception, bool isInterpretedThrow) {
            Assert.NotNull(state, exception);
            if (RubyExceptionData.TryGetInstance(exception) == null) {
                RubyExceptionData.AssociateInstance(exception).SetInterpretedTrace(state);
            }
        }

        #endregion
    }
}
