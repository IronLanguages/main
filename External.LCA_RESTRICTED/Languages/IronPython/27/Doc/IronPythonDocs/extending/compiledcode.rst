.. highlightlang:: c


.. hosting-compiledcode:

************
CompiledCode
************

CompiledCode represents code that has been compiled to execute repeatedly without having to compile it each time, and it represents the default ScriptScope the code runs in.  The default scope may have optimized variable storage and lookup for the code.  You can always execute the code in any ScriptScope if you need it to execute in a clean scope each time, or you want to accumulate side effects from the code in another scope.

You can get CompiledCode from Compile methods on ScriptSource.  CompiledCode objects have an internal reference to the engine that produced them.  Because they have a default scope in which to execute, and the use for CompiledCode objects is to execute them, they have several execute methods.

Note, members that take or return ObjectHandles are not present on Silverlight.

CompiledCode Summary::

    public class CompiledCode : MarshallByRefObject {
        public ScriptScope DefaultScope { get; }
        public ScriptEngine Engine { get; }
        public object Execute() {}
        public object Execute(ScriptScope scope) { }
        public ObjectHandle ExecuteAndWrap() { }
        public ObjectHandle ExecuteAndWrap(ScriptScope scope) { }
        public T Execute<T>() { }
        public T Execute<T>(ScriptScope scope) { }
    }

CompiledCode Members
====================

.. ctype:: CompiledCode

    CompiledCode has no public constructors.  To create a CompiledCode call Compile on a ScriptSource object.

.. cfunction:: ScriptScope DefaultScope { get; }

    This property returns the default ScriptScope in which the code executes.  This allows you to extract variable values after executing the code or insert variable bindings before executing the code.

.. cfunction:: ScriptEngine Engine { get; }

    This property returns the engine that produced the compiled code.

.. cfunction:: object Execute() {}
.. cfunction:: object Execute(ScriptScope scope) { }
.. cfunction:: ObjectHandle ExecuteAndWrap() { }
.. cfunction:: ObjectHandle ExecuteAndWrap(ScriptScope scope) { }
.. cfunction:: T Execute<T>() { }
.. cfunction:: T Execute<T>(ScriptScope scope) { }

    These methods execute the compiled code in a variety of ways.  Half of the overloads do the same thing as their complement, one executes in the default scope while the other takes a ScriptScope in which to execute the code.  If invoked on null, this throws an ArgumentNullException.

    ExecuteAndWrap returns an ObjectHandle for use when the engine and/or scope are remote.

    Execute<T> returns the result as the specified type, using the engine's Operations.ConvertTo<T> method.  If this method cannot convert to the specified type, then it throws an exception.
