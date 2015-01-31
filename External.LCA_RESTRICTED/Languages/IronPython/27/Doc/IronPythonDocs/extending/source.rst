.. highlightlang:: c


.. hosting-source:

************
ScriptSource
************

ScriptSource represents source code and offer a variety of ways to execute or compile the source.  You can get ScriptSources from factory methods on ScriptEngine, and ScriptSources are tied to the engine that created them.  The associated engine provides the execution and compilation semantics for the source.

ScriptSources have properties that direct the parsing of and report aspects of the source.  For example, the source could be marked as being an expression or a statement, for language that need to distinguish expressions and statements semantically for how to parse them.  The code could be marked as being interactive, which means the language's parser should handle standard interpreter affordances the language might support (for example, Python's "_" variable or VB's "?" syntax).

ScriptSources also have a Path property.  This is mostly useful for those marked as being a file.  The Path is the key for engines recognizing ScriptSources they have seen before so that they do not repeatedly load files when load-once semantics should apply (see ScriptEngine.LoadFile).  The Path also helps the engine find the ScriptScope the file executed in (see ScriptEngine.GetScope), which is useful for some tool host scenarios.  The host defines what a canonical representation of a path is.  The host needs to set the path to the same string when it intends for ScriptSources to match for the purposes of the above functions on ScriptEngine.

You can create ScriptSource objects with factory methods on ScriptEngine.

Note, members that take or return ObjectHandles are not present on Silverlight.

ScriptSource Overview::

    public sealed class ScriptSource : MarshalByRefObject {
        internal ScriptSource();
        public string Path { get; }
        public SourceCodeKind Kind { get;}
    
        public ScriptCodeParseResult GetCodeProperties();
        public ScriptCodeParseResult GetCodeProperties(CompilerOptions options);
    
        public ScriptEngine Engine { get; }
    
        public CompiledCode Compile();
        public CompiledCode Compile(ErrorListener sink);
        public CompiledCode Compile(CompilerOptions options);
        public CompiledCode Compile(CompilerOptions options, ErrorListener sink);
    
        public object Execute();
        public object Execute(ScriptScope scope);
        public ObjectHandle ExecuteAndWrap ();
        public ObjectHandle ExecuteAndWrap (ScriptScope scope);
        public T Execute<T>();
        public T Execute<T>(ScriptScope scope);
        public int ExecuteProgram();
    
        public ScriptCodeReader GetReader();
        public Encoding DetectEncoding();
    
        // line/file mapping:
        public string GetCode();
        public string GetCodeLine(int line);
        public string[] GetCodeLines(int start, int count);
        public SourceSpan MapLine(SourceSpan span);
        public SourceLocation MapLine(SourceLocation loc);
        public int MapLine(int line);
        public string MapLineToFile(int line);
    }
  
ScriptSource Members
====================

.. ctype:: ScriptSource

    ScriptSource has no public constructor.  A ScriptSource must be constructed by calling CreateScriptSource on a ScriptEngine.

.. cfunction:: string Path { get; }

    This property returns the identifier for this script source.  In many cases the Path doesn't matter.  It is mostly useful for file ScriptSources.  The Path is the key for engines to recognize ScriptSources they have seen before so that they do not repeatedly load files when load-once semantics should apply.  The Path also helps the engine find the ScriptScope the file executed in, which is useful for some tool host scenarios (see ScriptEngine.GetScope).
    
    The Path is null if not set explicitly on construction.  The path has the value the ScriptSource was created with.  In the case of relative file paths, for example, the DLR does not convert them to absolute or canonical representations.
    
.. cfunction:: SourceCodeKind Kind { get;}

    This property returns the kind of source this ScriptSource represents.  This property is a hint to the ScriptEngine how to parse the code ScriptSource (as an expression, statement, whole file, etc.).
    
    If you're unsure, File can be used to direct the language to generally parse the code.  For languages that are expression-based, they should interpret Statement as Expression.

.. cfunction:: ScriptCodeParseResult GetCodeProperties();
.. cfunction:: ScriptCodeParseResult GetCodeProperties(CompilerOptions options);

    This method returns the properties of the code to support tools.  The values indicate the state of parsing the source relative to completeness, or whether the source is complete enough to execute.

    CompilerOptions type will likely change by the time the DLR Hosting APIs move into the .NET libraries, possibly becoming Dictionary<str,obj>.

.. cfunction:: ScriptEngine Engine { get; }

    This property returns the language engine associated with this ScriptSource.  There is always a language tied to the source for convenience.  Also, we do not think it is useful to support having a piece of code that could perhaps be parsed by multiple languages.

.. cfunction:: CompiledCode Compile();
.. cfunction:: CompiledCode Compile(ErrorListener sink);
.. cfunction:: CompiledCode Compile(CompilerOptions options);
.. cfunction:: CompiledCode Compile(CompilerOptions options, ErrorListener sink);

    These methods compile the source and return a CompileCode object that can be executed repeatedly in its default scope or in other scopes without having to recompile the code.
    
    Each call to Compile returns a new CompiledCode object.  Each call to Compile always calls on its content provider to get sources, and the default file content provider always re-opens the file and reads its contents.
    
    If any arguments are null, these throw ArgumentNullExceptions.
    
    If you supply an error listener, and there were errors, these methods return null.  Otherwise, it leaves any raised exceptions unhandled.
    
    These methods do not take a ScriptScope to compile against.  That would prevent compilation from choosing optimized scope implementations.  You can always execute compiled code against any scope (see Execute* methods).
    
    CompilerOptions type will likely change by the time the DLR Hosting APIs move into the .NET libraries, possibly becoming Dictionary<str,obj>.

.. cfunction:: object Execute();
.. cfunction:: object Execute(ScriptScope scope);
.. cfunction:: ObjectHandle ExecuteAndWrap ();
.. cfunction:: ObjectHandle ExecuteAndWrap (ScriptScope scope);
.. cfunction:: T Execute<T>();
.. cfunction:: T Execute<T>(ScriptScope scope);
.. cfunction:: int ExecuteProgram();

    These methods execute the source code and return a result in various ways.  There are complementary overloads that take a ScriptScope and those that do not.  The overloads with no arguments create a new scope for each execution.  These methods throw the scope away and use it for side effects only, returning the result in the same way the complementary overload does.
    
    These methods always execute the ScriptSource.  Even when the source is a file, and the associated engine's language has an execute-at-most-once mechanism, these methods always execute the source contents.
    
    Each call to Execute always calls on its content provider to get sources, and the default file content provider always re-opens the file and reads its contents.
    
    Execute returns an object that is the resulting value of running the code.  When the ScriptSource is a file or statement, the language decides what is an appropriate value to return.  Some languages return the value produced by the last expression or statement, but languages that are not expression based may return null.
    
    ExecuteAndWrap returns an ObjectHandle for use when the engine and/or scope are remote.
    
    Execute<T> returns the result as the specified type, using the associated engine's Operations.ConvertTo<T> method.  If this method cannot convert to the specified type, then it throws an exception.
    
    ExecuteProgram runs the source as though it were launched from an OS command shell and returns a process exit code indicating the success or error condition of executing the code.  Each time this method is called it creates a fresh ScriptScope in which to run the source, and if you were to use ScriptEngine.GetScope, you'd get whatever last ScriptScope the engine created for the source.

.. cfunction:: ScriptCodeReader GetReader();

    This method returns a derived type of TextReader that is bound to this ScriptSource.  Every time you call this method you get a new ScriptCodeReader reset to beginning parsing state, and no two instances interfere with each other.

.. cfunction:: Encoding DetectEncoding() {}

    This method returns the encoding for the source.   The language associated with the source has the chance to read the beginning of the file if it has any special handling for encodings based on the first few bytes of the file.  This method could return an encoding different than what the source was created with.

.. cfunction:: string GetCode();

    This method returns all the source code contents as a string.  The result may share storage with the string passed to create the ScriptSource.
    
    Each call to GetCode always calls on its content provider to get sources, and the default file content provider always re-opens the file and reads its contents.

.. cfunction:: string GetCodeLine(int line);
.. cfunction:: string[] GetCodeLines(int start, int count);

    These methods return a string (or strings) for the line (or lines) indexed.  Count is one-based.  The count argument can be greater than the number of lines.  The start argument cannot be zero or negative.
    
    The line and count arguments can cause indexing to go beyond the end of the source.  GetCodeLine returns null in that case.  GetCodeLines returns strings only for existing lines and does not throw an exception or include nulls in the array.  If start is beyond the end, the result is an empty array.
    
.. cfunction:: SourceSpan MapLine(SourceSpan span);
.. cfunction:: SourceLocation MapLine(SourceLocation loc);
.. cfunction:: public int MapLine(int line);

    These methods map physical line numbers to virtual line numbers for reporting errors or other information to users.  These are useful for languages that support line number directives for their parsers and error reporting.
    
.. cfunction:: string MapLineToFile(int line);

    This method maps a physical line number to a .NET CLR pdb or file with symbol information in it.  The result is an absolute path or relative path that resolves in a standard .NET way to the appropriate file.
