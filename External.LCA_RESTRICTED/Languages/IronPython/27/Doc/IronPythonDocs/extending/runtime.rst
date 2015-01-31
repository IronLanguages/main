.. highlightlang:: c


.. hosting-runtime:

*************
ScriptRuntime
*************

This class is the starting point for hosting.  For Level One scenarios, you just create ScriptScopes, use Globals, and use ExecuteFile.  For Level Two scenarios, you can get to ScriptEngines and so on.

ScriptRuntime represents global script state.  This includes referenced assemblies, a "global object" (ScriptRuntime.Globals), "published" scopes (scopes bound to a name on Globals), available language engines, etc.

ScriptRuntime has a single constructor and two convenience factory methods.  You can create multiple instances of a ScriptRuntime in a single AppDomain.  For information on configuring a ScriptRuntime for what languages it allows, global settings, language settings, etc., see ScriptRuntimeSetup.

ScriptRuntime Overview::

    public class ScriptRuntime : MarshalByRefObject {
        public ScriptRuntime(ScriptRuntimeSetup setup)
        public static ScriptRuntime CreateFromConfiguration()
        public static ScriptRuntime CreateRemote(AppDomain domain, ScriptRuntimeSetup setup)
    
        public ScriptScope ExecuteFile(string path);
        public ScriptScope Globals { get; set;}
        public ScriptScope CreateScope();
        public ScriptScope CreateScope(IAttributesCollection storage);
        
        public ScriptEngine GetEngine(string languageId);
        public ScriptEngine GetEngineByFileExtension(string extension);
        public string[] GetRegisteredFileExtensions();
        public string[] GetRegisteredLanguageIdentifiers();
    
        public void LoadAssembly(Assembly assm);
    
        public ObjectOperations Operations { get; }
        public ObjectOperations CreateOperations() { }
    
        public ScriptRuntimeSetup Setup { get {} }
        public ScriptHost Host { get; }
        public ScriptIO IO { get; }
        public void Shutdown()
    }
    

ScriptRuntime Members
=====================

.. ctype:: ScriptRuntime(ScriptRuntimeSetup setup)

    The constructor requires a ScriptRuntimeSetup, which gives the host full control of the languages allowed in the ScriptRuntime, their options, and the global runtime options.
    
    This method ensures the list of languages in the setup object has no duplicate elements based on the LanguageSetup.TypeName property (just comparing them as strings at this point).  Later, when engines fault in, the DLR also ensures that none of the assembly-qualified types actually identify the same type.
    
    This method ensures the list of languages in the setup have no conflicting LangaugeSetup.Names elements or Language.FileExtensions elements.
    After calling this method, modifying the ScriptRuntimeSetup object throws an exception.

.. cfunction:: ScriptRuntime CreateFromConfiguration()
.. cfunction:: ScriptRuntime CreateRemote(AppDomain domain, ScriptRuntimeSetup setup)

    These factory methods construct and return ScriptRuntimes.  They primarily are for convenience and discoverability via editors that complete members on types.
    
    CreateFromConfiguration is just a convenience for:
        new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration())
    
    CreateRemote creates the ScriptRuntime in the specified domain, instantiates the ScriptRuntimeSetup.HostType in that domain, and returns the ScriptRuntime.  Any arguments specified in ScriptRuntimeSetup.HostArguments must derive from MBRO or serialize across app domain boundaries.  The same holds for any values in ScriptRuntimeSetup.Options and any LanguageSetup.Options.

.. cfunction:: ScriptScope ExecuteFile(string path);

    This method executes the source identified in the path argument and returns the new ScriptScope in which the source executed.  This method calls on the ScriptRuntime.Host to get the PlatformAdaptationLayer and then calls on it to resolve and open the path.   ExecuteFile determines the language engine to use from the path's extension and the ScriptRuntime's configuration, comparing extensions case-insensitively.
    
    This convenience method exists for Level 1 hosting scenarios where the path is likely an absolute pathname or a filename that naturally resolves with standard .NET BCL file open calls.
    
    Each time this method is called it create a fresh ScriptScope in which to run the source.  Calling Engine.GetScope returns the last ScriptScope created for repeated invocations of ExecuteFile on the same path.
    
    This method adds variable name bindings within the ScriptRuntime.Globals object as appropriate for the language.  Dynamic language code can then access and drill into objects bound to those names.  For example, the IronPython loader adds the base file name to the ScriptRuntime.Globals as a Python module, and when IronPython is importing names, it looks in ScriptRuntime.Globals to find names to import.  IronRuby's loader adds constants and modules to the ScriptRuntime.Globals object.  DLR JScript adds all globals there.
    
    In Globals, each language decides its own semantics for name conflicts, but the expected model is last-writer-wins.  Languages do have the ability to add names to Globals so that only code executing in that language can see the global names.  In this case, other languages would not have the ability to clobber the name bindings.  For example, Python might do this for its special built-in modules.  However, most names should be added so that all languages can see the bindings and interoperate with the objects bound to the names.

.. cfunction:: ScriptScope Globals { get; set; }

    This property returns the "global object" or name bindings of the ScriptRuntime as a ScriptScope.  You can set the globals scope, which you might do if you created a ScriptScope with an IAttributesCollection so that your host could late bind names.

.. cfunction:: ScriptScope CreateScope();
.. cfunction:: ScriptScope CreateScope(IAttributesCollection storage);

    This method returns a new ScriptScope.
    
    The storage parameter lets you supply the dictionary of the scope so that you can provide late bound values for some name lookups.  If storage is null, this method throws an ArgumentNullException.

.. cfunction:: ScriptEngine GetEngine(string languageId);

    This method returns the one engine associated with this ScriptRuntime that matches the languageId argument, compared case-insensitively.  This loads the engine and initializes it if needed.
    
    If languageId is null, or it does not map to an engine in the ScriptRuntime's configuration, then this method throws an exception.

.. cfunction:: ScriptEngine GetEngineByFileExtension(string extension);

    This method takes a file extension and returns the one engine associated with this ScriptRuntime that matches the extension argument.  This strips one leading period if extension starts with a period.
    
    This loads the engine and initializes it if needed.  The file extension associations are determined by the ScriptRuntime configuration (see configuration section above).  This method compares extensions case-insensitively.

    If extension is null, or it does not map to an engine in the ScriptRuntime's configuration, then this method throws an exception.

.. cfunction:: string[] GetRegisteredFileExtensions();

    This method returns an array of strings (without periods) where each element is a registered file extension for this ScriptRuntime.  Each file extension maps to a language engine based on the ScriptRuntime configuration (see configuration section above).  If there are none, this returns an empty array.


.. cfunction:: string[] GetRegisteredLanguageIdentifiers();

    This method returns an array of strings where each element is a registered language identifier for this ScriptRuntime.  Each language identifier maps to a language engine based on the ScriptRuntime configuration (see configuration section above).  Typically all registered file extensions are also language identifiers.  If there are no language identifiers, this returns an empty array.

.. cfunction:: void LoadAssembly(Assembly assm);

    This method walks the assembly's namespaces and adds name bindings in ScriptRuntime.Globals to represent namespaces available in the assembly.  Each top-level namespace name becomes a name in Globals, bound to a dynamic object representing the namespace.  Within each top-level namespace object, the DLR binds names to dynamic objects representing each sub namespace or type.
    
    By default, the DLR seeds the ScriptRuntime with Mscorlib and System assemblies.  You can avoid this by setting the ScriptRuntimeSetup option "NoDefaultReferences" to true.
    
    When this method encounters the same fully namespace-qualified type name, it merges names together objects representing the namespaces.  If you called LoadAssembly on two different assemblies, each contributing to System.Foo.Bar namespace, then all names within System.Foo.Bar from both assemblies will be present in the resulting object representing Bar.

.. cfunction:: Operations

    This property returns a default, language-neutral ObjectOperations.  ObjectOperations lets you perform various operations on objects.  When the objects do not provide their own behaviors for performing the operations, this ObjectOperations uses general .NET semantics.  Because there are many situations when general .NET semantics are insufficient due to dynamic objects often not using straight .NET BCL types, this ObjectOperations will throw exceptions when one produced by a ScriptEngine would succeed.
    
    Because an ObjectOperations object caches rules for the types of objects and operations it processes, using the default ObjectOperations for many objects could degrade the caching benefits.  Eventually the cache for some operations could degrade to a point where ObjectOperations stops caching and does a full search for an implementation of the requested operation for the given objects.  For simple hosting situations, this is sufficient behavior.

.. cfunction:: ObjectOperations CreateOperations() { }

    These methods return a new ObjectOperations object.  See the Operations property for why you might want to call this and for limitations of ObjectOperations provided by a ScriptRuntime instead of one obtained from a ScriptEngine.
    
    There currently is little guidance on how to choose when to create new ObjectOperations objects.  However, there is a simple heuristic.  If you were to perform some operations over and over on the same few types of objects, it would be advantageous to create an ObjectOperations just for use with those few cases.  If you perform different operations with many types of objects just once or twice, you can use the default instance provided by the ObjectOperations property.

.. cfunction:: ScriptRuntimeSetup Setup { get {} }

    This property returns a read-only ScriptRuntimeSetup object describing the configuration information used to create the ScriptRuntime.

.. cfunction:: ScriptHost Host { get; }

    This property returns the ScriptHost associated with the ScriptRuntime.  This is not settable because the ScriptRuntime must create the host from a supplied type to support remote ScriptRuntime creation.  Setting it would also be bizarre because it would be similar to changing the owner of the ScriptRuntime.

.. cfunction:: ScriptIO IO { get; }

    This property returns the ScriptIO associated with the ScriptRuntime.  The ScriptIO lets you control the standard input and output streams for code executing in the ScriptRuntime.

.. cfunction:: void Shutdown()
    
    This method announces to the language engines that are loaded that the host is done using the ScriptRuntime.  Languages that have a shutdown hook or mechanism for code to release system resources on shutdown will invoke their shutdown protocols.
    
    There are no other guarantees from this method.  For example, It is undefined when code executing (possibly on other threads) will stop running.  Also, any calls on the ScriptRuntime, hosting API objects associated with the runtime, or dynamic objects extracted from the runtime have undefined behavior.
