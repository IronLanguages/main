.. highlightlang:: c


.. hosting-scopes:

***********
ScriptScope
***********

This class represents a namespace essentially.  Hosts can bind variable names in ScriptScopes, fetch variable values, etc.  Hosts can execute code within scopes for distinct name bindings.

ScriptScopes also have some convenience members and an optional language affinity.  Scopes use the language to look up names and convert values.  If the ScriptScope has no default language, then these convenience methods throw an exception, and the Engine property returns null.

Hosts can store ScriptScopes as the values of names on ScriptRuntime.Globals or in other scopes.  When dynamic language code encounters a ScriptScope as an object, the DLR manifests the scope as a dynamic object.  This means that normal object member access sees the variables stored in the ScriptScope first.  Languages executing code that is doing the object member access get a chance to find members if the members are not variables in the ScriptScope.  The language might bind to the .NET static type members documented here.  They might detect the .NET type is ScriptScope and throw a missing member exception, use meta-programming hooks to give dynamic language code a chance to produce the member, or return sentinel objects according to the language's semantics.

Hosts can use ScriptScopes (regardless of whether they have a language affinity) to execute any kind of code within their namespace context.  ScriptEngine methods that execute code take a ScriptScope argument.  There are parallel methods on engines for getting and setting variables so that hosts can request a name lookup with any specific language's semantics in any ScriptScope.

You create instances of ScriptScopes using the CreateScope and ExecuteFile methods on ScriptRuntimes or CreateScope on ScriptEngine.

Note, members that take or return ObjectHandles are not present on Silverlight.

ScriptScope Overview::

    public class ScriptScope : MarshallByRefObject {
        public ScriptScope(ScriptEngine engine, Scope scope);
    
        public object GetVariable(string name);
        public ObjectHandle GetVariableHandle(string name);
        public bool RemoveVariable(string name);
        public void SetVariable(string name, object value);
        public void SetVariable(string name, ObjectHandle handle);
        public bool TryGetVariable(string name, out object value);
        public bool TryGetVariableHandle(string name, 
                                           out ObjectHandle handle);
        public T GetVariable<T>(string name);
        public bool TryGetVariable<T>(string name, out T value);
        public bool ContainsVariable(string name);
    
        public IEnumerable<string> GetVariableNames();
        public IEnumerable<KeyValuePair<string, object>> GetItems();
    
        public ScriptEngine Engine { get;}
    }

ScriptScope Members
===================

.. ctype:: ScriptScope(ScriptEngine engine, Scope scope)

Creates a new ScriptScope associated with the provided engine and whos local storage is scope.

.. cfunction:: object GetVariable(string name);
.. cfunction:: ObjectHandle GetVariableHandle(string name);
.. cfunction:: T GetVariable<T>(string name);

    These methods fetch the value of a variable stored in the scope.
    
    If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is a case-sensitive, literal lookup of the name in the scope's dictionary.  If there is a default engine, then the name lookup uses that language's semantics.
    
    GetVariableHandle is useful when the ScriptScope is remote so that you get back an ObjectHandle referring to the value.
    
    GetVariable<T> uses implicit conversion.  It throws a NotSupportedException if the engine cannot perform the requested type conversion.  If there is no associated engine, this method uses standard .NET conversion, which could throw an ArgumentException.
    
    If you need an explicit conversion to T, you can use scope.Engine.Operations.ExplicitConvertTo<T>.

.. cfunction:: void SetVariable(string name, object value);
.. cfunction:: void SetVariable(string name, ObjectHandle handle);
    
    These methods assign a value to a variable in the scope, overwriting any previous value.
    
    If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name mapping is a case-sensitive, literal mapping of the name in the scope's dictionary.  If there is a default engine, then the name lookup uses that language's semantics.

.. cfunction:: bool TryGetVariable(string name, out object value);
.. cfunction:: bool TryGetVariableHandle(string name, out ObjectHandle handle);
.. cfunction:: bool TryGetVariable<T>(string name, out T value);

    These methods fetch the value of a variable stored in the scope and return a Boolean indicating success of the lookup.  When the method's result is false, then it assigns null to value.
    
    If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is a case-sensitive, literal lookup of the name in the scope's dictionary.  If there is a default engine, then the name lookup uses that language's semantics.
    
    TryGetVariableHandle is useful when the ScriptScope is remote so that you get back an ObjectHandle referring to the value.
    
    TryGetVariable<T> uses implicit conversion.  It throws a NotSupportedException if the engine cannot perform the requested type conversion.  If there is no associated engine, this method uses standard .NET conversion, which could throw an ArgumentException.
    
    If you need an explicit conversion to T, you can use scope.Engine.Operations.TryExplicitConvertTo<T>.

.. cfunction:: bool ContainsVariable(string name);

    This method returns whether the variable is exists in this scope and has a value.
    
    If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, it is case-sensitive for example.  If there is a default engine, then the name lookup uses that language's semantics.
    

.. cfunction:: IEnumerable<string> GetVariableNames();

    This method returns an enumeration of strings, one string for each variable name in this scope.  If there are no names, then it returns an empty array.  Modifying the array has no impact on the ScriptScope.  This method returns a new instance for the result of each call.

.. cfunction:: IEnumerable<KeyValuePair<string, object>> GetItems ();

    This method returns an IEnumerable of variable name/value pairs, one for each variable name in this scope.  If there are no names, then the enumeration is empty.  Modifying the array has no impact on the ScriptScope.  This method returns a new instance for the result of each call, and modifying the scope while using the enumeration has undefined behavior.

.. cfunction:: bool RemoveVariable(string name);

    This method removes the variable name and returns whether the variable existed and had a value in the scope when you called this method.
    
    If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, it is case-sensitive for example.  If there is a default engine, then the name lookup uses that language's semantics.
    
    Some languages may refuse to remove some variables.  If the scope has an associated language that has variables that cannot be removed, and name identifies such a variable, it is undefined what happens.  Languages vary on whether this is a no-op or exceptional.

.. cfunction:: ScriptEngine Engine { get;}

    This property returns the engine associated with this scope.  If the scope was created without a language affinity, then this property returns null.
