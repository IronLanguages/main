.. highlightlang:: c


.. hosting-exceptops:

*******************
ExceptionOperations
*******************

This class provides language-specific utilities for working with exceptions coming from executing code.  You access instances of this type from Engine.GetService.

ExceptionOperations Summary::

    public sealed class ExceptionOperations : MarshalByRefObject {
        public string FormatException(Exception exception);
        public void GetExceptionMessage(Exception exception, out string message, out string errorTypeName);
    
    }
    
    
ExceptionOperations Members
===========================

.. ctype:: ExceptionOperations

    ExceptionOperations objects cannot be directly created.  To get an ExceptionOperations instance call ScriptEngine.GetService<ExceptionOperations>();
    
