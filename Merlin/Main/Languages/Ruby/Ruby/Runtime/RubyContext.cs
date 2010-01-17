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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// An isolation context: could be per thread/request or shared accross certain threads.
    /// </summary>
    [ReflectionCached]
    public sealed class RubyContext : LanguageContext {
        internal static readonly Guid RubyLanguageGuid = new Guid("F03C4640-DABA-473f-96F1-391400714DAB");
        private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
        private static int _RuntimeIdGenerator = 0;

        // MRI compliance:
        public static readonly string/*!*/ MriVersion = "1.8.6";
        public static readonly string/*!*/ MriReleaseDate = "2009-03-31";
        public static readonly int MriPatchLevel = 368;

        // IronRuby:
        public const string/*!*/ IronRubyVersionString = "0.9.3.0";
        public static readonly Version IronRubyVersion = new Version(0, 9, 3, 0);
        internal const string/*!*/ IronRubyDisplayName = "IronRuby";
        internal const string/*!*/ IronRubyNames = "IronRuby;Ruby;rb";
        internal const string/*!*/ IronRubyFileExtensions = ".rb";

        // TODO: remove
        internal static RubyContext _Default;

        private readonly int _runtimeId;
        private readonly RubyScope/*!*/ _emptyScope;

        private RubyOptions/*!*/ _options;
        private readonly TopNamespaceTracker _namespaces;
        private readonly Loader/*!*/ _loader;
        private readonly Scope/*!*/ _globalScope;
        private readonly RubyMetaBinderFactory/*!*/ _metaBinderFactory;
        private DynamicOperations _operations;
        private readonly RubyBinder _binder;
        private DynamicDelegateCreator _delegateCreator;

        #region Global Variables (thread-safe access)

        /// <summary>
        /// $0
        /// </summary>
        public MutableString CommandLineProgramPath { get; set; }

        /// <summary>
        /// $? of type Process::Status
        /// </summary>
        [ThreadStatic]
        private static object _childProcessExitStatus;

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

        private readonly Dictionary<string/*!*/, GlobalVariable>/*!*/ _globalVariables;
        public object/*!*/ GlobalVariablesLock { get { return _globalVariables; } }

        // not thread safe: use GlobalVariablesLock to synchronize access to the variables:
        public IEnumerable<KeyValuePair<string, GlobalVariable>>/*!*/ GlobalVariables {
            get { return _globalVariables; }
        }
        
        #endregion

        #region Random Number Generator

        private object _randomNumberGeneratorLock = new object();
        private Random _randomNumberGenerator; // lazy
        private object _randomNumberGeneratorSeed = ScriptingRuntimeHelpers.Int32ToObject(0);

        public object RandomNumberGeneratorSeed {
            get { return _randomNumberGeneratorSeed; }
        }

        public void SeedRandomNumberGenerator(IntegerValue value) {
            lock (_randomNumberGeneratorLock) {
                _randomNumberGenerator = new Random(value.IsFixnum ? value.Fixnum : value.Bignum.GetHashCode());
                _randomNumberGeneratorSeed = value.ToObject();
            }
        }

        public Random/*!*/ RandomNumberGenerator {
            get {
                if (_randomNumberGenerator == null) {
                    lock (_randomNumberGeneratorLock) {
                        if (_randomNumberGenerator == null) {
                            _randomNumberGenerator = new Random();
                        }
                    }
                }
                return _randomNumberGenerator;
            }
        }

        #endregion

        #region Threading

        // Thread#main
        private readonly Thread _mainThread;

        // Thread#critical=
        // We just need a bool. But we store the Thread object for easier debugging if there is a hang
        private Thread _criticalThread;
        private readonly object _criticalMonitor = new object();

        #endregion

        #region Tracing

        private readonly RubyInputProvider/*!*/ _inputProvider;
        private Proc _traceListener;

        [ThreadStatic]
        private bool _traceListenerSuspended;
        
        private readonly Stopwatch _upTime;

        // TODO: thread-safety
        internal Action<Expression, MSA.DynamicExpression> CallSiteCreated { get; set; }

        #endregion

        #region Look-aside tables

        /// <summary>
        /// Maps CLR types to Ruby classes/modules.
        /// Doesn't contain classes defined in Ruby.
        /// </summary>
        private readonly Dictionary<Type, RubyModule>/*!*/ _moduleCache;
        private object ModuleCacheLock { get { return _moduleCache; } }

        /// <summary>
        /// Maps CLR namespace trackers to Ruby modules.
        /// </summary>
        private readonly Dictionary<NamespaceTracker, RubyModule>/*!*/ _namespaceCache;
        private object NamespaceCacheLock { get { return _namespaceCache; } }

        // Maps objects to InstanceData. The keys store weak references to the objects.
        // Objects are compared by reference (identity). 
        // An entry can be removed as soon as the key object becomes unreachable.
        private readonly WeakTable<object, RubyInstanceData>/*!*/ _referenceTypeInstanceData;
        private object/*!*/ ReferenceTypeInstanceDataLock { get { return _referenceTypeInstanceData; } }

        // Maps values to InstanceData. The keys store value representatives. 
        // All objects that has the same value (value-equality) map to the same InstanceData.
        // Entries cannot be ever freed since anytime in future one may create a new object whose value has already been mapped to InstanceData.
        private readonly Dictionary<object, RubyInstanceData>/*!*/ _valueTypeInstanceData;
        private object/*!*/ ValueTypeInstanceDataLock { get { return _valueTypeInstanceData; } }

        // not thread-safe: 
        private readonly RubyInstanceData/*!*/ _nilInstanceData = new RubyInstanceData(RubyUtils.NilObjectId);

        #endregion

        #region Class Hierarchy

        public IDisposable/*!*/ ClassHierarchyLocker() {
            return _classHierarchyLock.CreateLocker();
        }

        public IDisposable/*!*/ ClassHierarchyUnlocker() {
            return _classHierarchyLock.CreateUnlocker();
        }

        private readonly CheckedMonitor/*!*/ _classHierarchyLock = new CheckedMonitor();

        [Emitted]
        public int ConstantAccessVersion = 1;

        [Conditional("DEBUG")]
        internal void RequiresClassHierarchyLock() {
            Debug.Assert(_classHierarchyLock.IsLocked, "Code can only be executed while holding class hierarchy lock.");
        }

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
        private RubyClass _comObjectClass;

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
        
        internal RubyClass ComObjectClass {
            get {
#if !SILVERLIGHT
                if (_comObjectClass == null) {
                    GetOrCreateClass(Utils.ComObjectType);
                }
#endif
                return _comObjectClass;
            }
        }

        internal Action<RubyModule>/*!*/ ClassSingletonTrait { get { return _classSingletonTrait; } }
        internal Action<RubyModule>/*!*/ SingletonSingletonTrait { get { return _singletonSingletonTrait; } }

        // Set of names that method_missing defined on any module was resolved for and that are cached. Lazy init.
        // 
        // Note: We used to have this set per module but that doesn't work - see unit test MethodCallCaching_MethodMissing4.
        // Whenever a method is added to a class C we would need to traverse all its subclasses to see if any of them 
        // has the added method in its MissingMethodsCachedInSites table. 
        // TODO: Could we optimize this search? If so we could also free the per-module set if mm is removed.
        internal HashSet<string> MissingMethodsCachedInSites { get; set; }

        #endregion

        #region Properties

        public PlatformAdaptationLayer/*!*/ Platform {
            get { return DomainManager.Platform; }
        }

        public override LanguageOptions Options {
            get { return _options; }
        }

        public RubyOptions RubyOptions {
            get { return _options; }
        }

        internal RubyScope/*!*/ EmptyScope {
            get { return _emptyScope; }
        }

        public Thread MainThread {
            get { return _mainThread; }
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

        public object CriticalMonitor {
            get { return _criticalMonitor; }
        }

        public Thread CriticalThread {
            get { return _criticalThread; }
            set { _criticalThread = value; }
        }

        public Proc TraceListener {
            get { return _traceListener; }
            set { _traceListener = value; }
        }

        public RubyInputProvider/*!*/ InputProvider {
            get { return _inputProvider; }
        }

        public object ChildProcessExitStatus {
            get { return _childProcessExitStatus; }
            set { _childProcessExitStatus = value; }
        }

        public Scope/*!*/ TopGlobalScope {
            get { return _globalScope; }
        }

        internal RubyMetaBinderFactory/*!*/ MetaBinderFactory {
            get { return _metaBinderFactory; }
        }

        public Loader/*!*/ Loader {
            get { return _loader; }
        }

        public bool ShowCls {
            get { return false; }
        }

        public EqualityComparer/*!*/ EqualityComparer {
            get {
                if (_equalityComparer == null) {
                    _equalityComparer = new EqualityComparer(this);
                }
                return _equalityComparer;
            }
        }
        private EqualityComparer _equalityComparer;
        
        public object Verbose { get; set; }

        public override Version LanguageVersion {
            get { return IronRubyVersion; }
        }

        public override Guid LanguageGuid {
            get { return RubyLanguageGuid; }
        }

        public override Guid VendorGuid {
            get { return LanguageVendor_Microsoft; }
        }

        public int RuntimeId {
            get { return _runtimeId; }
        }

        internal TopNamespaceTracker/*!*/ Namespaces {
            get { return _namespaces; }
        }

        #endregion

        #region Initialization

        public RubyContext(ScriptDomainManager/*!*/ manager, IDictionary<string, object> options)
            : base(manager) {
            ContractUtils.RequiresNotNull(manager, "manager");
            _options = new RubyOptions(options);

            _runtimeId = Interlocked.Increment(ref _RuntimeIdGenerator);
            _upTime = new Stopwatch();
            _upTime.Start();
            
            _binder = new RubyBinder(this);

            _metaBinderFactory = new RubyMetaBinderFactory(this);
            _runtimeErrorSink = new RuntimeErrorSink(this);
            _equalityComparer = new EqualityComparer(this);
            _globalVariables = new Dictionary<string, GlobalVariable>();
            _moduleCache = new Dictionary<Type, RubyModule>();
            _namespaceCache = new Dictionary<NamespaceTracker, RubyModule>();
            _referenceTypeInstanceData = new WeakTable<object, RubyInstanceData>();
            _valueTypeInstanceData = new Dictionary<object, RubyInstanceData>();
            _inputProvider = new RubyInputProvider(this, _options.Arguments, _options.ArgumentEncoding);
            _globalScope = DomainManager.Globals;
            _loader = new Loader(this);
            _emptyScope = new RubyTopLevelScope(this);            
            _currentException = null;
            _currentSafeLevel = 0;
            _childProcessExitStatus = null;
            _inputSeparator = MutableString.CreateAscii("\n");
            _outputSeparator = null;
            _stringSeparator = null;
            _itemSeparator = null;
            _mainThread = Thread.CurrentThread;
            
            if (_options.KCode != null) {
                Utils.Log("Initialized to " + _options.KCode.Name, "KCODE");
                KCode = _options.KCode;
            }
            
            // uses k-coding:
            if (_options.MainFile != null) {
                CommandLineProgramPath = EncodePath(_options.MainFile);
            }

            if (_options.Verbosity <= 0) {
                Verbose = null;
            } else if (_options.Verbosity == 1) {
                Verbose = ScriptingRuntimeHelpers.False; 
            } else {
                Verbose = ScriptingRuntimeHelpers.True;
            }

            _namespaces = new TopNamespaceTracker(manager);
            manager.AssemblyLoaded += new EventHandler<AssemblyLoadedEventArgs>(AssemblyLoaded);
            foreach (Assembly asm in manager.GetLoadedAssemblyList()) {
                _namespaces.LoadAssembly(asm);
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

        internal RubyBinder/*!*/ Binder {
            get { return _binder; }
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
            DefineGlobalVariableNoLock("PROGRAM_NAME", Runtime.GlobalVariables.CommandLineProgramPath);

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

            MutableString version = MutableString.CreateAscii(RubyContext.MriVersion);
            MutableString platform = MakePlatformString();

            MutableString releaseDate = MutableString.CreateAscii(RubyContext.MriReleaseDate);
            MutableString rubyEngine = MutableString.CreateAscii("ironruby");

            using (ClassHierarchyLocker()) {
                RubyClass obj = _objectClass;

                obj.SetConstantNoMutateNoLock("RUBY_ENGINE", rubyEngine);
                obj.SetConstantNoMutateNoLock("RUBY_VERSION", version);
                obj.SetConstantNoMutateNoLock("RUBY_PATCHLEVEL", RubyContext.MriPatchLevel);
                obj.SetConstantNoMutateNoLock("RUBY_PLATFORM", platform);
                obj.SetConstantNoMutateNoLock("RUBY_RELEASE_DATE", releaseDate);

                obj.SetConstantNoMutateNoLock("VERSION", version);
                obj.SetConstantNoMutateNoLock("PLATFORM", platform);
                obj.SetConstantNoMutateNoLock("RELEASE_DATE", releaseDate);

                obj.SetConstantNoMutateNoLock("IRONRUBY_VERSION", MutableString.CreateAscii(RubyContext.IronRubyVersionString));

                obj.SetConstantNoMutateNoLock("STDIN", StandardInput);
                obj.SetConstantNoMutateNoLock("STDOUT", StandardOutput);
                obj.SetConstantNoMutateNoLock("STDERR", StandardErrorOutput);

                ConstantStorage argf;
                if (obj.TryGetConstantNoAutoloadCheck("ARGF", out argf)) {
                    _inputProvider.Singleton = argf.Value;
                }

                obj.SetConstantNoMutateNoLock("ARGV", _inputProvider.CommandLineArguments);

                // Hash
                // SCRIPT_LINES__
            }
        }

        private static MutableString/*!*/ MakePlatformString() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                    return MutableString.CreateAscii("i386-darwin");
                
                case PlatformID.Unix:
                    return MutableString.CreateAscii("i386-linux"); 

                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return MutableString.CreateAscii("i386-mswin32");

                default:
                    return MutableString.CreateAscii("unknown");
            }
        }

        private void InitializeFileDescriptors(SharedIO/*!*/ io) {
            Debug.Assert(_fileDescriptors.Count == 0);
            Stream stream = new ConsoleStream(io, ConsoleStreamType.Input);                
            StandardInput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.ReadOnly);
            stream = new ConsoleStream(io, ConsoleStreamType.Output);
            StandardOutput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.WriteOnly | IOMode.WriteAppends);
            stream = new ConsoleStream(io, ConsoleStreamType.ErrorOutput);
            StandardErrorOutput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.WriteOnly | IOMode.WriteAppends);
        }

        // TODO: internal
        public void RegisterPrimitives(
            Action<RubyModule>/*!*/ classSingletonTrait,
            Action<RubyModule>/*!*/ singletonSingletonTrait,
            Action<RubyModule>/*!*/ mainSingletonTrait,

            Action<RubyModule>/*!*/ kernelInstanceTrait,
            Action<RubyModule>/*!*/ kernelClassTrait,
            Action<RubyModule> kernelConstantsInitializer,

            Action<RubyModule>/*!*/ objectInstanceTrait,
            Action<RubyModule>/*!*/ objectClassTrait,
            Action<RubyModule> objectConstantsInitializer,

            Action<RubyModule>/*!*/ moduleInstanceTrait,
            Action<RubyModule>/*!*/ moduleClassTrait,
            Action<RubyModule> moduleConstantsInitializer,

            Action<RubyModule>/*!*/ classInstanceTrait,
            Action<RubyModule>/*!*/ classClassTrait,
            Action<RubyModule> classConstantsInitializer) {

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

            var moduleFactories = new Delegate[] {
                new Func<RubyScope, BlockParam, RubyClass, object>(RubyModule.CreateAnonymousModule),
            };

            var classFactories = new Delegate[] {
                new Func<RubyScope, BlockParam, RubyClass, RubyClass, object>(RubyClass.CreateAnonymousClass),
            };

            // locks to comply with lock requirements:
            using (ClassHierarchyLocker()) {
                _kernelModule = new RubyModule(this, Symbols.Kernel, kernelInstanceTrait, kernelConstantsInitializer, null, null, null, ModuleRestrictions.Builtin);
                _objectClass = new RubyClass(this, Symbols.Object, objectTracker.Type, null, objectInstanceTrait, objectConstantsInitializer, null, null, new[] { _kernelModule }, objectTracker, null, false, false, ModuleRestrictions.Builtin & ~ModuleRestrictions.NoOverrides);
                _moduleClass = new RubyClass(this, Symbols.Module, typeof(RubyModule), null, moduleInstanceTrait, moduleConstantsInitializer, moduleFactories, _objectClass, null, null, null, false, false, ModuleRestrictions.Builtin);
                _classClass = new RubyClass(this, Symbols.Class, typeof(RubyClass), null, classInstanceTrait, classConstantsInitializer, classFactories, _moduleClass, null, null, null, false, false, ModuleRestrictions.Builtin);

                _kernelModule.InitializeDummySingletonClass(_moduleClass, kernelClassTrait);
                _objectClass.InitializeDummySingletonClass(_classClass, objectClassTrait);
                _moduleClass.InitializeDummySingletonClass(_objectClass.SingletonClass, moduleClassTrait);
                _classClass.InitializeDummySingletonClass(_moduleClass.SingletonClass, classClassTrait);

                _objectClass.SetConstantNoMutateNoLock(_moduleClass.Name, _moduleClass);
                _objectClass.SetConstantNoMutateNoLock(_classClass.Name, _classClass);
                _objectClass.SetConstantNoMutateNoLock(_objectClass.Name, _objectClass);
                _objectClass.SetConstantNoMutateNoLock(_kernelModule.Name, _kernelModule);
            }

            AddModuleToCacheNoLock(typeof(Kernel), _kernelModule);
            AddModuleToCacheNoLock(objectTracker.Type, _objectClass);
            AddModuleToCacheNoLock(typeof(RubyObject), _objectClass);
            AddModuleToCacheNoLock(_moduleClass.GetUnderlyingSystemType(), _moduleClass);
            AddModuleToCacheNoLock(_classClass.GetUnderlyingSystemType(), _classClass);
        }

        #endregion

        #region CLR Types and Namespaces

        private void AssemblyLoaded(object sender, AssemblyLoadedEventArgs e) {
            _namespaces.LoadAssembly(e.Assembly);
        }

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

            lock (ModuleCacheLock) {
                return GetOrCreateModuleNoLock(tracker);
            }
        }

        internal bool TryGetModule(NamespaceTracker/*!*/ namespaceTracker, out RubyModule result) {
            lock (NamespaceCacheLock) {
                return _namespaceCache.TryGetValue(namespaceTracker, out result);
            }
        }

        internal RubyModule/*!*/ GetOrCreateModule(Type/*!*/ moduleType) {
            Debug.Assert(RubyModule.IsModuleType(moduleType));

            lock (ModuleCacheLock) {
                return GetOrCreateModuleNoLock(moduleType);
            }
        }

        public bool TryGetModule(Type/*!*/ type, out RubyModule result) {
            lock (ModuleCacheLock) {
                return _moduleCache.TryGetValue(type, out result);
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
            lock (ModuleCacheLock) {
                return GetOrCreateClassNoLock(type);
            }
        }

        private RubyModule/*!*/ GetOrCreateModuleNoLock(NamespaceTracker/*!*/ tracker) {
            Assert.NotNull(tracker);

            RubyModule result;
            if (_namespaceCache.TryGetValue(tracker, out result)) {
                return result;
            }

            result = CreateModule(GetQualifiedName(tracker), null, null, null, null, tracker, null, ModuleRestrictions.None);
            _namespaceCache[tracker] = result;
            return result;
        }

        private RubyModule/*!*/ GetOrCreateModuleNoLock(Type/*!*/ moduleType) {
            Debug.Assert(RubyModule.IsModuleType(moduleType));

            RubyModule result;
            if (_moduleCache.TryGetValue(moduleType, out result)) {
                return result;
            }

            TypeTracker tracker = (TypeTracker)TypeTracker.FromMemberInfo(moduleType);

            RubyModule[] mixins;
            if (moduleType.IsGenericType && !moduleType.IsGenericTypeDefinition) {
                // I<T0..Tn> mixes in its generic definition I<,..,>
                mixins = new[] { GetOrCreateModuleNoLock(moduleType.GetGenericTypeDefinition()) };
            } else {
                mixins = null;
            }

            result = CreateModule(GetQualifiedNameNoLock(moduleType), null, null, null, mixins, null, tracker, ModuleRestrictions.None);
            _moduleCache[moduleType] = result;
            return result;
        }

        private RubyClass/*!*/ GetOrCreateClassNoLock(Type/*!*/ type) {
            Debug.Assert(!RubyModule.IsModuleType(type));

            RubyClass result;
            if (TryGetClassNoLock(type, out result)) {
                return result;
            }

            RubyClass baseClass;

            if (type.IsByRef) {
                baseClass = _objectClass;
            } else {
                baseClass = GetOrCreateClassNoLock(type.BaseType);
            }

            TypeTracker tracker = (TypeTracker)TypeTracker.FromMemberInfo(type);
            RubyModule[] clrMixins = GetClrMixinsNoLock(type);
            RubyModule[] expandedMixins;

            if (clrMixins != null) {
                using (ClassHierarchyLocker()) {
                    expandedMixins = RubyModule.ExpandMixinsNoLock(baseClass, clrMixins);
                }
            } else {
                expandedMixins = RubyModule.EmptyArray;
            }

            result = CreateClass(
                GetQualifiedNameNoLock(type), type, null, null, null, null, null, 
                baseClass, expandedMixins, tracker, null, false, false, ModuleRestrictions.None
            );

            if (Utils.IsComObjectType(type)) {
                _comObjectClass = result;
            }

            _moduleCache[type] = result;
            return result;
        }

        /// <summary>
        /// An interface is mixed into the type that implements it.
        /// A generic type definition is mixed into its instantiations.
        /// 
        /// In both cases these modules don't contribute any callable CLR methods, 
        /// however Ruby methods defined on them can be called from the target type instances.
        /// </summary>
        private RubyModule[] GetClrMixinsNoLock(Type/*!*/ type) {
            List<RubyModule> modules = new List<RubyModule>();

            if (type.IsGenericType && !type.IsGenericTypeDefinition) {
                modules.Add(GetOrCreateModuleNoLock(type.GetGenericTypeDefinition()));
            }
            
            if (type.IsArray) {
                if (type.GetArrayRank() > 1) {
                    RubyModule module;
                    if (TryGetModuleNoLock(typeof(MultiDimensionalArray), out module)) {
                        modules.Add(module);
                    }
                }
            } else if (type.IsEnum) {
                if (type.IsDefined(typeof(FlagsAttribute), false)) {
                    RubyModule module;
                    if (TryGetModuleNoLock(typeof(FlagEnumeration), out module)) {
                        modules.Add(module);
                    }
                }
            }

            foreach (Type iface in ReflectionUtils.GetDeclaredInterfaces(type)) {
                modules.Add(GetOrCreateModuleNoLock(iface));
            }

            return modules.Count > 0 ? modules.ToArray() : null;
        }

        #endregion

        #region Class and Module Factories (thread-safe)

        internal RubyClass/*!*/ CreateClass(string name, Type type, object classSingletonOf,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, Delegate/*!*/[] factories,
            RubyClass/*!*/ superClass, RubyModule/*!*/[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo, 
            bool isRubyClass, bool isSingletonClass, ModuleRestrictions restrictions) {
            Assert.NotNull(superClass);

            RubyClass result = new RubyClass(this, name, type, classSingletonOf,
                instanceTrait, constantsInitializer, factories, superClass, expandedMixins, tracker, structInfo,
                isRubyClass, isSingletonClass, restrictions
            );

            result.InitializeDummySingletonClass(superClass.SingletonClass, classTrait);
            return result;
        }

        internal RubyModule/*!*/ CreateModule(string name,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[] expandedMixins, NamespaceTracker namespaceTracker, TypeTracker typeTracker, ModuleRestrictions restrictions) {

            RubyModule result = new RubyModule(
                this, name, instanceTrait, constantsInitializer, expandedMixins, namespaceTracker, typeTracker, restrictions
            );
            result.InitializeDummySingletonClass(_moduleClass, classTrait);
            return result;
        }

        /// <summary>
        /// Creates a singleton class for specified object unless it already exists. 
        /// </summary>
        public RubyClass/*!*/ CreateSingletonClass(object obj) {
            RubyModule module = obj as RubyModule;
            if (module != null) {
                return module.CreateSingletonClass();
            }

            return CreateInstanceSingleton(obj, null, null, null, null);
        }

        internal RubyClass/*!*/ CreateMainSingleton(object obj, RubyModule/*!*/[] expandedMixins) {
            return CreateInstanceSingleton(obj, _mainSingletonTrait, null, null, expandedMixins);
        }

        internal RubyClass/*!*/ CreateInstanceSingleton(object obj, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, 
            Action<RubyModule> constantsInitializer, RubyModule/*!*/[] expandedMixins) {
            Debug.Assert(!(obj is RubyModule));
            Debug.Assert(RubyUtils.CanCreateSingleton(obj));

            if (obj == null) {
                return _nilClass;
            }

            if (obj is bool) {
                return (bool)obj ? _trueClass : _falseClass;
            }

            RubyInstanceData data = null;
            RubyClass immediate = GetImmediateClassOf(obj, ref data);
            if (immediate.IsSingletonClass) {
                Debug.Assert(!immediate.IsDummySingletonClass);
                return immediate;
            }

            RubyClass result = CreateClass(
                null, null, obj, instanceTrait, classTrait ?? _classSingletonTrait, constantsInitializer, null,
                immediate, expandedMixins, null, null, true, true, ModuleRestrictions.None
            );

            using (ClassHierarchyLocker()) {
                // singleton might have been created by another thread:
                immediate = GetImmediateClassOf(obj, ref data);
                if (immediate.IsSingletonClass) {
                    Debug.Assert(!immediate.IsDummySingletonClass);
                    return immediate;
                }

                SetInstanceSingletonOfNoLock(obj, ref data, result);

                if (!(obj is IRubyObject)) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Non-IRO singleton created " + immediate.NominalClass.Name);
                }
            }

            Debug.Assert(result.IsSingletonClass && !result.IsDummySingletonClass);
            return result;
        }

        /// <summary>
        /// Defines a new module nested in the given owner.
        /// The module is published into the global scope if the owner is Object.
        /// 
        /// Thread safe.
        /// </summary>
        internal RubyModule/*!*/ DefineModule(RubyModule/*!*/ owner, string/*!*/ name) {
            RubyModule result = CreateModule(owner.MakeNestedModuleName(name), null, null, null, null, null, null, ModuleRestrictions.None);
            PublishModule(name, owner, result);
            return result;
        }

        /// <summary>
        /// Defines a new class nested in the given owner.
        /// The module is published into the global scope it if is not anonymous and the owner is Object.
        /// 
        /// Thread safe.
        /// Triggers "inherited" event.
        /// </summary>
        internal RubyClass/*!*/ DefineClass(RubyModule/*!*/ owner, string name, RubyClass/*!*/ superClass, RubyStruct.Info structInfo) {
            Assert.NotNull(owner, superClass);

            if (superClass.TypeTracker != null && superClass.TypeTracker.Type.ContainsGenericParameters) {
                throw RubyExceptions.CreateTypeError(String.Format(
                    "{0}: cannot inherit from open generic instantiation {1}. Only closed instantiations are supported.",
                    name, superClass.Name
                ));
            }

            string qualifiedName = owner.MakeNestedModuleName(name);
            RubyClass result = CreateClass(
                qualifiedName, null, null, null, null, null, null, superClass, null, null, structInfo, true, false, ModuleRestrictions.None
            );
            PublishModule(name, owner, result);
            superClass.ClassInheritedEvent(result);

            return result;
        }

        private static void PublishModule(string name, RubyModule/*!*/ owner, RubyModule/*!*/ module) {
            if (name != null) {
                owner.SetConstant(name, module);
                if (owner.IsObjectClass) {
                    module.Publish(name);
                }
            }
        }

        #endregion

        #region Libraries (thread-safe)

        internal RubyModule/*!*/ DefineLibraryModule(string name, Type/*!*/ type,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, ModuleRestrictions restrictions, bool builtin) {
            Assert.NotNull(type);
            Assert.NotNullItems(mixins);

            var expandedMixins = RubyModule.ExpandMixinsNoLock(null, mixins);

            RubyModule result;
            bool exists;
            lock (ModuleCacheLock) {
                if (!(exists = TryGetModuleNoLock(type, out result))) {
                    if (name == null) {
                        name = GetQualifiedNameNoLock(type);
                    }

                    // Use empty constant initializer rather than null so that we don't try to initialize nested types.
                    result = CreateModule(
                        name, instanceTrait, classTrait, constantsInitializer ?? RubyModule.EmptyInitializer, expandedMixins, null,
                        GetLibraryModuleTypeTracker(type, restrictions),
                        restrictions
                    );

                    AddModuleToCacheNoLock(type, result);
                }
            }

            if (exists) {
                result.IncludeLibraryModule(instanceTrait, classTrait, constantsInitializer, mixins, builtin);
            }

            return result;
        }

        // isSelfContained: The traits are defined on type (public static methods marked by RubyMethod attribute).
        internal RubyClass/*!*/ DefineLibraryClass(string name, Type/*!*/ type,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyClass super, RubyModule[]/*!*/ mixins, Delegate/*!*/[] factories, ModuleRestrictions restrictions, bool builtin) {
            Assert.NotNull(type);
            Assert.NotNullItems(mixins);

            RubyModule[] expandedMixins;
            using (ClassHierarchyLocker()) {
                expandedMixins = RubyModule.ExpandMixinsNoLock(super, mixins);
            }

            RubyClass result;
            bool exists;
            lock (ModuleCacheLock) {
                if (!(exists = TryGetClassNoLock(type, out result))) {
                    if (name == null) {
                        name = GetQualifiedNameNoLock(type);
                    }

                    if (super == null) {
                        super = GetOrCreateClassNoLock(type.BaseType);
                    }

                    // Use empty constant initializer rather than null so that we don't try to initialize nested types.
                    result = CreateClass(
                        name, type, null, instanceTrait, classTrait, constantsInitializer ?? RubyModule.EmptyInitializer, factories,
                        super, expandedMixins, GetLibraryModuleTypeTracker(type, restrictions), null, false, false,
                        restrictions
                    );

                    AddModuleToCacheNoLock(type, result);
                }
            }

            if (exists) {
                if (super != null && super != result.SuperClass) {
                    // TODO: better message
                    throw new InvalidOperationException("Cannot change super class");
                }

                if (factories != null && factories.Length != 0) {
                    throw new InvalidOperationException("Cannot add factories to an existing class");
                }

                result.IncludeLibraryModule(instanceTrait, classTrait, constantsInitializer, mixins, builtin);
                return result;
            } else if (!builtin) {
                super.ClassInheritedEvent(result);
            }

            return result;
        }

        private static TypeTracker GetLibraryModuleTypeTracker(Type/*!*/ type, ModuleRestrictions restrictions) {
            return (restrictions & ModuleRestrictions.NoUnderlyingType) != 0 ? null : ReflectionCache.GetTypeTracker(type);
        }

        #endregion

        #region Getting Modules and Classes from objects, CLR types and CLR namespaces (thread-safe)

        public RubyModule/*!*/ GetModule(Type/*!*/ type) {
            if (RubyModule.IsModuleType(type)) {
                return GetOrCreateModule(type);
            } else {
                return GetOrCreateClass(type);
            }
        }

        public RubyModule/*!*/ GetModule(NamespaceTracker/*!*/ namespaceTracker) {
            return GetOrCreateModule(namespaceTracker);
        }

        public RubyClass/*!*/ GetClass(Type/*!*/ type) {
            ContractUtils.Requires(!RubyModule.IsModuleType(type));
            return GetOrCreateClass(type);
        }

        /// <summary>
        /// Gets a class of the specified object (skips any singletons).
        /// </summary>
        public RubyClass/*!*/ GetClassOf(object obj) {
            ContractUtils.Ensures(!ContractUtils.Result<RubyClass>().IsSingletonClass);
            return TryGetClassOfRubyObject(obj) ?? GetOrCreateClass(obj.GetType());
        }

        private RubyClass TryGetClassOfRubyObject(object obj) {
            if (obj == null) {
                return _nilClass;
            }

            if (obj is bool) {
                return (bool)obj ? _trueClass : _falseClass;
            }

            IRubyObject rubyObj = obj as IRubyObject;
            if (rubyObj != null) {
                var result = rubyObj.ImmediateClass.GetNonSingletonClass();
                Debug.Assert(result != null, "Invalid IRubyObject implementation: Class should not be null");
                return result;
            }

            return null;
        }

        /// <summary>
        /// Gets a singleton or class for <c>obj</c>.
        /// Might return a class object from a foreign runtime (if obj is a runtime bound object).
        /// </summary>
        public RubyClass/*!*/ GetImmediateClassOf(object obj) {
            RubyInstanceData data = null;
            return GetImmediateClassOf(obj, ref data);
        }

        private RubyClass/*!*/ GetImmediateClassOf(object obj, ref RubyInstanceData data) {
            RubyClass result = TryGetImmediateClassOf(obj, ref data);
            if (result != null) {
                return result;
            }

            result = GetClassOf(obj);
            if (data != null) {
                data.UpdateImmediateClass(result);
            }

            return result;
        }

        // thread-safety:
        // If the immediate class reference is being changed (a singleton is being defined) during this operation
        // it is undefined which one of the classes we return. 
        private RubyClass TryGetImmediateClassOf(object obj, ref RubyInstanceData data) {
            IRubyObject rubyObj = obj as IRubyObject;
            if (rubyObj != null) {
                return rubyObj.ImmediateClass;
            } else if (data != null || (data = TryGetInstanceData(obj)) != null) {
                return data.ImmediateClass;
            } else {
                return null;
            }
        }

        // thread-safety: must only be run under a lock that prevents singleton creation on the target object:
        private void SetInstanceSingletonOfNoLock(object obj, ref RubyInstanceData data, RubyClass/*!*/ singleton) {
            RequiresClassHierarchyLock();
            Debug.Assert(!(obj is RubyModule) && singleton != null);

            IRubyObject rubyObj = obj as IRubyObject;
            if (rubyObj != null) {
                rubyObj.ImmediateClass = singleton;
            } else if (data != null) {
                data.ImmediateClass = singleton;
            } else {
                (data = GetInstanceData(obj)).ImmediateClass = singleton;
            }
        }

        internal RubyClass TryGetSingletonOf(object obj, ref RubyInstanceData data) {
            RubyClass immediate = TryGetImmediateClassOf(obj, ref data);
            return immediate != null ? (immediate.IsSingletonClass ? immediate : null) : null;
        }

        public bool IsKindOf(object obj, RubyModule/*!*/ m) {
            return GetImmediateClassOf(obj).HasAncestor(m);
        }

        public bool IsInstanceOf(object value, object classObject) {
            RubyClass c = classObject as RubyClass;
            if (c != null) {
                return GetClassOf(value).IsSubclassOf(c);
            }

            return false;
        }

        #endregion

        #region Module Names

        /// <summary>
        /// Gets the Ruby name of the class of the given object.
        /// </summary>
        public string/*!*/ GetClassName(object obj) {
            return GetClassName(obj, false);
        }

        /// <summary>
        /// Gets the display name of the class of the given object.
        /// Includes singleton names.
        /// </summary>
        public string/*!*/ GetClassDisplayName(object obj) {
            return GetClassName(obj, true);
        }

        private string/*!*/ GetClassName(object obj, bool display) {
            // doesn't create a RubyClass for .NET types

            RubyClass cls = TryGetClassOfRubyObject(obj);
            if (cls != null) {
                return cls.Name;
            }

            return GetTypeName(obj.GetType(), display);
        }

        public string/*!*/ GetTypeName(Type/*!*/ type, bool display) {
            RubyModule module;
            lock (ModuleCacheLock) {
                if (TryGetModuleNoLock(type, out module)) {
                    if (display) {
                        return module.GetDisplayName(this, false).ToString();
                    } else {
                        return module.Name;
                    }
                } else {
                    return GetQualifiedNameNoLock(type);
                }
            }
        }

        private string/*!*/ GetQualifiedNameNoLock(Type/*!*/ type) {
            return GetQualifiedNameNoLock(type, this, false);
        }

        internal static string/*!*/ GetQualifiedNameNoLock(Type/*!*/ type, RubyContext context, bool noGenericArgs) {
            return AppendQualifiedNameNoLock(new StringBuilder(), type, context, noGenericArgs).ToString();
        }

        private static StringBuilder/*!*/ AppendQualifiedNameNoLock(StringBuilder/*!*/ result, Type/*!*/ type, RubyContext context, bool noGenericArgs) {
            if (type.IsGenericParameter) {
                return result.Append(type.Name);
            }

            // arrays, by-refs, pointers:
            Type elementType = type.GetElementType();
            if (elementType != null) {
                AppendQualifiedNameNoLock(result, elementType, context, noGenericArgs);
                if (type.IsByRef) {
                    result.Append('&');
                } else if (type.IsArray) {
                    result.Append('[');
                    result.Append(',', type.GetArrayRank() - 1);
                    result.Append(']');
                } else {
                    Debug.Assert(type.IsPointer);
                    result.Append('*');
                }
                return result;
            }
            
            // qualifiers:
            if (type.DeclaringType != null) {
                AppendQualifiedNameNoLock(result, type.DeclaringType, context, noGenericArgs);
                result.Append("::");
            } else if (type.Namespace != null) {
                result.Append(type.Namespace.Replace(Type.Delimiter.ToString(), "::"));
                result.Append("::");
            }

            result.Append(ReflectionUtils.GetNormalizedTypeName(type));

            // generic args:
            if (!noGenericArgs && type.IsGenericType) {
                result.Append("[");

                var genericArgs = type.GetGenericArguments();
                for (int i = 0; i < genericArgs.Length; i++) {
                    if (i > 0) {
                        result.Append(", ");
                    }
                    
                    RubyModule module;
                    if (context != null && context.TryGetModuleNoLock(genericArgs[i], out module)) {
                        result.Append(module.Name);
                    } else {
                        AppendQualifiedNameNoLock(result, genericArgs[i], context, noGenericArgs);
                    }
                }

                result.Append("]");
            }

            return result;
        }

        private static string/*!*/ GetQualifiedName(NamespaceTracker/*!*/ namespaceTracker) {
            ContractUtils.RequiresNotNull(namespaceTracker, "namespaceTracker");
            if (namespaceTracker.Name == null) return String.Empty;

            return namespaceTracker.Name.Replace(Type.Delimiter.ToString(), "::");
        }

        #endregion

        #region Member Resolution (thread-safe)

        // thread-safe:
        public MethodResolutionResult ResolveMethod(object target, string/*!*/ name, bool includePrivate) {
            var owner = GetImmediateClassOf(target);
            return owner.ResolveMethod(name, includePrivate ? VisibilityContext.AllVisible : new VisibilityContext(owner));
        }

        // thread-safe:
        public MethodResolutionResult ResolveMethod(object target, string/*!*/ name, VisibilityContext visibility) {
            return GetImmediateClassOf(target).ResolveMethod(name, visibility);
        }

        // thread-safe:
        public bool TryGetModule(RubyGlobalScope autoloadScope, string/*!*/ moduleName, out RubyModule result) {
            using (ClassHierarchyLocker()) {
                result = _objectClass;
                int pos = 0;
                while (true) {
                    int pos2 = moduleName.IndexOf("::", pos, StringComparison.Ordinal);
                    string partialName;
                    if (pos2 < 0) {
                        partialName = moduleName.Substring(pos);
                    } else {
                        partialName = moduleName.Substring(pos, pos2 - pos);
                        pos = pos2 + 2;
                    }
                    ConstantStorage tmp;
                    if (!result.TryResolveConstantNoLock(autoloadScope, partialName, out tmp)) {
                        result = null;
                        return false;
                    }
                    result = tmp.Value as RubyModule;
                    if (result == null) {
                        return false;
                    } else if (pos2 < 0) {
                        return true;
                    }
                }
            }
        }

        // thread-safe:
        public object ResolveMissingConstant(RubyModule/*!*/ owner, string/*!*/ name) {
            if (owner.IsObjectClass) {
                object value;
                if (RubyOps.TryGetGlobalScopeConstant(this, _globalScope, name, out value)) {
                    return value;
                }

                if ((value = _namespaces.TryGetPackageAny(name)) != null) {
                    return TrackerToModule(value);
                }
            }

            throw RubyExceptions.CreateNameError(String.Format("uninitialized constant {0}::{1}", owner.Name, name));
        }

        // thread-safe:
        internal object TrackerToModule(object value) {
            TypeGroup typeGroup = value as TypeGroup;
            if (typeGroup != null) {
                return value;
            }

            // TypeTracker retrieved from namespace tracker should behave like a RubyClass/RubyModule:
            TypeTracker typeTracker = value as TypeTracker;
            if (typeTracker != null) {
                return GetModule(typeTracker.Type);
            }

            // NamespaceTracker retrieved from namespace tracker should behave like a RubyModule:
            NamespaceTracker namespaceTracker = value as NamespaceTracker;
            if (namespaceTracker != null) {
                return GetModule(namespaceTracker);
            }

            return value;
        }

        #endregion

        #region Object Operations: InstanceData access (thread-safe)

        // Retrieving instance data is thread safe. Operations on the instance data object are not.

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
                lock (ValueTypeInstanceDataLock) {
                    _valueTypeInstanceData.TryGetValue(obj, out result);
                }
                return result;
            }

            TryGetClrTypeInstanceData(obj, out result);
            return result;
        }

        internal bool TryGetClrTypeInstanceData(object/*!*/ obj, out RubyInstanceData result) {
            lock (ReferenceTypeInstanceDataLock) {
                return _referenceTypeInstanceData.TryGetValue(obj, out result);
            }
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
                lock (ValueTypeInstanceDataLock) {
                    if (!_valueTypeInstanceData.TryGetValue(obj, out result)) {
                        _valueTypeInstanceData.Add(obj, result = new RubyInstanceData());
                    }
                }
                return result;
            }

            lock (ReferenceTypeInstanceDataLock) {
                if (!_referenceTypeInstanceData.TryGetValue(obj, out result)) {
                    _referenceTypeInstanceData.Add(obj, result = new RubyInstanceData());
                }
            }
            
            return result;
        }

        internal DynamicOperations Operations {
            get {
                if (_operations == null) {
                    Interlocked.CompareExchange(ref _operations, new DynamicOperations(this), null);
                }

                return _operations;
            }
        }

        #endregion

        #region Object Operations: Instance variables, flags (NOT thread-safe)

        public bool HasInstanceVariables(object obj) {
            RubyInstanceData data = TryGetInstanceData(obj);
            return data != null && data.HasInstanceVariables;
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

        private RubyInstanceData MutateInstanceVariables(object obj) {
            RubyInstanceData data;
            if (IsObjectFrozen(obj, out data)) {
                throw RubyExceptions.CreateObjectFrozenError();
            }
            return data;
        }

        public void SetInstanceVariable(object obj, string/*!*/ name, object value) {
            (MutateInstanceVariables(obj) ?? GetInstanceData(obj)).SetInstanceVariable(name, value);
        }

        public bool TryRemoveInstanceVariable(object obj, string/*!*/ name, out object value) {
            RubyInstanceData data = MutateInstanceVariables(obj) ?? TryGetInstanceData(obj);
            if (data == null || !data.TryRemoveInstanceVariable(name, out value)) {
                value = null;
                return false;
            }
            return true;
        }

        //
        // Thread safety: target object must be a fresh object not be shared with other threads:
        // 
        // Copies instance variables from source to target object.
        // If the source has a singleton class it's members are copied to the target as well.
        // Assumes a fresh instance of target, with no instance data.
        //
        internal void CopyInstanceData(object source, object target, bool copySingletonMembers) {
            RubyInstanceData targetData = null;
            Debug.Assert(!copySingletonMembers || !(source is RubyModule));
            Debug.Assert(TryGetInstanceData(target) == null);
            // target object is not a singleton:
            Debug.Assert(!copySingletonMembers || TryGetSingletonOf(target, ref targetData) == null && targetData == null);
            
            RubyInstanceData sourceData = TryGetInstanceData(source);
            if (sourceData != null) {
                if (sourceData.HasInstanceVariables) {
                    sourceData.CopyInstanceVariablesTo(targetData = GetInstanceData(target));
                }
            }

            if (copySingletonMembers) {
                using (ClassHierarchyLocker()) {
                    RubyClass singleton = TryGetSingletonOf(source, ref sourceData);
                    if (singleton != null) {
                        var singletonDup = singleton.Duplicate(target);
                        singletonDup.InitializeMembersFrom(singleton);

                        SetInstanceSingletonOfNoLock(target, ref targetData, singletonDup);
                    }
                }
            }
        }

        public bool IsObjectFrozen(object obj) {
            RubyInstanceData data;
            return IsObjectFrozen(obj, out data);
        }

        private bool IsObjectFrozen(object obj, out RubyInstanceData data) {
            var state = obj as IRubyObjectState;
            if (state != null) {
                data = null;
                return state.IsFrozen;
            }

            data = TryGetInstanceData(obj);
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

        #region Dynamic Object Operations (thread-safe)

        /// <summary>
        /// Calls "inspect" and converts its result to a string using "to_s" protocol (<see cref="ConvertToSAction"/>).
        /// </summary>
        public MutableString/*!*/ Inspect(object obj) {
            RubyClass cls = GetClassOf(obj);
            var inspect = cls.InspectSite;
            var toS = cls.InspectResultConversionSite;
            return toS.Target(toS, inspect.Target(inspect, obj));
        }

        #endregion

        #region Global Variables: General access (thread-safe)

        public object GetGlobalVariable(string/*!*/ name) {
            object value;
            TryGetGlobalVariable(null, name, out value);
            return value;
        }

        public void DefineGlobalVariable(string/*!*/ name, object value) {
            lock (GlobalVariablesLock) {
                _globalVariables[name] = new GlobalVariableInfo(value);
            }
        }

        public void DefineReadOnlyGlobalVariable(string/*!*/ name, object value) {
            lock (GlobalVariablesLock) {
                _globalVariables[name] = new ReadOnlyGlobalVariableInfo(value);
            }
        }

        public void DefineGlobalVariable(string/*!*/ name, GlobalVariable/*!*/ variable) {
            ContractUtils.RequiresNotNull(variable, "variable");
            lock (GlobalVariablesLock) {
                _globalVariables[name] = variable;
            }
        }

        internal void DefineGlobalVariableNoLock(string/*!*/ name, GlobalVariable/*!*/ variable) {
            _globalVariables[name] = variable;
        }

        public bool DeleteGlobalVariable(string/*!*/ name) {
            lock (GlobalVariablesLock) {
                return _globalVariables.Remove(name);
            }
        }

        public void AliasGlobalVariable(string/*!*/ newName, string/*!*/ oldName) {
            lock (GlobalVariablesLock) {
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
            lock (GlobalVariablesLock) {
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
            lock (GlobalVariablesLock) {
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
            lock (GlobalVariablesLock) {
                return _globalVariables.TryGetValue(name, out variable);
            }
        }

        #endregion

        #region Global Variables: Special variables (thread-safe)

        /// <summary>
        /// $!
        /// </summary>
        [ThreadStatic]
        private static Exception _currentException;

        public Exception CurrentException {
            get { return _currentException; }
            internal set { _currentException = RubyUtils.GetVisibleException(value); }
        }

        internal Exception SetCurrentException(object value) {
            Exception e = value as Exception;

            // "$! = nil" is allowed
            if (value != null && e == null) {
                throw RubyExceptions.CreateTypeError("assigning non-exception to $!");
            }

            Debug.Assert(RubyUtils.GetVisibleException(e) == e);
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

        /// <summary>
        /// $KCODE
        /// </summary>
        public RubyEncoding KCode { get; internal set; }
        
        /// <summary>
        /// $SAFE
        /// </summary>
        [ThreadStatic]
        private static int _currentSafeLevel;

        public int CurrentSafeLevel {
            get { return _currentSafeLevel; }
        }

        public void SetSafeLevel(int value) {
            if (_currentSafeLevel <= value) {
                _currentSafeLevel = value;
            } else {
                throw RubyExceptions.CreateSecurityError(String.Format("tried to downgrade safe level from {0} to {1}",
                    _currentSafeLevel, value));
            }
        }

        #endregion

        #region IO (thread-safe)

        private sealed class FileDescriptor {
            public int DuplicateCount;
            public readonly Stream/*!*/ Stream;

            public FileDescriptor(Stream/*!*/ stream) {
                Assert.NotNull(stream);
                Stream = stream;
                DuplicateCount = 1;
            }

            public void Close() {
                DuplicateCount--;
                if (DuplicateCount == 0) {
                    Stream.Close();
                }
            }
        }

        private readonly List<FileDescriptor>/*!*/ _fileDescriptors = new List<FileDescriptor>(10);

        public const int StandardInputDescriptor = 0;
        public const int StandardOutputDescriptor = 1;
        public const int StandardErrorOutputDescriptor = 2;

        public object StandardInput { get; set; }
        public object StandardOutput { get; set; }
        public object StandardErrorOutput { get; set; }

        private FileDescriptor TryGetFileDescriptorNoLock(int descriptor) {
            return (descriptor < 0 || descriptor >= _fileDescriptors.Count) ? null : _fileDescriptors[descriptor];
        }

        private int AddFileDescriptorNoLock(FileDescriptor/*!*/ fd) {
            for (int i = 0; i < _fileDescriptors.Count; i++) {
                if (_fileDescriptors[i] == null) {
                    _fileDescriptors[i] = fd;
                    return i;
                }
            }
            _fileDescriptors.Add(fd);
            return _fileDescriptors.Count - 1;
        }

        public Stream GetStream(int descriptor) {
            lock (_fileDescriptors) {
                var fd = TryGetFileDescriptorNoLock(descriptor);
                return (fd != null) ? fd.Stream : null;
            }
        }

        public void SetStream(int descriptor, Stream/*!*/ stream) {
            ContractUtils.RequiresNotNull(stream, "stream");

            lock (_fileDescriptors) {
                var fd = TryGetFileDescriptorNoLock(descriptor);
                if (fd == null) {
                    throw RubyExceptions.CreateEBADF();
                }
                if (fd.Stream != stream) {
                    fd.Close();
                    _fileDescriptors[descriptor] = new FileDescriptor(stream);
                }
            }
        }

        public void RedirectFileDescriptor(int descriptor, int toDescriptor) {
            lock (_fileDescriptors) {
                var fd = TryGetFileDescriptorNoLock(descriptor);
                if (fd == null) {
                    throw RubyExceptions.CreateEBADF();
                }

                var toFd = TryGetFileDescriptorNoLock(toDescriptor);
                if (toFd == null) {
                    throw RubyExceptions.CreateEBADF();
                }

                if (fd == toFd) {
                    return;
                }

                fd.Close();
                toFd.DuplicateCount++;
                _fileDescriptors[descriptor] = toFd;
            }
        }

        public int AllocateFileDescriptor(Stream/*!*/ stream) {
            ContractUtils.RequiresNotNull(stream, "stream");
            lock (_fileDescriptors) {
                return AddFileDescriptorNoLock(new FileDescriptor(stream));
            }
        }

        public int DuplicateFileDescriptor(int descriptor) {
            lock (_fileDescriptors) {
                var fd = TryGetFileDescriptorNoLock(descriptor);
                if (fd == null) {
                    throw RubyExceptions.CreateEBADF();
                }
                fd.DuplicateCount++;
                return AddFileDescriptorNoLock(fd);
            }
        }

        public void CloseStream(int descriptor) {
            lock (_fileDescriptors) {
                var fd = TryGetFileDescriptorNoLock(descriptor);
                if (fd == null) {
                    throw RubyExceptions.CreateEBADF();
                }
                fd.Close();
                _fileDescriptors[descriptor] = null;
            }
        }

        public void RemoveFileDescriptor(int descriptor) {
            lock (_fileDescriptors) {
                if (TryGetFileDescriptorNoLock(descriptor) == null) {
                    throw RubyExceptions.CreateEBADF();
                }

                _fileDescriptors[descriptor] = null;
            }
        }

        private readonly RuntimeErrorSink/*!*/ _runtimeErrorSink;

        public RuntimeErrorSink/*!*/ RuntimeErrorSink {
            get { return _runtimeErrorSink; }
        }

        public void ReportWarning(string/*!*/ message) {
            ReportWarning(message, false);
        }

        public void ReportWarning(string/*!*/ message, bool isVerbose) {
            _runtimeErrorSink.Add(null, message, SourceSpan.None, isVerbose ? Errors.RuntimeVerboseWarning : Errors.RuntimeWarning, Severity.Warning);
        }

        public RubyEncoding/*!*/ GetPathEncoding() {
            return KCode ?? 
                // we need to force UTF8 encoding since the path can contain non-ascii characters:
                (_options.Compatibility == RubyCompatibility.Ruby18 ? RubyEncoding.KCodeUTF8 : RubyEncoding.UTF8);
        }

        /// <summary>
        /// Creates a mutable string encoded using the current KCODE or (K-)UTF8 if KCODE is not set.
        /// </summary>
        /// <exception cref="EncoderFallbackException">Invalid characters present.</exception>
        public MutableString/*!*/ EncodePath(string/*!*/ path) {
            return EncodePath(path, GetPathEncoding());
        }

        public MutableString TryEncodePath(string/*!*/ path) {
            try {
                return MutableString.Create(path, GetPathEncoding()).CheckEncoding();
            } catch (EncoderFallbackException) {
                return null;
            }
        }

        internal static MutableString/*!*/ EncodePath(string/*!*/ path, RubyEncoding/*!*/ encoding) {
            try {
                return MutableString.Create(path, encoding).CheckEncoding();
            } catch (EncoderFallbackException e) {
                throw RubyExceptions.CreateEINVAL(
                    e,
                    "Path \"{0}\" contains characters that cannot be represented in encoding {1}: {2}",
                    path.ToAsciiString(),
                    encoding.RealEncoding.Name,
                    e.Message
                );
            }
        }

        /// <summary>
        /// Transcodes given mutable string to Unicode path that can be passed to the .NET IO system (or host).
        /// </summary>
        /// <exception cref="InvalidError">Invalid characters present.</exception>
        public string/*!*/ DecodePath(MutableString/*!*/ path) {
            try {
                if (KCode != null) {
                    return path.ToString(KCode.StrictEncoding);
                } else if (path.Encoding.IsKCoding || path.IsBinaryEncoded) {
                    // force UTF8 encoding to make round-trip work:
                    return path.ToString(Encoding.UTF8);
                } else {
                    return path.ConvertToString();
                }
            } catch (DecoderFallbackException) {
                throw RubyExceptions.CreateEINVAL("Invalid multi-byte sequence in path `{0}'", path.ToAsciiString());
            }
        }

        #endregion

        #region Library Data (thread-safe)

        private Dictionary<object, object> _libraryData;

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

        public void TrySetLibraryData(object key, object value) {
            EnsureLibraryData();

            lock (_libraryData) {
                _libraryData[key] = value;
            }
        }

        #endregion

        #region Parsing, Compilation (thread-safe)

        public override ScriptCode CompileSourceCode(SourceUnit/*!*/ sourceUnit, CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
            ContractUtils.RequiresNotNull(options, "options");
            ContractUtils.RequiresNotNull(errorSink, "errorSink");
            ContractUtils.Requires(sourceUnit.LanguageContext == this, "Language mismatch.");

#if DEBUG
            if (RubyOptions.LoadFromDisk) {
                string code;
                Utils.Log(String.Format("compiling {0}", sourceUnit.Path ??
                    ((code = sourceUnit.GetCode()).Length < 100 ? code : code.Substring(0, 100))
                    .Replace('\r', ' ').Replace('\n', ' ')
                ), "COMPILER");
            }
#endif
            var rubyOptions = (RubyCompilerOptions)options;

            var lambda = ParseSourceCode<Func<RubyScope, object, object>>(sourceUnit, rubyOptions, errorSink);
            if (lambda == null) {
                return null;
            }

            return new RubyScriptCode(lambda, sourceUnit, rubyOptions.FactoryKind);
        }

#if MEASURE_AST
        private static readonly object _TransformationLock = new object();
        private static readonly Dictionary<ExpressionType, int> _TransformationHistogram = new Dictionary<ExpressionType,int>();
#endif

        private static long _ParseTimeTicks;
        private static long _AstGenerationTimeTicks;

        internal MSA.Expression<T> ParseSourceCode<T>(SourceUnit/*!*/ sourceUnit, RubyCompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            Debug.Assert(sourceUnit.LanguageContext == this);

            long ts1, ts2;

            ts1 = Stopwatch.GetTimestamp();
            SourceUnitTree ast = new Parser().Parse(sourceUnit, options, errorSink);
            ts2 = Stopwatch.GetTimestamp();

            Interlocked.Add(ref _ParseTimeTicks, ts2 - ts1);

            if (ast == null) {
                return null;
            }

            MSA.Expression<T> lambda;
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

        internal MSA.Expression<T>/*!*/ TransformTree<T>(SourceUnitTree/*!*/ ast, SourceUnit/*!*/ sourceUnit, RubyCompilerOptions/*!*/ options) {
            return ast.Transform<T>(
                new AstGenerator(
                    this,
                    options,
                    sourceUnit.Document,
                    ast.Encoding,
                    sourceUnit.Kind == SourceCodeKind.InteractiveCode
                )
            );
        }

        public override CompilerOptions/*!*/ GetCompilerOptions() {
            return new RubyCompilerOptions(_options) {
                FactoryKind = TopScopeFactoryKind.Hosted,
            };
        }

        public override CompilerOptions/*!*/ GetCompilerOptions(Scope/*!*/ scope) {
            var result = new RubyCompilerOptions(_options) {
                FactoryKind = TopScopeFactoryKind.Hosted
            };

            var rubyGlobalScope = (RubyGlobalScope)scope.GetExtension(ContextId);
            if (rubyGlobalScope != null && rubyGlobalScope.TopLocalScope != null) {
                result.LocalNames = rubyGlobalScope.TopLocalScope.GetVisibleLocalNames();
            }

            return result;
        }

        public override ErrorSink GetCompilerErrorSink() {
            return _runtimeErrorSink;
        }

        public override ScriptCode/*!*/ LoadCompiledCode(Delegate/*!*/ method, string path, string customData) {
            // TODO: we need to save the kind of the scope factory:
            SourceUnit su = new SourceUnit(this, NullTextContentProvider.Null, path, SourceCodeKind.File);
            return new RubyScriptCode((Func<RubyScope, object, object>)method, su, TopScopeFactoryKind.Hosted);
        }

        public void CheckConstantName(string name) {
            if (!Tokenizer.IsConstantName(name, _options.Compatibility >= RubyCompatibility.Ruby19 || KCode != null)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as a constant name", name));
            }
        }

        public void CheckClassVariableName(string name) {
            if (!Tokenizer.IsClassVariableName(name, _options.Compatibility >= RubyCompatibility.Ruby19 || KCode != null)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as a class variable name", name));
            }
        }

        public void CheckInstanceVariableName(string name) {
            if (!Tokenizer.IsInstanceVariableName(name, _options.Compatibility >= RubyCompatibility.Ruby19 || KCode != null)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as an instance variable name", name));
            }
        }

        #endregion

        #region Global Scope (thread-safe)

        /// <summary>
        /// Creates a scope extension for a DLR scope unless it already exists for the given scope.
        /// </summary>
        internal RubyGlobalScope/*!*/ InitializeGlobalScope(Scope/*!*/ globalScope, bool createHosted, bool bindGlobals) {
            Assert.NotNull(globalScope);

            var scopeExtension = globalScope.GetExtension(ContextId);
            if (scopeExtension != null) {
                return (RubyGlobalScope)scopeExtension;
            }

            RubyObject mainObject = new RubyObject(_objectClass);
            RubyClass mainSingleton = CreateMainSingleton(mainObject, null);

            RubyGlobalScope result = new RubyGlobalScope(this, globalScope, mainObject, createHosted);
            if (bindGlobals) {
                // method_missing:
                mainSingleton.SetMethodNoEvent(this, Symbols.MethodMissing, 
                    new RubyLibraryMethodInfo(new Delegate[] {
                        new Func<RubyScope, BlockParam, object, SymbolId, object[], object>(RubyTopLevelScope.TopMethodMissing)
                    }, RubyMemberFlags.Private, mainSingleton)
                );

                mainSingleton.SetGlobalScope(result);
            }
            return (RubyGlobalScope)globalScope.SetExtension(ContextId, result);
        }

        public override int ExecuteProgram(SourceUnit/*!*/ program) {
            try {
                RubyCompilerOptions options = new RubyCompilerOptions(_options) {
                    FactoryKind = TopScopeFactoryKind.Main
                };

                CompileSourceCode(program, options, _runtimeErrorSink).Run();
            } catch (SystemExit e) {
                return e.Status;
            }

            return 0;
        }

        #endregion

        #region Shutdown (thread-safe)

        private readonly List<BlockParam> _shutdownHandlers = new List<BlockParam>();
        private object ShutdownHandlersLock { get { return _shutdownHandlers; }}

        public void RegisterShutdownHandler(BlockParam/*!*/ proc) {
            ContractUtils.RequiresNotNull(proc, "proc");

            lock (ShutdownHandlersLock) {
                _shutdownHandlers.Add(proc);
            }
        }

        private void ExecuteShutdownHandlers() {
            var handlers = new List<BlockParam>();

            SystemExit lastSystemExit = null;
            Exception lastUncaughtException = null;

            while (true) {
                lock (ShutdownHandlersLock) {
                    if (_shutdownHandlers.Count == 0) {
                        break;
                    }
                    handlers.AddRange(_shutdownHandlers);
                    _shutdownHandlers.Clear();
                }

                for (int i = handlers.Count - 1; i >= 0; --i) {
                    try {
                        object result;
                        handlers[i].Yield(out result);
                    } catch (SystemExit e) {
                        // Kernel#at_exit can call exit and set the exitcode. Furthermore, exit can be called 
                        // from multiple blocks registered with Kernel#at_exit.
                        lastSystemExit = e;
                    } catch (Exception e) {
			            CurrentException = e;
                        lastUncaughtException = e;
                    }
                }

                handlers.Clear();
            }

            if (lastSystemExit != null) {
                throw lastSystemExit;
            }

            if (lastUncaughtException != null) {
                throw lastUncaughtException;
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
                        string methodInfo = counter.Id;
                        if (methodInfo.Length > maxLength) {
                            maxLength = methodInfo.Length;
                        }

                        totalTicks += counter.Ticks;

                        keys[i] = methodInfo;
                        values[i] = counter.Ticks;
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
                using (TextWriter output = File.CreateText("perfstats.log")) {
                    output.WriteLine(String.Format(@"
  total:         {0}
  parse:         {1}
  ast transform: {2}
  script code:   {3}
  il:            {4} (TODO)
  binding:       {5} ({6} calls)
",
                        _upTime.Elapsed,
                        new TimeSpan(_ParseTimeTicks),
                        new TimeSpan(_AstGenerationTimeTicks),
                        new TimeSpan(Loader._ScriptCodeGenerationTimeTicks),
                        new TimeSpan(), // TODO: new TimeSpan(Loader._ILGenerationTimeTicks),
#if MEASURE
                    new TimeSpan(MetaAction.BindingTimeTicks), 
                    MetaAction.BindCallCount
#else
     "N/A", "N/A"
#endif
));

#if MEASURE_BINDING
                    output.WriteLine();
                    output.WriteLine("---- MetaAction kinds ----");
                    output.WriteLine();

                    PerfTrack.DumpHistogram(MetaAction.HistogramOfKinds, output);

                    output.WriteLine();

                    output.WriteLine();
                    output.WriteLine("---- MetaAction instances ----");
                    output.WriteLine();

                    PerfTrack.DumpHistogram(MetaAction.HistogramOfInstances, output);

                    output.WriteLine();
#endif

#if MEASURE_AST
                    output.WriteLine();
                    output.WriteLine("---- Ruby Parser generated Expression Trees ----");
                    output.WriteLine();
                    
                    PerfTrack.DumpHistogram(_TransformationHistogram, output);

                    output.WriteLine();
#endif
                    PerfTrack.DumpStats(output);
                }
            }
#endif
            _loader.SaveCompiledCode();

            ExecuteShutdownHandlers();
        }

        #endregion

        #region Exceptions (thread-safe)

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

        public override SourceCodeReader/*!*/ GetSourceReader(Stream/*!*/ stream, Encoding/*!*/ defaultEncoding, string path) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
            ContractUtils.Requires(stream.CanRead && stream.CanSeek, "stream", "The stream must support seeking and reading");

#if SILVERLIGHT
            return base.GetSourceReader(stream, defaultEncoding, path);
#else
            return GetSourceReader(stream, defaultEncoding, _options.Compatibility);
        }

        private static SourceCodeReader/*!*/ GetSourceReader(Stream/*!*/ stream, Encoding/*!*/ defaultEncoding, RubyCompatibility compatibility) {
            if (compatibility <= RubyCompatibility.Ruby18) {
                return new SourceCodeReader(new StreamReader(stream, defaultEncoding, false), defaultEncoding);
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

        public override string/*!*/ FormatObject(DynamicOperations/*!*/ operations, object obj) {
            var inspectSite = operations.GetOrCreateSite<object, object>(
                RubyCallAction.Make(this, "inspect", RubyCallSignature.WithImplicitSelf(1))
            );

            var tosSite = operations.GetOrCreateSite<object, MutableString>(ConvertToSAction.Make(this));

            return tosSite.Target(tosSite, inspectSite.Target(inspectSite, obj)).ToString();
        }

        #endregion

        #region MetaObject binding

        public override GetMemberBinder/*!*/ CreateGetMemberBinder(string/*!*/ name, bool ignoreCase) {
            // TODO:
            if (ignoreCase) {
                return base.CreateGetMemberBinder(name, ignoreCase);
            }
            return new InteropBinder.GetMember(this, name);
        }

        public override InvokeMemberBinder/*!*/ CreateCallBinder(string/*!*/ name, bool ignoreCase, CallInfo/*!*/ callInfo) {
            // TODO:
            if (ignoreCase || callInfo.ArgumentNames.Count != 0) {
                return base.CreateCallBinder(name, ignoreCase, callInfo);
            }
            return new InteropBinder.InvokeMember(this, name, callInfo);
        }

        public override CreateInstanceBinder/*!*/ CreateCreateBinder(CallInfo /*!*/ callInfo) {
            // TODO:
            if (callInfo.ArgumentNames.Count != 0) {
                return base.CreateCreateBinder(callInfo);
            }

            return new InteropBinder.CreateInstance(this, callInfo);
        }

        public override ConvertBinder/*!*/ CreateConvertBinder(Type toType, bool? explicitCast) {
            return new InteropBinder.Convert(this, toType, explicitCast ?? true);
        }

        // TODO: override GetMemberNames?
        public IList<string>/*!*/ GetForeignDynamicMemberNames(object obj) {
            if (obj is IRubyDynamicMetaObjectProvider) {
                return ArrayUtils.EmptyStrings;
            }
#if !SILVERLIGHT
            if (Utils.IsComObject(obj)) {
                return new List<string>(Microsoft.Scripting.ComInterop.ComBinder.GetDynamicMemberNames(obj));
            }
#endif
            return GetMemberNames(obj);
        }

        #endregion

        #region Dynamic Sites (thread-safe)

        private CallSite<Func<CallSite, object, MutableString>> _stringConversionSite;

        public CallSite<Func<CallSite, object, MutableString>>/*!*/ StringConversionSite {
            get { return RubyUtils.GetCallSite(ref _stringConversionSite, ConvertToSAction.Make(this)); }
        }

        private readonly Dictionary<Key<string, RubyCallSignature>, CallSite>/*!*/ _sendSites =
            new Dictionary<Key<string, RubyCallSignature>, CallSite>();

        private object SendSitesLock { get { return _sendSites; } }

        public CallSite<TSiteFunc>/*!*/ GetOrCreateSendSite<TSiteFunc>(string/*!*/ methodName, RubyCallSignature callSignature)
            where TSiteFunc : class {

            lock (SendSitesLock) {
                CallSite site;
                if (_sendSites.TryGetValue(Key.Create(methodName, callSignature), out site)) {
                    return (CallSite<TSiteFunc>)site;
                }

                var newSite = CallSite<TSiteFunc>.Create(RubyCallAction.Make(this, methodName, callSignature));
                _sendSites.Add(Key.Create(methodName, callSignature), newSite);
                return newSite;
            }
        }

        public DynamicDelegateCreator/*!*/ DelegateCreator {
            get {
                if (_delegateCreator == null) {
                    Interlocked.CompareExchange(ref _delegateCreator, new DynamicDelegateCreator(this), null);
                }

                return _delegateCreator;
            }
        }

        #endregion

        #region Ruby Events

        private CallSite<Func<CallSite, object, object, object>> _respondTo;

        internal object Send(ref CallSite<Func<CallSite, object, object, object>> site, string/*!*/ eventName,
            object target, string/*!*/ memberName) {

            if (site == null) {
                Interlocked.CompareExchange(
                    ref site,
                    CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(this, eventName, RubyCallSignature.WithImplicitSelf(1))),
                    null
                );
            }

            return site.Target(site, target, SymbolTable.StringToId(memberName));
        }

        public bool RespondTo(object target, string/*!*/ methodName) {
            return RubyOps.IsTrue(Send(ref _respondTo, "respond_to?", target, methodName));
        }

        internal void ReportTraceEvent(string/*!*/ operation, RubyScope/*!*/ scope, RubyModule/*!*/ module, string/*!*/ name, string fileName, int lineNumber) {
            if (_traceListener != null && !_traceListenerSuspended) {

                try {
                    _traceListenerSuspended = true;

                    _traceListener.Call(new[] {
                        MutableString.CreateAscii(operation),                                         // event
                        fileName != null ? scope.RubyContext.EncodePath(fileName) : null,             // file
                        ScriptingRuntimeHelpers.Int32ToObject(lineNumber),                            // line
                        SymbolTable.StringToId(name),                                                 // TODO: alias
                        new Binding(scope),                                                           // binding
                        module.IsSingletonClass ? ((RubyClass)module).SingletonClassOf : module       // module
                    });
                } finally {
                    _traceListenerSuspended = false;
                }
            }
        }

        #endregion
    }
}
