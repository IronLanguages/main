.. highlightlang:: c


.. _extending-parsing:

**********************
Parsing and Tokenizing
**********************

.. ctype: Parser()

    The Parser class has no public constructors.  To create a Parser call the Parser.CreateParser factory method.

.. cfunction:: Parser CreateParser(CompilerContext context, PythonOptions options)

    Creates a new parser using the given compiler context and options.
    
.. cfunction:: PythonAst ParseFile(bool makeModule)
.. cfunction:: PythonAst ParseFile(bool makeModule, bool returnValue)

.. cfunction:: PythonAst ParseInteractiveCode(out ScriptCodeParseResult properties)

    Parse one or more lines of interactive input
    
    Returns null if input is not yet valid but could be with more lines

.. cfunction:: PythonAst ParseSingleStatement()
.. cfunction:: PythonAst ParseTopExpression()

.. cfunction:: static int GetNextAutoIndentSize(string text, int autoIndentTabWidth)

    Given the interactive text input for a compound statement, calculate what the indentation level of the next line should be

.. cfunction:: ErrorSink ErrorSink { get; set; }
.. cfunction:: ParserSink ParserSink { get; set; }
.. cfunction:: public int ErrorCode { get; }
.. cfunction:: void Reset(SourceUnit sourceUnit, ModuleOptions languageFeatures)
.. cfunction:: void Reset()
.. cfunction:: void Dispose()
