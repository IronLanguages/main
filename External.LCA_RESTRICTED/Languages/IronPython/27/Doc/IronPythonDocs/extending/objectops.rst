.. highlightlang:: c


.. hosting-objectops:

****************
ObjectOperations
****************

This utility class provides operations on objects.  The operations work on objects emanating from a ScriptRuntime or straight up .NET static objects.  The behaviors of this class are language-specific, depending on which language owns the instance you're using.

You get ObjectOperations objects from ScriptEngines.  The operations have a language-specific behavior determined by the engine from which you got the ObjectOperations object.  For example, calling GetMember on most objects to get the "__dict__" member using an ObjectOperations obtained from an IronPython ScriptEngine will return the object's dictionary of members.  However, using an ObjectOperations obtained from an IronRuby engine, would raise a member missing exception.

The reason ObjectOperations is a utility class that is not static is that the instances provide a context of caching for performing the operations.  If you were to perform several operations over and over on the same few objects, it would be advantageous to create a special ObjectOperations just for use with those few objects.  If you perform different operations with many objects just once or twice, you can use the default instance provided by the ScriptEngine.

Half of the methods do the same thing as their complement, one works with objects of type object while the other works with ObjectHandles.  We need the overloads for clear method selection and to allow for an ObjectHandle to be treated as object should that be interesting.

You obtain objectOperation objects from ScriptEngines' Operations property and CreateOperations method.

Note, members that take or return ObjectHandles are not present on Silverlight.

ObjectOperations Overview::

    public sealed class ObjectOperations : MarshalByRefobject {
        public ScriptEngine Engine { get; }
        public ObjectHandle Add(ObjectHandle self, ObjectHandle other);
        public object Add(object self, object other);
        public object BitwiseAnd(object self, object other);
        public ObjectHandle BitwiseAnd(ObjectHandle self, ObjectHandle other);
        public ObjectHandle BitwiseOr(ObjectHandle self, ObjectHandle other);
        public object BitwiseOr(object self, object other);
        public bool ContainsMember(ObjectHandle obj, String name);
        public bool ContainsMember(object obj, String name, bool ignoreCase);
        public bool ContainsMember(object obj, String name);
        public T ConvertTo<T>(object obj);
        public ObjectHandle ConvertTo<T>(ObjectHandle obj);
        public object ConvertTo(object obj, Type type);
        public ObjectHandle ConvertTo(ObjectHandle obj, Type type);
        public ObjectHandle CreateInstance(ObjectHandle obj, params ObjectHandle[] parameters);
        public ObjectHandle CreateInstance(ObjectHandle obj, params object[] parameters);
        public object CreateInstance(object obj, params object[] parameters);
        public object Divide(object self, object other);
        public ObjectHandle Divide(ObjectHandle self, ObjectHandle other);
        public object DoOperation(ExpressionType operation, object target);
        public TResult DoOperation<TTarget, TResult>(ExpressionType operation, TTarget target);
        public object DoOperation(ExpressionType operation, object target, object other);
        public TResult DoOperation<TTarget, TOther, TResult>(ExpressionType operation, TTarget target, TOther other);
        public object DoOperation(ExpressionType op, ObjectHandle target);
        public ObjectHandle DoOperation(ExpressionType op, ObjectHandle target, ObjectHandle other);
        public bool Equal(object self, object other);
        public bool Equal(ObjectHandle self, ObjectHandle other);
        public object ExclusiveOr(object self, object other);
        public ObjectHandle ExclusiveOr(ObjectHandle self, ObjectHandle other);
        public T ExplicitConvertTo<T>(object obj);
        public object ExplicitConvertTo(object obj, Type type);
        public ObjectHandle ExplicitConvertTo(ObjectHandle obj, Type type);
        public ObjectHandle ExplicitConvertTo<T>(ObjectHandle obj);
        public String Format(object obj);
        public String Format(ObjectHandle obj);
        public IList<System.String> GetCallSignatures(ObjectHandle obj);
        public IList<System.String> GetCallSignatures(object obj);
        public String GetDocumentation(object obj);
        public String GetDocumentation(ObjectHandle obj);
        public T GetMember<T>(ObjectHandle obj, String name);
        public T GetMember<T>(object obj, String name, bool ignoreCase);
        public object GetMember(object obj, String name);
        public object GetMember(object obj, String name, bool ignoreCase);
        public ObjectHandle GetMember(ObjectHandle obj, String name);
        public T GetMember<T>(object obj, String name);
        public IList<System.String> GetMemberNames(ObjectHandle obj);
        public IList<System.String> GetMemberNames(object obj);
        public bool GreaterThan(object self, object other);
        public bool GreaterThan(ObjectHandle self, ObjectHandle other);
        public bool GreaterThanOrEqual(object self, object other);
        public bool GreaterThanOrEqual(ObjectHandle self, ObjectHandle other);
        public ObjectHandle Invoke(ObjectHandle obj, params ObjectHandle[] parameters);
        public ObjectHandle Invoke(ObjectHandle obj, params object[] parameters);
        public object Invoke(object obj, params object[] parameters);
        public object InvokeMember(object obj, String memberName, params object[] parameters);
        public bool IsCallable(object obj);
        public bool IsCallable(ObjectHandle obj);
        public ObjectHandle LeftShift(ObjectHandle self, ObjectHandle other);
        public object LeftShift(object self, object other);
        public bool LessThan(object self, object other);
        public bool LessThan(ObjectHandle self, ObjectHandle other);
        public bool LessThanOrEqual(ObjectHandle self, ObjectHandle other);
        public bool LessThanOrEqual(object self, object other);
        public ObjectHandle Modulo(ObjectHandle self, ObjectHandle other);
        public object Modulo(object self, object other);
        public ObjectHandle Multiply(ObjectHandle self, ObjectHandle other);
        public object Multiply(object self, object other);
        public bool NotEqual(object self, object other);
        public bool NotEqual(ObjectHandle self, ObjectHandle other);
        public object Power(object self, object other);
        public ObjectHandle Power(ObjectHandle self, ObjectHandle other);
        public bool RemoveMember(object obj, String name);
        public bool RemoveMember(ObjectHandle obj, String name);
        public bool RemoveMember(object obj, String name, bool ignoreCase);
        public ObjectHandle RightShift(ObjectHandle self, ObjectHandle other);
        public object RightShift(object self, object other);
        public void SetMember(object obj, String name, object value, bool ignoreCase);
        public void SetMember(ObjectHandle obj, String name, ObjectHandle value);
        public void SetMember<T>(object obj, String name, T value, bool ignoreCase);
        public void SetMember<T>(object obj, String name, T value);
        public void SetMember<T>(ObjectHandle obj, String name, T value);
        public void SetMember(object obj, String name, object value);
        public ObjectHandle Subtract(ObjectHandle self, ObjectHandle other);
        public object Subtract(object self, object other);
        public bool TryConvertTo<T>(ObjectHandle obj, out ObjectHandle result);
        public bool TryConvertTo(object obj, Type type, out object result);
        public bool TryConvertTo<T>(object obj, out T result);
        public bool TryConvertTo(ObjectHandle obj, Type type, out ObjectHandle result);
        public bool TryExplicitConvertTo<T>(object obj, out T result);
        public bool TryExplicitConvertTo<T>(ObjectHandle obj, out ObjectHandle result);
        public bool TryExplicitConvertTo(ObjectHandle obj, Type type, out ObjectHandle result);
        public bool TryExplicitConvertTo(object obj, Type type, out object result);
        public bool TryGetMember(object obj, String name, bool ignoreCase, out object value);
        public bool TryGetMember(ObjectHandle obj, String name, out ObjectHandle value);
        public bool TryGetMember(object obj, String name, out object value);
        public T Unwrap<T>(ObjectHandle obj);
    }

ObjectOperations Members
========================

.. ctype:: ObjectOperations

    ObjectOperations has no public constructors.  To get an ObjectOperations access the Operations property or call CreateOperations on a ScriptEngine.

.. cfunction:: ScriptEngine Engine { get; }

    This property returns the engine bound to this ObjectOperations.  The engine binding provides the language context or semantics applied to each requested operation.

.. cfunction:: bool IsCallable(object obj)
.. cfunction:: bool IsCallable(ObjectHandle obj)

    These methods returns whether the object is callable.  Languages should return delegates when fetching the value of variables or executing expressions that result in callable objects.  However, sometimes you'll get objects that are callable, but they are not wrapped in a delegate.  Note, even if this method returns true, a call may fail due to incorrect number of arguments or incorrect types of arguments.

.. cfunction:: object Invoke(object obj, params object[] parameters)
.. cfunction:: ObjectHandle Invoke(ObjectHandle obj, params ObjectHandle[] parameters)
.. cfunction:: ObjectHandle Invoke(ObjectHandle obj, params object[] parameters)

    These methods invoke objects that are callable.  In general you should not need to call these methods.  Languages should return delegates when fetching the value of variables or executing expressions that result in callable objects.  However, sometimes you'll get objects that are callable, but they are not wrapped in a delegate. If you're calling an object multiple times, you can use ConvertTo to get a strongly typed delegate that you can call more efficiently.  You'll also need to use Invoke for objects that are remote.
    
    If any obj arguments are null, then these throw an ArgumentNullException.
    
.. cfunction:: object InvokeMember(object obj, string memberName, params object[] parameters)

    This method invokes the specified member name on the specified object.  
    
.. cfunction:: ObjectHandle CreateInstance(ObjectHandle obj, params ObjectHandle[] parameters)
.. cfunction:: ObjectHandle CreateInstance(ObjectHandle obj, params object[] parameters)
.. cfunction:: object CreateInstance(object obj, params object[] parameters)

    These methods create objects when the input object can be instantiated.
    
    If any obj arguments are null, then these throw an ArgumentNullException.
    
.. cfunction:: T GetMember<T>(ObjectHandle obj, String name)
.. cfunction:: T GetMember<T>(object obj, String name, bool ignoreCase)
.. cfunction:: object GetMember(object obj, String name)
.. cfunction:: object GetMember(object obj, String name, bool ignoreCase)
.. cfunction:: ObjectHandle GetMember(ObjectHandle obj, String name)
.. cfunction:: T GetMember<T>(object obj, String name)

    These methods return a named member of an object.
    
    The generic overloads do not modify obj to convert to the requested type.  If they cannot perform the requested conversion to the concrete type, then they throw a NotSupportedException.  You can use Unwrap<T> after ConvertTo<T> on ObjectHandle to get a local T for the result.
    
    If the specified member does not exist, or if it is write-only, then these throw exceptions.

.. cfunction:: bool TryGetMember(object obj, String name, bool ignoreCase, out object value)
.. cfunction:: bool TryGetMember(ObjectHandle obj, String name, out ObjectHandle value)
.. cfunction:: bool TryGetMember(object obj, String name, out object value)

    These methods try to get a named member of an object.  They return whether name was a member of obj and set the out value to name's value.  If the name was not a member of obj, then this method sets value to null.
    
    If obj or name is null, then these throw an ArgumentNullException.
    
.. cfunction:: bool ContainsMember(ObjectHandle obj, String name)
.. cfunction:: bool ContainsMember(object obj, String name, bool ignoreCase)
.. cfunction:: bool ContainsMember(object obj, String name)

    These methods return whether the name is a member of obj.

.. cfunction:: bool RemoveMember(object obj, String name)
.. cfunction:: bool RemoveMember(ObjectHandle obj, String name)
.. cfunction:: bool RemoveMember(object obj, String name, bool ignoreCase)

    These methods remove name from obj so that it is no longer a member of obj.  If the object or the language binding of this ObjectOperations allows read-only or non-removable members, and name identifies such a member, then it is undefined what happens.  Languages vary on whether this is a no-op or exceptional.
    
    If any arguments are null, then these throw an ArgumentNullException.

.. cfunction:: void SetMember(object obj, String name, object value, bool ignoreCase) 
.. cfunction:: void SetMember(ObjectHandle obj, String name, ObjectHandle value)
.. cfunction:: void SetMember<T>(object obj, String name, T value, bool ignoreCase)
.. cfunction:: void SetMember<T>(object obj, String name, T value)
.. cfunction:: void SetMember<T>(ObjectHandle obj, String name, T value)
.. cfunction:: void SetMember(object obj, String name, object value)

    These members set the value of a named member of an object.  There are generic overloads that can be used to avoid boxing values and casting of strongly typed members.
    
    If the object or the language binding of this ObjectOperations supports read-only members, and name identifies such a member, then these methods throw a NotSupportedException.
    
    If any arguments are null, then these throw an ArgumentNullException.

.. cfunction:: T ConvertTo<T>(object obj)
.. cfunction:: ObjectHandle ConvertTo<T>(ObjectHandle obj)
.. cfunction:: object ConvertTo(object obj, Type type)
.. cfunction:: ObjectHandle ConvertTo(ObjectHandle obj, Type type)

    These methods convert an object to the requested type using implicit conversions, and they do not modify obj.  Obj may be returned if it is already the requested type.  You can use Unwrap<T> after ConvertTo<T> on ObjectHandle to get a local T for the result.
    
    If any of the arguments is null, then these throw an ArgumentNullException.
    
    If these methods cannot perform the requested conversion, then they throw a NotSupportedException.

.. cfunction:: public T ExplicitConvertTo<T>(object obj)
.. cfunction:: object ExplicitConvertTo(object obj, Type type)
.. cfunction:: ObjectHandle ExplicitConvertTo(ObjectHandle obj, Type type)
.. cfunction:: ObjectHandle ExplicitConvertTo<T>(ObjectHandle obj)

    These methods convert an object to the requested type using explicit conversions, which may be lossy.  Otherwise these methods are the same as the ConvertTo* methods.

.. cfunction:: bool TryConvertTo<T>(ObjectHandle obj, out ObjectHandle result)
.. cfunction:: bool TryConvertTo(object obj, Type type, out object result)
.. cfunction:: bool TryConvertTo<T>(object obj, out T result)
.. cfunction:: bool TryConvertTo(ObjectHandle obj, Type type, out ObjectHandle result)

    These methods try to convert an object to the requested type using implicit conversions, and they do not modify obj.  They return whether they could perform the conversion and set the out result parameter.  If the methods could not perform the conversion, then they set result to null.
    
    You can use Unwrap<T> after calling overloads on ObjectHandle to get a local T for the result.
    
    If they cannot perform the conversion to the requested type, then they throw a NotSupportedException.  
    
    If obj is null, then these throw an ArgumentNullException.

.. cfunction:: bool TryExplicitConvertTo<T>(object obj, out T result)
.. cfunction:: bool TryExplicitConvertTo<T>(ObjectHandle obj, out ObjectHandle result)
.. cfunction:: bool TryExplicitConvertTo(ObjectHandle obj, Type type, out ObjectHandle result)
.. cfunction:: bool TryExplicitConvertTo(object obj, Type type, out object result)

    These methods try to convert an object to the request type using explicit conversions, which may be lossy.  Otherwise these methods are the same as TryConvertTo* methods.

.. cfunction:: T Unwrap<T>(ObjectHandle obj)

    This method unwraps the remote object reference, converting it to the specified type before returning it.  If this method cannot perform the requested conversion to the concrete type, then it throws a NotSupportedException.  If the requested T does not serialize back to the calling app domain, the CLR throws an exception.

.. cfunction:: string Format(object obj)
.. cfunction:: string Format(ObjectHandle obj)

    These methods return a string representation of obj that is parse-able by the language.  ConvertTo operations that request a string return a display string for the object that is not necessarily parse-able as input for evaluation.

.. cfunction:: IList<string> GetMemberNames(object obj)
.. cfunction:: IList<string> GetMemberNames(ObjectHandle obj)

    These methods return an array of all the member names that obj has explicitly, determined by the language associated with this ObjectOperations.  Computed or late bound member names may not be in the result.

.. cfunction:: string GetDocumentation(object obj)
.. cfunction:: string GetDocumentation(ObjectHandle obj)

    These methods return the documentation for obj.  When obj is a static .NET object, this returns xml documentation comment information associated with the DLL containing obj's type.  If there is no available documentation for the object, these return the empty string.  Some languages do not have documentation hooks for objects, in which case they return the empty string.

.. cfunction:: IList<string> GetCallSignatures(object obj)
.. cfunction:: IList<string> GetCallSignatures(ObjectHandle obj)

    These methods return arrays of stings, each one describing a call signature that obj supports.  If the object is not callable, these throw a NotSupportedException.

.. cfunction:: object DoOperation(ExpressionType operation, object target)
.. cfunction:: public TResult DoOperation<TTarget, TResult>(ExpressionType operation, TTarget target)
.. cfunction:: public object DoOperation(ExpressionType operation, object target, object other)
.. cfunction:: public TResult DoOperation<TTarget, TOther, TResult>(ExpressionType operation, TTarget target, TOther other)
.. cfunction:: public object DoOperation(ExpressionType op, ObjectHandle target)
.. cfunction:: public ObjectHandle DoOperation (ExpressionType op, ObjectHandle target, ObjectHandle other)

    These methods perform the specified unary and binary operations on the supplied target and other objects, returning the results.  If the specified operator cannot be performed on the object or objects supplied, then these throw an exception.  See the Expression Tree spec for information on the expected semantics of the operators.
    
    The Hosting APIs share the ExpressionType enum with Expression Trees and the dynamic object interop protocol to specify what operation to perform.  Most values overlap making a distinct enum just another concept to learn, but this enum contains values for operations used in Expression Trees that do not make sense when passed to this method (for example, Block, Try, and Throw).  These methods pass the operation to the language that created the ObjectOperations object, and the language handles the ExpressionType as it sees fit.  For example, IronPython only supports the following ExpressionType values:
    
.. cfunction:: object Add(object self, object other)
.. cfunction:: ObjectHandle Add(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Add, self, other)

.. cfunction:: object Subtract(object self, object other)
.. cfunction:: ObjectHandle Subtract(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Subtract, self, other)

.. cfunction:: object Power(object self, object other)
.. cfunction:: ObjectHandle Power(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Power, self, other)
    
.. cfunction:: object Multiply(object self, object other)
.. cfunction:: ObjectHandle Multiply(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Multiply, self, other)

.. cfunction:: object Divide(object self, object other)
.. cfunction:: ObjectHandle Divide(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Divide, self, other)

.. cfunction:: ObjectHandle Modulo(ObjectHandle self, ObjectHandle other)
.. cfunction:: object Modulo(object self, object other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Modulo, self, other)

.. cfunction:: object LeftShift(object self, object other)
.. cfunction:: ObjectHandle LeftShift(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.LeftShift, self, other)

.. cfunction:: object RightShift(object self, object other)
.. cfunction:: ObjectHandle RightShift(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.RightShift, self, other)

.. cfunction:: object BitwiseAnd(object self, object other) {
.. cfunction:: ObjectHandle BitwiseAnd(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.BitwiseAnd, self, other)
    
.. cfunction:: object BitwiseOr(object self, object other)
.. cfunction:: ObjectHandle BitwiseOr(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.BitwiseOr, self, other)

.. cfunction:: object ExclusiveOr(object self, object other)
.. cfunction:: ObjectHandle ExclusiveOr(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.ExclusiveOr, self, other)

.. cfunction:: bool Equal(object self, object other)
.. cfunction:: bool Equal(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Equal, self, other)

.. cfunction:: bool NotEqual(object self, object other)
.. cfunction:: bool NotEqual(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.Equal, self, other)

.. cfunction:: bool LessThan(object self, object other)
.. cfunction:: bool LessThan(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.LessThan, self, other)

.. cfunction:: bool LessThanOrEqual(ObjectHandle self, ObjectHandle other)
.. cfunction:: bool LessThanOrEqual(object self, object other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.LessThanOrEqual, self, other)


.. cfunction:: bool GreaterThan(object self, object other)
.. cfunction:: bool GreaterThan(ObjectHandle self, ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.GreaterThan, self, other)

.. cfunction:: bool GreaterThanOrEqual(object self, object other)
.. cfunction:: bool GreaterThanOrEqual(ObjectHandle self,  ObjectHandle other)

    These methods are convenience members that are equivalent to:
        DoOperation(ExpressionType.GreaterThanOrEqual, self, other)
