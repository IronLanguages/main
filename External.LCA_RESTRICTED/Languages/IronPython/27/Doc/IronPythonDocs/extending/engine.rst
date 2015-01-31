.. highlightlang:: c


.. hosting-engines:

************
ScriptEngine
************

ScriptEngines represent a language implementation in the DLR, and they are the work horse for intermediate and advanced hosting scenarios.  ScriptEngines offer various ways to execute code and create ScriptScopes and ScriptSources.  ScriptSources offer methods for executing code in various ways from different kinds of sources.  ScriptEngines offer the more common or convenience methods for executing code.

There are several members on ScriptEngine for working with ScriptScope variables.  These give you the engine's specific language view of variable operations in any scope, regardless of the scope's associated language, if any.  

There is only one instance of a ScriptEngine for a given language in a given ScriptRuntime.  You get to engines with ScriptRuntime's methods or the Engine property of ScriptScope.

Note, members that take or return ObjectHandles are not present on Silverlight.

ScriptEngine Overview::

    public class ScriptEngine : MarshalByRefObject {
        public ScriptRuntime Runtime { get; }
        public string LanguageDisplayName { get; }
        public string[] GetRegisteredIdentifiers();
        public string[] GetRegisteredExtensions();
    
        public object Execute(string expression);
        public object Execute(string expression, ScriptScope scope);
        public T Execute<T>(string code);
        public T Execute<T>(string expression, ScriptScope scope)
        public ObjectHandle ExecuteAndWrap(string expression)
        public ObjectHandle ExecuteAndWrap(string expression, ScriptScope scope)
        public ScriptScope ExecuteFile(string path)
        public ScriptScope ExecuteFile(string path, ScriptScope scope) 
    
        public ScriptScope GetScope(string path);
    
        public ObjectOperations Operations { get; }
        public ObjectOperations CreateOperations();
        public ObjectOperations CreateOperations(ScriptScope Scope);
    
        public ScriptSource CreateScriptSourceFromString(string expression);
        public ScriptSource CreateScriptSourceFromString(string expression, string path);
        public ScriptSource CreateScriptSourceFromString(string code, SourceCodeKind kind);
        public ScriptSource CreateScriptSourceFromString(string code, string path, SourceCodeKind kind);
    
        public ScriptSource CreateScriptSourceFromFile(string path);
        public ScriptSource CreateScriptSourceFromFile(string path, System.Text.Encoding encoding);
        public ScriptSource CreateScriptSourceFromFile(string path, System.Text.Encoding encoding, SourceCodeKind kind);
    
        public ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path)
        public ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path, System.Text.Encoding encoding)
        public ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path, System.Text.Encoding encoding, SourceCodeKind kind)
        public ScriptSource CreateScriptSource(TextContentProvider contentProvider, string path, SourceCodeKind kind)
    
        public ScriptSource CreateScriptSource(CodeObject content);
        public ScriptSource CreateScriptSource(CodeObject content, string path);
        public ScriptSource CreateScriptSource(CodeObject content, SourceCodeKind kind);
        public ScriptSource CreateScriptSource(System.CodeDom.CodeObject code, string path, SourceCodeKind kind);
    
        public ScriptScope CreateScope();
        public ScriptScope CreateScope(IAttributesCollection globals);
        public ScriptScope CreateScope(Object globals);
        
        public object GetVariable(ScriptScope scope, string name);
        public ObjectHandle GetVariableHandle(ScriptScope scope, string name);
        public bool RemoveVariable(ScriptScope scope, string name);
        public void SetVariable(ScriptScope scope, string name, object value);
        public void SetVariable(ScriptScope scope, string name, ObjectHandle value);
        public bool TryGetVariable(ScriptScope scope, string name, out object value);
        public bool TryGetVariableHandle(ScriptScope scope, string name, out ObjectHandle value);
        public T GetVariable<T>(ScriptScope scope, string name);
        public bool TryGetVariable<T>(ScriptScope scope, string name, out T value);
        public bool ContainsVariable(ScriptScope scope, string name);
    
        public ServiceType GetService<ServiceType>(params object[] args) where ServiceType : class;
    
        public LanguageSetup Setup { get; }
        public CompilerOptions GetCompilerOptions() {}
        public CompilerOptions GetCompilerOptions(ScriptScope scope) {}
    
        public ICollection<string> GetSourceSearchPaths() {}
        public void SetSearchPaths (ICollection<string> paths){}
        public System.Version LanguageVersion { get; }
    }

ScriptEngine Members
====================

.. ctype:: ScriptEngine

    ScriptEngine has no public constructors.  To get a ScriptEngine instance call GetEngine on a ScriptRuntime instance.

.. cfunction:: ScriptRuntime Runtime { get; }

    This property returns the ScriptRuntime for the context in which this engine executes.

.. cfunction:: string LanguageDisplayName { get; }

    This property returns a display name for the engine or language that is suitable for UI.

.. cfunction:: string[] GetRegisteredIdentifiers();
.. cfunction:: string[] GetRegisteredExtensions();

    These methods return unique identifiers for this engine and file extensions that map to this engine and its language.  This information comes from configuration data passed to ScriptRuntime.Create.
    
    Modifying the results of these methods has no effect on configuration of this engine.

.. cfunction::  object Execute(string expression);
.. cfunction:: object Execute(string expression, ScriptScope scope);
.. cfunction:: T Execute<T>(string expression);
.. cfunction:: T Execute<T>(string expression, ScriptScope scope)
.. cfunction:: ObjectHandle ExecuteAndWrap(string expression)
.. cfunction:: ObjectHandle ExecuteAndWrap(string expression, ScriptScope scope)

    These methods execute the strings as expressions and return a result in various ways.  There are complementary overloads that take a ScriptScope.  The overloads that do not take scopes create a new scope for each execution.  These methods throw the scope away and use it for side effects only, returning the result in the same way the complementary overload does.
    
    Execute<T> returns the result as the specified type, using the engine's Operations.ConvertTo<T> method.  If this method cannot convert to the specified type, then it throws a NotSupportedException.
    
    ExecuteAndWrap returns an ObjectHandle for use when the engine and/or scope are remote.                                       

.. cfunction:: ScriptScope ExecuteFile(string path)
.. cfunction:: ScriptScope ExecuteFile(string path, ScriptScope scope) 

    These methods execute the strings the contents of files and return the scope in which the string executed.  The overload  that does not take a ScriptScope creates a new one each time it is called.

.. cfunction:: ScriptScope GetScope(string path);

    This method returns the ScriptScope in which the specified path/source executed.  This method works in conjunction with LoadFile and language implementer APIs for loading dynamic language libraries (see LoadFile's side note).  The path argument needs to match a ScriptSource's Path property because it is the key to finding the ScriptScope.  Hosts need to make sure they create ScriptSources (see ScriptHost as well as methods on ScriptEngine) with their Path properties set appropriately (for example, resolving relative paths to canonical full pathnames, FileInfo.FullPath for standard .NET resolved paths).
    
    GetScope is primarily useful for tools that need to map files to their execution scopes when the tool did not create the scope.  For example, an editor and interpreter tool might execute a file, Foo, that imports or requires a file, Bar.  The editor end user might later open the Bar and want to execute expressions in its context.  The tool would need to find Bar's ScriptScope for setting the appropriate context in its interpreter window.  This method helps with this scenario.
    
    Languages may return null.  For example, Ruby's require expression executes a file's contents in the calling scope.  Since Ruby does not have a distinct scope in which the file executed in this case, they return null for such files.

.. cfunction:: ObjectOperations Operations { get; }

    This property returns a default ObjectOperations for the engine.  ObjectOperations lets you perform various operations on objects.  Because an ObjectOperations object caches rules for the types of objects and operations it processes, using the default ObjectOperations for many objects could degrade the caching benefits.  Eventually the cache for some operations could degrade to a point where ObjectOperations stops caching and does a full search for an implementation of the requested operation for the given objects.  For simple hosting situations, this is sufficient behavior.
    
    See CreateOperations for alternatives.

.. cfunction:: ObjectOperations CreateOperations();
.. cfunction:: ObjectOperations CreateOperations(ScriptScope Scope);

    These methods return a new ObjectOperations object.  See the Operations property for why you might want to call this.
    
    There currently is little guidance on how to choose when to create new ObjectOperations objects.  However, there is a simple heuristic.  If you were to perform some operations over and over on the same few types of objects, it would be advantageous to create an ObjectOperations just for use with those few cases.  If you perform different operations with many types of objects just once or twice, you can use the default instance provided by the ObjectOperations property.

    The overload that takes a ScriptScope supports pretty advanced or subtle scenarios.  It allows you to get an ObjectOperations that uses the execution context built up in a ScriptScope.  For example, the engine affiliated with the scope could be IronPython, and you could execute code that did an "import clr" or "from __future__ import true_division".  These change execution behaviors within that ScriptScope.  If you obtained objects from that scope or executing expressions in that scope, you may want to operate on those objects with the same execution behaviors; however, you generally do not need to worry about these subtleties for typical object interactions.

.. cfunction:: ScriptSource CreateScriptSourceFromString(string expression);
.. cfunction:: ScriptSource CreateScriptSourceFromString(string expression, string path);
.. cfunction:: ScriptSource CreateScriptSourceFromString(string code, SourceCodeKind kind);
.. cfunction:: ScriptSource CreateScriptSourceFromString(string code, string path, SourceCodeKind kind);

    These methods return ScriptSource objects from string contents.  These are factory methods for creating ScriptSources with this language binding.
    
    The default SourceCodeKind is AutoDetect.
    
    The ScriptSource's Path property defaults to null.  When path is non-null, if executing  the resulting ScriptSource would create a ScriptScope, then path should map to the ScriptScope via GetScope.

.. cfunction:: ScriptSource CreateScriptSourceFromFile(string path);
.. cfunction:: ScriptSource CreateScriptSourceFromFile(string path, System.Text.Encoding encoding);
.. cfunction:: ScriptSource CreateScriptSourceFromFile(string path, System.Text.Encoding encoding, SourceCodeKind kind);

    These methods return ScriptSource objects from file contents.  These are factory methods for creating ScriptSources with this language binding.  The path's extension does NOT have to be registered or valid for the engine.  This method does NOT go through the PlatformAdaptationLayer to open the file; it goes directly to the file system via .NET.
    
    The default SourceCodeKind is File.
    
    The ScriptSource's Path property will be the path argument, which needs to be in some canonical form according to the host if the host is using GetScope to find the source's execution context later.
    
    Creating the ScriptSource does not open the file.  Any exceptions that will be thrown on opening or reading the file happen when you use the ScriptSource to execute or compile the source.
    
    The encoding defaults to the platform encoding.

.. cfunction:: ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path)
.. cfunction:: ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path, System.Text.Encoding encoding)
.. cfunction:: ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string path, System.Text.Encoding encoding, SourceCodeKind kind)

    This is a factory method for creating a ScriptSources with this language binding.
    
    This version lets you supply binary (sequence of bytes) stream input.  This is useful when opening files that may contain language-specific encodings that are marked in the first few bytes of the file's contents.  There is a default StreamContentProvider used internally if you call CreateScriptSourceFromFile.  The encoding defaults to the platform encoding if the language doesn't recognize some other encoding (for example, one marked in the file's first few bytes).
    
.. cfunction:: ScriptSource CreateScriptSource(TextContentProvider contentProvider, string path, SourceCodeKind kind)

    This is a factory method for creating a ScriptSources with this language binding.
    
    This version lets you supply input from Unicode strings or stream readers.  This could be useful for implementing a TextReader over internal host data structures, such as an editor's text representation.
    
.. cfunction:: ScriptSource CreateScriptSource(System.CodeDom.CodeObject code, string path, SourceCodeKind kind)
.. cfunction:: ScriptSource CreateScriptSource(CodeObject content)
.. cfunction:: ScriptSource CreateScriptSource(CodeObject content, string path)
.. cfunction:: ScriptSource CreateScriptSource(CodeObject content, SourceCodeKind kind)
.. cfunction:: ScriptSource CreateScriptSource(System.CodeDom.CodeObject code, string path, SourceCodeKind kind)

    This is a factory method for creating a ScriptSources with this language binding.
    
    This version is for supporting language independent code generation.  The expected CodeDom support is extremely minimal for syntax-independent expression of semantics.  Languages may do more, but hosts should only expect CodeMemberMethod support, and only sub nodes consisting of the following:

    - CodeSnippetStatement
    - CodeSnippetExpression
    - CodePrimitiveExpression
    - CodeMethodInvokeExpression
    - CodeExpressionStatement (for holding MethodInvoke)

    This support exists primarily for ASP.NET pages that contain snippets of DLR languages, and these requirements were very limited.  When the CodeObject argument does not match this specification, you will get a type cast error, but if the language supports more options, you could get different errors per engine.
    
    The path argument in all cases is a unique ID that the host may use to retrieve the scope in which the source executes via Engine.GetScope.

.. cfunction:: ScriptScope CreateScope();
.. cfunction:: ScriptScope CreateScope(IAttributesCollection globals);
.. cfunction:: ScriptScope CreateScope(Object globals);

    This method returns a new ScriptScope with this engine as the default language for the scope.
    
    The globals parameter lets you supply the dictionary of the scope so that you can provide late bound values for some name lookups.  If globals is null, this method throws an ArgumentNullException.  If globals is not an IAttributesCollection, then it must implement IDynamicMetaObjectProvider; otherwise, this method throws an exception because language engines will not be able to reliably execute code in the scope.

.. cfunction:: object GetVariable(ScriptScope scope, string name);
.. cfunction:: ObjectHandle GetVariableHandle(ScriptScope scope,  string name);
.. cfunction:: T GetVariable<T>(ScriptScope scope, string name);

    These methods return the value of variables in the scope argument.  These methods are duals to the members on ScriptScope, and they look up variables with this engine's semantics, regardless of any language association in the ScriptScope.  For example, if this engine's language is case-insensitive or provides name mappings so that variables appear with naming conventions in this engine's language, these methods help you look up names with the behaviors of this engine's language.
    
    GetVariable<T> uses the IDynamicObject protocol to convert the resulting object to T if the object is an IDynamicObject.  It uses implicit conversions.  If you need an explicit conversion to T, you can use Engine.Operations.ExplicitConvertTo<T>.
    
    GetVariable<T> throws a NotSupportedException if the engine cannot perform the requested type conversion.
    
    If any argument is null, these throw an ArgumentNullException.

.. cfunction:: bool RemoveVariable(ScriptScope scope, string name);

    This method removes a variable from the scope, performing a language-specific lookup, and returns whether the variable existed and had a value in the scope when you called this method.  See "GetVariable* Methods" for why we offer this dual to ScriptScope's member.
    
    Some languages may refuse to remove some variables.  If the ObjectOperations' associated language has variables that cannot be removed, and name identifies such a variable, it is undefined what happens.  Languages vary on whether this is a no-op or exceptional.
    
    If either argument is null, this throws an ArgumentNullException.

.. cfunction:: void SetVariable(ScriptScope scope, string name, object value);
.. cfunction:: void SetVariable(ScriptScope scope, string name, ObjectHandle value);

    This method sets a variable's value in the scope, performing a language-specific mapping to store a value for name.  See "GetVariable* Methods" for why we offer this dual to ScriptScope's member.
    
    If the scope or name argument is null, these throw an ArgumentNullException.

.. cfunction:: bool TryGetVariable(ScriptScope scope, string name,  out object value);
.. cfunction:: bool TryGetVariableHandle(ScriptScope scope, string name, out ObjectHandle value);
.. cfunction:: bool TryGetVariable<T>(ScriptScope scope, string name, out T value);

    These methods try to get a variable's value from the scope, performing a language-specific lookup, and return a Boolean indicating success of the lookup.  When the method's result is false, then it assigns null to value.  See "GetVariable* Methods" for why we offer this dual to ScriptScope's member.
    
    GetVariable<T> uses the IDynamicObject protocol to convert the resulting object to T if the object is an IDynamicObject.  It uses implicit conversions.  If you need an explicit conversion to T, you can use Engine.Operations.TryExplicitConvertTo<T>.
    
    TryGetVariable<T> throws a NotSupportedException if the engine cannot perform the requested type conversion.
    
    TryGetVariableHandle is useful when the ScriptScope is remote so that you get back an ObjectHandle referring to the value.
    
    If the scope or name argument is null, these throw an ArgumentNullException.

.. cfunction:: bool ContainsVariable(ScriptScope scope, string name);

    This method returns whether the scope contains a binding for the variable name, performing a language-specific lookup.  See "GetVariable* Methods" for why we offer this dual to ScriptScope's member.

.. cfunction:: ServiceType GetService<ServiceType>(params object[] args) where ServiceType : class;

    This method returns a language-specific service.  It provides a point of extensibility for a language implementation to offer more functionality than the standard engine members discussed here.  If the specified service is not available, this returns null.
    
    The following are services expected to be supported:
    
    ExceptionOperations	This duplicates some members of Exception and can return a string in the style of this engine's language to describe the exception argument.
    
    TokenCategorizer 	This is for building tools that want to scan languages and get token info, such as colorization categories.
    
    This type will change and be spec'ed external to this document eventually, see the section below for this type.
    
    OptionsParser 	This can parse a command shell (cmd.exe) style command line string.  Hosts that are trying to be an interactive console or incorporate standard command line switches of a language's console can get the engine's command line parser.
    
    This is a place holder for DLR v2.  Its design will definitely change.  We have a big open issue to redesign language and DLR support for building interactive UI, interpreters, tools, etc., with some common support around command lines and consoles.
    
    CommandLine 	is a helper object for parsing and processing interactive console input, maintaining a history of input, etc.
    
    This is a place holder for DLR v2.  Its design will definitely change.  We have a big open issue to redesign language and DLR support for building interactive UI, interpreters, tools, etc., with some common support around command lines and consoles.
    
    ScriptConsole 	This is a helper object for the UI of an interpreter, how output is displayed and how we get input.  If the language does not implement a ScriptConsole, there is a default Console object they can return.
    This is a place holder for DLR v2.  Its design will definitely change.  We have a big open issue to redesign language and DLR support for building interactive UI, interpreters, tools, etc., with some common support around command lines and consoles.  Need to distinguish this and CommandLine.

.. cfunction:: LanguageSetup Setup { get; }

    This property returns a read-only LanguageSetup describing the configuration used to instantiate this engine.

.. cfunction:: CompilerOptions GetCompilerOptions() {}
.. cfunction:: CompilerOptions GetCompilerOptions(ScriptScope scope) {}

    This method returns the compiler options object for the engine's language.  The overload that takes a ScriptScope returns options that represent any accrued imperative options state from the scope (for example, "from futures import truedivision" in python).  To operate on the options before passing them to ScriptSource.Compile, for example, you may need to cast the result to the documented subtype of CompilerOptions 	for the language you're manipulating.
    
    If scope is null, this throws an ArgumentNullException.
    
    CompilerOptions type will likely change by the time the DLR Hosting APIs move into the .NET libraries, possibly becoming Dictionary<str,obj>.

.. cfunction:: ICollection<string> GetSearchPaths () {}

    This method returns the search paths used by the engine for loading files when a script wants to import or require another file of code.  These are also the paths used by ScriptRuntime.UseFile.
    
    These paths do not affect ScriptRuntime.ExecuteFile.  The ScriptHost's PlatformAdaptationLayer (or the default's direct use of .NET file APIs) controls partial file name resolution for ExecuteFile.

.. cfunction:: void SetSearchPaths (ICollection<string> paths){}

    This method sets the search paths used by the engine for loading files when a script wants to import or require another file of code.  Setting these paths affects ScriptRuntime.UseFile.
    
    These paths do not affect ScriptRuntime.ExecuteFile.  The ScriptHost's PlatformAdaptationLayer (or the default's direct use of .NET file APIs) controls partial file name resolution for ExecuteFile.
    
.. cfunction::  System.Version LanguageVersion { get; }

    This property returns the language's version.
