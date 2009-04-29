/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Security.Permissions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Linq.Expressions;
using System.Dynamic;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// ObjectOperations provide a large catalogue of object operations such as member access, conversions, 
    /// indexing, and things like addition.  There are several introspection and tool support services available
    /// for more advanced hosts.  
    /// 
    /// You get ObjectOperation instances from ScriptEngine, and they are bound to their engines for the semantics 
    /// of the operations.  There is a default instance of ObjectOperations you can share across all uses of the 
    /// engine.  However, very advanced hosts can create new instances.
    /// </summary>
    public sealed class ObjectOperations
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
        {

        private readonly DynamicOperations _ops;
        private readonly ScriptEngine _engine;

        // friend class: DynamicOperations
        internal ObjectOperations(DynamicOperations ops, ScriptEngine engine) {
            Assert.NotNull(ops);
            Assert.NotNull(engine);
            _ops = ops;
            _engine = engine;
        }
        
        public ScriptEngine Engine {
            get { return _engine; }
        }

#pragma warning disable 618

        #region Local Operations

        /// <summary>
        /// Returns true if the object can be called, false if it cannot.  
        /// 
        /// Even if an object is callable Call may still fail if an incorrect number of arguments or type of arguments are provided.
        /// </summary>
        public bool IsCallable(object obj) {
            return _ops.IsCallable(obj);
        }

        /// <summary>
        /// Invokes the provided object with the given parameters and returns the result.
        /// 
        /// The prefered way of calling objects is to convert the object to a strongly typed delegate 
        /// using the ConvertTo methods and then invoking that delegate.
        /// </summary>
        public object Invoke(object obj, params object[] parameters) {
            return _ops.Invoke(obj, parameters);
        }

        /// <summary>
        /// Invokes a member on the provided object with the given parameters and returns the result.
        /// </summary>
        public object InvokeMember(object obj, string memberName, params object[] parameters) {
            return _ops.InvokeMember(obj, memberName, parameters);
        }

        /// <summary>
        /// Creates a new instance from the provided object using the given parameters, and returns the result.
        /// </summary>
        public object CreateInstance(object obj, params object[] parameters) {
            return _ops.CreateInstance(obj, parameters);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Throws an exception if the member does not exist or is write-only.
        /// </summary>
        public object GetMember(object obj, string name) {
            return _ops.GetMember(obj, name);
        }

        /// <summary>
        /// Gets the member name from the object obj and converts it to the type T.  Throws an exception if the
        /// member does not exist, is write-only, or cannot be converted.
        /// </summary>
        public T GetMember<T>(object obj, string name) {
            return _ops.GetMember<T>(obj, name);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Returns true if the member is successfully retrieved and 
        /// stores the value in the value out param.
        /// </summary>
        public bool TryGetMember(object obj, string name, out object value) {
            return _ops.TryGetMember(obj, name, out value);
        }

        /// <summary>
        /// Returns true if the object has a member named name, false if the member does not exist.
        /// </summary>
        public bool ContainsMember(object obj, string name) {
            return _ops.ContainsMember(obj, name);
        }

        /// <summary>
        /// Removes the member name from the object obj.  
        /// </summary>
        public void RemoveMember(object obj, string name) {
            _ops.RemoveMember(obj, name);
        }

        /// <summary>
        /// Sets the member name on object obj to value.
        /// </summary>
        public void SetMember(object obj, string name, object value) {
            _ops.SetMember(obj, name, value);
        }

        /// <summary>
        /// Sets the member name on object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>(object obj, string name, T value) {
            _ops.SetMember<T>(obj, name, value);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Throws an exception if the member does not exist or is write-only.
        /// </summary>
        public object GetMember(object obj, string name, bool ignoreCase) {
            return _ops.GetMember(obj, name, ignoreCase);
        }

        /// <summary>
        /// Gets the member name from the object obj and converts it to the type T.  Throws an exception if the
        /// member does not exist, is write-only, or cannot be converted.
        /// </summary>
        public T GetMember<T>(object obj, string name, bool ignoreCase) {
            return _ops.GetMember<T>(obj, name, ignoreCase);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Returns true if the member is successfully retrieved and 
        /// stores the value in the value out param.
        /// </summary>
        public bool TryGetMember(object obj, string name, bool ignoreCase, out object value) {
            return _ops.TryGetMember(obj, name, ignoreCase, out value);
        }

        /// <summary>
        /// Returns true if the object has a member named name, false if the member does not exist.
        /// </summary>
        public bool ContainsMember(object obj, string name, bool ignoreCase) {
            return _ops.ContainsMember(obj, name, ignoreCase);
        }

        /// <summary>
        /// Removes the member name from the object obj.  
        /// </summary>
        public void RemoveMember(object obj, string name, bool ignoreCase) {
            _ops.RemoveMember(obj, name, ignoreCase);
        }

        /// <summary>
        /// Sets the member name on object obj to value.
        /// </summary>
        public void SetMember(object obj, string name, object value, bool ignoreCase) {
            _ops.SetMember(obj, name, value, ignoreCase);
        }

        /// <summary>
        /// Sets the member name on object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>(object obj, string name, T value, bool ignoreCase) {
            _ops.SetMember<T>(obj, name, value, ignoreCase);
        }

        /// <summary>
        /// Convers the object obj to the type T.
        /// </summary>
        public T ConvertTo<T>(object obj) {
            return _ops.ConvertTo<T>(obj);
        }

        /// <summary>
        /// Converts the object obj to the type type.
        /// </summary>
        public object ConvertTo(object obj, Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            return _ops.ConvertTo(obj, type);
        }

        /// <summary>
        /// Converts the object obj to the type T.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryConvertTo<T>(object obj, out T result) {
            return _ops.TryConvertTo<T>(obj, out result);
        }

        /// <summary>
        /// Converts the object obj to the type type.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryConvertTo(object obj, Type type, out object result) {
            return _ops.TryConvertTo(obj, type, out result);
        }

        /// <summary>
        /// Converts the object obj to the type T including explicit conversions which may lose information.
        /// </summary>
        public T ExplicitConvertTo<T>(object obj) {
            return _ops.ExplicitConvertTo<T>(obj);
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.
        /// </summary>
        public object ExplicitConvertTo(object obj, Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            return _ops.ExplicitConvertTo(obj, type);
        }

        /// <summary>
        /// Converts the object obj to the type T including explicit conversions which may lose information.
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo<T>(object obj, out T result) {
            return _ops.TryExplicitConvertTo<T>(obj, out result);
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.  
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo(object obj, Type type, out object result) {
            return _ops.TryExplicitConvertTo(obj, type, out result);
        }


        /// <summary>
        /// Performs a generic unary operation on the specified target and returns the result.
        /// </summary>
        public object DoOperation(ExpressionType operation, object target) {
            return _ops.DoOperation<object, object>(operation, target);
        }

        /// <summary>
        /// Performs a generic unary operation on the strongly typed target and returns the value as the specified type
        /// </summary>
        public TResult DoOperation<TTarget, TResult>(ExpressionType operation, TTarget target) {
            return _ops.DoOperation<TTarget, TResult>(operation, target);
        }

        /// <summary>
        /// Performs the generic binary operation on the specified targets and returns the result.
        /// </summary>
        public object DoOperation(ExpressionType operation, object target, object other) {
            return _ops.DoOperation<object, object, object>(operation, target, other);
        }

        /// <summary>
        /// Peforms the generic binary operation on the specified strongly typed targets and returns
        /// the strongly typed result.
        /// </summary>
        public TResult DoOperation<TTarget, TOther, TResult>(ExpressionType operation, TTarget target, TOther other) {
            return _ops.DoOperation<TTarget, TOther, TResult>(operation, target, other);
        }

        /// <summary>
        /// Performs addition on the specified targets and returns the result.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Add(object self, object other) {
            return DoOperation(ExpressionType.Add, self, other);
        }

        /// <summary>
        /// Performs subtraction on the specified targets and returns the result.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Subtract(object self, object other) {
            return DoOperation(ExpressionType.Subtract, self, other);
        }

        /// <summary>
        /// Raises the first object to the power of the second object.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Power(object self, object other) {
            return DoOperation(ExpressionType.Power, self, other);
        }

        /// <summary>
        /// Multiplies the two objects.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Multiply(object self, object other) {
            return DoOperation(ExpressionType.Multiply, self, other);
        }

        /// <summary>
        /// Divides the first object by the second object.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Divide(object self, object other) {
            return DoOperation(ExpressionType.Divide, self, other);
        }

        /// <summary>
        /// Performs modulus of the 1st object by the second object.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        public object Modulo(object self, object other) {
            return DoOperation(ExpressionType.Modulo, self, other);
        }
        /// <summary>
        /// Shifts the left object left by the right object.  Throws an exception if the
        /// operation cannot be performed.
        /// </summary>
        public object LeftShift(object self, object other) {
            return DoOperation(ExpressionType.LeftShift, self, other);
        }

        /// <summary>
        /// Shifts the left object right by the right object.  Throws an exception if the
        /// operation cannot be performed.
        /// </summary>
        public object RightShift(object self, object other) {
            return DoOperation(ExpressionType.RightShift, self, other);
        }

        /// <summary>
        /// Performs a bitwise-and of the two operands.  Throws an exception if the operation 
        /// cannot be performed.
        /// </summary>
        public object BitwiseAnd(object self, object other) {
            return DoOperation(ExpressionType.And, self, other);
        }

        /// <summary>
        /// Performs a bitwise-or of the two operands.  Throws an exception if the operation 
        /// cannot be performed.
        /// </summary>
        public object BitwiseOr(object self, object other) {
            return DoOperation(ExpressionType.Or, self, other);
        }

        /// <summary>
        /// Performs a exclusive-or of the two operands.  Throws an exception if the operation 
        /// cannot be performed.
        /// </summary>
        public object ExclusiveOr(object self, object other) {
            return DoOperation(ExpressionType.ExclusiveOr, self, other);
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is less than the right object.
        /// Throws an exception if hte comparison cannot be performed.
        /// </summary>
        public bool LessThan(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.LessThan, self, other));
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is greater than the right object.
        /// Throws an exception if hte comparison cannot be performed.
        /// </summary>
        public bool GreaterThan(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.GreaterThan, self, other));
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is less than or equal to the right object.
        /// Throws an exception if hte comparison cannot be performed.
        /// </summary>
        public bool LessThanOrEqual(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.LessThanOrEqual, self, other));
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is greater than or equal to the right object.
        /// Throws an exception if hte comparison cannot be performed.
        /// </summary>
        public bool GreaterThanOrEqual(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.GreaterThanOrEqual, self, other));
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is equal to the right object.
        /// Throws an exception if the comparison cannot be performed.
        /// </summary>
        public bool Equal(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.Equal, self, other));
        }

        /// <summary>
        /// Compares the two objects and returns true if the left object is not equal to the right object.
        /// Throws an exception if hte comparison cannot be performed.
        /// </summary>
        public bool NotEqual(object self, object other) {
            return ConvertTo<bool>(_ops.DoOperation<object, object, object>(ExpressionType.NotEqual, self, other));
        }

        /// <summary>
        /// Returns a string which describes the object as it appears in source code
        /// </summary>
        [Obsolete("Use Format method instead.")]
        public string GetCodeRepresentation(object obj) {
            return obj.ToString();
            //return _ops.DoOperation<object, string>(StandardOperators.CodeRepresentation, obj);
        }

        /// <summary>
        /// Returns a string representation of the object in a language specific object display format.
        /// </summary>
        public string Format(object obj) {
            return _ops.Format(obj);
        }

        /// <summary>
        /// Returns a list of strings which contain the known members of the object.
        /// </summary>
        public IList<string> GetMemberNames(object obj) {
            return _ops.GetMemberNames(obj);
        }

        /// <summary>
        /// Returns a string providing documentation for the specified object.
        /// </summary>
        public string GetDocumentation(object obj) {
            return _ops.GetDocumentation(obj);
        }

        /// <summary>
        /// Returns a list of signatures applicable for calling the specified object in a form displayable to the user.
        /// </summary>
        public IList<string> GetCallSignatures(object obj) {
            return _ops.GetCallSignatures(obj);
        }

        #endregion

        #region Obsolete Operations

        /// <summary>
        /// Calls the provided object with the given parameters and returns the result.
        /// 
        /// The prefered way of calling objects is to convert the object to a strongly typed delegate 
        /// using the ConvertTo methods and then invoking that delegate.
        /// </summary>
        [Obsolete("Use Invoke instead")]
        public object Call(object obj, params object[] parameters) {
            return _ops.Invoke(obj, parameters);
        }

                /// <summary>
        /// Performs a generic unary operation on the specified target and returns the result.
        /// </summary>
        [Obsolete]
        public object DoOperation(Operators op, object target) {
            ExpressionType newOp = GetLinqOp(op);

            return _ops.DoOperation<object, object>(newOp, target);
        }
        
        /// <summary>
        /// Performs a generic unary operation on the strongly typed target and returns the value as the specified type
        /// </summary>
        [Obsolete("Use ExpressionType overload instead")]
        public TResult DoOperation<TTarget, TResult>(Operators op, TTarget target) {
            return _ops.DoOperation<TTarget, TResult>(GetLinqOp(op), target);
        }

        [Obsolete]
        private static ExpressionType GetLinqOp(Operators op) {
            ExpressionType? newOp = null;
            switch (op) {
                case Operators.Positive: newOp = ExpressionType.UnaryPlus; break;
                case Operators.Negate: newOp = ExpressionType.Negate; break;
                case Operators.OnesComplement: newOp = ExpressionType.OnesComplement; break;
                case Operators.IsFalse: newOp = ExpressionType.IsFalse; break;
                case Operators.Decrement: newOp = ExpressionType.Decrement; break;
                case Operators.Increment: newOp = ExpressionType.Increment; break;
                default:
                    throw new InvalidOperationException(String.Format("Unrecognized shared operation: {0}", op));
            }
            return newOp.Value;
        }

        /// <summary>
        /// Performs modulus of the 1st object by the second object.  Throws an exception
        /// if the operation cannot be performed.
        /// </summary>
        [Obsolete("Use Modulo instead")]
        public object Modulus(object self, object other) {
            return Modulo(self, other);
        }

        /// <summary>
        /// Performs the generic binary operation on the specified targets and returns the result.
        /// </summary>
        [Obsolete("Use ExpressionType overload instead")]
        public object DoOperation(Operators op, object target, object other) {
            return _ops.DoOperation<object, object, object>(GetLinqBinaryOp(op), target, other);
        }

        [Obsolete]
        private static ExpressionType GetLinqBinaryOp(Operators op) {
            ExpressionType? newOp = null;
            switch (op) {
                case Operators.Add: newOp = ExpressionType.Add; break;
                case Operators.BitwiseAnd: newOp = ExpressionType.And; break;
                case Operators.Divide: newOp = ExpressionType.Divide; break;
                case Operators.ExclusiveOr: newOp = ExpressionType.ExclusiveOr; break;
                case Operators.Mod: newOp = ExpressionType.Modulo; break;
                case Operators.Multiply: newOp = ExpressionType.Multiply; break;
                case Operators.BitwiseOr: newOp = ExpressionType.Or; break;
                case Operators.Power: newOp = ExpressionType.Power; break;
                case Operators.RightShift: newOp = ExpressionType.RightShift; break;
                case Operators.LeftShift: newOp = ExpressionType.LeftShift; break;
                case Operators.Subtract: newOp = ExpressionType.Subtract; break;

                case Operators.Equals: newOp = ExpressionType.Equal; break;
                case Operators.GreaterThan: newOp = ExpressionType.GreaterThan; break;
                case Operators.GreaterThanOrEqual: newOp = ExpressionType.GreaterThanOrEqual; break;
                case Operators.LessThan: newOp = ExpressionType.LessThan; break;
                case Operators.LessThanOrEqual: newOp = ExpressionType.LessThanOrEqual; break;
                case Operators.NotEquals: newOp = ExpressionType.NotEqual; break;
                default:
                    throw new InvalidOperationException(String.Format("Unrecognized shared operation: {0}", op));
            }
            return newOp.Value;
        }

        /// <summary>
        /// Peforms the generic binary operation on the specified strongly typed targets and returns
        /// the strongly typed result.
        /// </summary>
        [Obsolete]
        public TResult DoOperation<TTarget, TOther, TResult>(Operators op, TTarget target, TOther other) {
            return _ops.DoOperation<TTarget, TOther, TResult>(GetLinqBinaryOp(op), target, other);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Calls the specified remote object with the specified remote parameters.
        /// 
        /// Though delegates are preferable for calls they may not always be usable for remote objects.
        /// </summary>
        [Obsolete("Use Invoke instead")]
        public ObjectHandle Call(ObjectHandle obj, params ObjectHandle[] parameters) {
            return Invoke(obj, parameters);
        }

        /// <summary>
        /// Calls the specified remote object with the local parameters which will be serialized
        /// to the remote app domain.
        /// </summary>
        [Obsolete("Use Invoke instead")]
        public ObjectHandle Call(ObjectHandle obj, params object[] parameters) {
            return Invoke(obj, parameters);
        }

        /// <summary>
        /// Performs the specified unary operator on the remote object.
        /// </summary>
        [Obsolete("Use the ExpressionType overload instead")]
        public object DoOperation(Operators op, ObjectHandle target) {
            return DoOperation(op, GetLocalObject(target));
        }

        /// <summary>
        /// Performs the specified binary operator on the remote object.
        /// </summary>
        [Obsolete("Use the ExpressionType enum instead")]
        public ObjectHandle DoOperation(Operators op, ObjectHandle target, ObjectHandle other) {
            return new ObjectHandle(DoOperation(op, GetLocalObject(target), GetLocalObject(other)));
        }

        /// <summary>
        /// Performs modulus on the 1st remote object by the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        [Obsolete("Use Modulo instead")]
        public ObjectHandle Modulus(ObjectHandle self, ObjectHandle other) {
            return Modulo(self, other);
        }
#endif
        #endregion

#pragma warning restore 618

        #region Remote APIs

#if !SILVERLIGHT
        // ObjectHandle overloads
        //

        /// <summary>
        /// Returns true if the remote object is callable.
        /// </summary>
        public bool IsCallable([NotNull]ObjectHandle obj) {
            return IsCallable(GetLocalObject(obj));
        }

        /// <summary>
        /// Invokes the specified remote object with the specified remote parameters.
        /// 
        /// Though delegates are preferable for calls they may not always be usable for remote objects.
        /// </summary>
        public ObjectHandle Invoke([NotNull]ObjectHandle obj, params ObjectHandle[] parameters) {
            ContractUtils.RequiresNotNull(parameters, "parameters");

            return new ObjectHandle(Invoke(GetLocalObject(obj), GetLocalObjects(parameters)));
        }

        /// <summary>
        /// Invokes the specified remote object with the local parameters which will be serialized
        /// to the remote app domain.
        /// </summary>
        public ObjectHandle Invoke([NotNull]ObjectHandle obj, params object[] parameters) {
            return new ObjectHandle(Invoke(GetLocalObject(obj), parameters));
        }

        public ObjectHandle Create([NotNull]ObjectHandle obj, [NotNull]params ObjectHandle[] parameters) {
            return new ObjectHandle(CreateInstance(GetLocalObject(obj), GetLocalObjects(parameters)));
        }

        public ObjectHandle Create([NotNull]ObjectHandle obj, params object[] parameters) {
            return new ObjectHandle(CreateInstance(GetLocalObject(obj), parameters));
        }

        /// <summary>
        /// Sets the remote object as a member on the provided remote object.
        /// </summary>
        public void SetMember([NotNull]ObjectHandle obj, string name, [NotNull]ObjectHandle value) {
            SetMember(GetLocalObject(obj), name, GetLocalObject(value));
        }

        /// <summary>
        /// Sets the member name on the remote object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>([NotNull]ObjectHandle obj, string name, T value) {
            SetMember<T>(GetLocalObject(obj), name, value);
        }

        /// <summary>
        /// Gets the member name on the remote object.  Throws an exception if the member is not defined or
        /// is write-only.
        /// </summary>
        public ObjectHandle GetMember([NotNull]ObjectHandle obj, string name) {
            return new ObjectHandle(GetMember(GetLocalObject(obj), name));
        }

        /// <summary>
        /// Gets the member name on the remote object.  Throws an exception if the member is not defined or
        /// is write-only.
        /// </summary>
        public T GetMember<T>([NotNull]ObjectHandle obj, string name) {
            return GetMember<T>(GetLocalObject(obj), name);
        }

        /// <summary>
        /// Gets the member name on the remote object.  Returns false if the member is not defined or
        /// is write-only.
        /// </summary>
        public bool TryGetMember([NotNull]ObjectHandle obj, string name, out ObjectHandle value) {
            object val;
            if (TryGetMember(GetLocalObject(obj), name, out val)) {
                value = new ObjectHandle(val);
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Tests to see if the member name is defined on the remote object.  
        /// </summary>
        public bool ContainsMember([NotNull]ObjectHandle obj, string name) {
            return ContainsMember(GetLocalObject(obj), name);
        }

        /// <summary>
        /// Removes the member from the remote object
        /// </summary>
        public void RemoveMember([NotNull]ObjectHandle obj, string name) {
            RemoveMember(GetLocalObject(obj), name);
        }

        /// <summary>
        /// Converts the remote object into the specified type returning a handle to
        /// the new remote object.
        /// </summary>
        public ObjectHandle ConvertTo<T>([NotNull]ObjectHandle obj) {
            return new ObjectHandle(ConvertTo<T>(GetLocalObject(obj)));
        }

        /// <summary>
        /// Converts the remote object into the specified type returning a handle to
        /// the new remote object.
        /// </summary>
        public ObjectHandle ConvertTo([NotNull]ObjectHandle obj, Type type) {
            return new ObjectHandle(ConvertTo(GetLocalObject(obj), type));
        }

        /// <summary>
        /// Converts the remote object into the specified type returning a handle to
        /// the new remote object. Returns true if the value can be converted,
        /// false if it cannot.
        /// </summary>
        public bool TryConvertTo<T>([NotNull]ObjectHandle obj, out ObjectHandle result) {
            T resultObj;
            if (TryConvertTo<T>(GetLocalObject(obj), out resultObj)) {
                result = new ObjectHandle(resultObj);
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Converts the remote object into the specified type returning a handle to
        /// the new remote object. Returns true if the value can be converted,
        /// false if it cannot.
        /// </summary>
        public bool TryConvertTo([NotNull]ObjectHandle obj, Type type, out ObjectHandle result) {
            object resultObj;
            if (TryConvertTo(GetLocalObject(obj), type, out resultObj)) {
                result = new ObjectHandle(resultObj);
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Converts the object obj to the type T including explicit conversions which may lose information.
        /// </summary>
        public ObjectHandle ExplicitConvertTo<T>([NotNull]ObjectHandle obj) {
            return new ObjectHandle(_ops.ExplicitConvertTo<T>(GetLocalObject(obj)));
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.
        /// </summary>
        public ObjectHandle ExplicitConvertTo([NotNull]ObjectHandle obj, Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            return new ObjectHandle(_ops.ExplicitConvertTo(GetLocalObject(obj), type));
        }

        /// <summary>
        /// Converts the object obj to the type T including explicit conversions which may lose information.
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo<T>([NotNull]ObjectHandle obj, out ObjectHandle result) {
            T outp;
            bool res = _ops.TryExplicitConvertTo<T>(GetLocalObject(obj), out outp);
            if (res) {
                result = new ObjectHandle(obj);
            } else {
                result = null;
            }
            return res;
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.  
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo([NotNull]ObjectHandle obj, Type type, out ObjectHandle result) {
            object outp;
            bool res = _ops.TryExplicitConvertTo(GetLocalObject(obj), type, out outp);
            if (res) {
                result = new ObjectHandle(obj);
            } else {
                result = null;
            }
            return res;
        }

        /// <summary>
        /// Unwraps the remote object and converts it into the specified type before
        /// returning it.
        /// </summary>
        public T Unwrap<T>([NotNull]ObjectHandle obj) {
            return ConvertTo<T>(GetLocalObject(obj));
        }

        /// <summary>
        /// Performs the specified unary operator on the remote object.
        /// </summary>
        public object DoOperation(ExpressionType op, [NotNull]ObjectHandle target) {
            return DoOperation(op, GetLocalObject(target));
        }

        /// <summary>
        /// Performs the specified binary operator on the remote object.
        /// </summary>
        public ObjectHandle DoOperation(ExpressionType op, ObjectHandle target, ObjectHandle other) {
            return new ObjectHandle(DoOperation(op, GetLocalObject(target), GetLocalObject(other)));
        }

        /// <summary>
        /// Adds the two remote objects.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle Add([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Add(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Subtracts the 1st remote object from the second.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle Subtract([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Subtract(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Raises the 1st remote object to the power of the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle Power([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Power(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Multiplies the two remote objects.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle Multiply([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Multiply(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Divides the 1st remote object by the 2nd. Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle Divide([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Divide(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Performs modulus on the 1st remote object by the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>        
        public ObjectHandle Modulo([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(Modulo(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Shifts the 1st remote object left by the 2nd remote object.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle LeftShift([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(LeftShift(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Shifts the 1st remote  object right by the 2nd remote object.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle RightShift([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(RightShift(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Performs bitwise-and on the two remote objects.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle BitwiseAnd([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(BitwiseAnd(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Performs bitwise-or on the two remote objects.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle BitwiseOr([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(BitwiseOr(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Performs exclusive-or on the two remote objects.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public ObjectHandle ExclusiveOr([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return new ObjectHandle(ExclusiveOr(GetLocalObject(self), GetLocalObject(other)));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is less than the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool LessThan([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return LessThan(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is greater than the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool GreaterThan([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return GreaterThan(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is less than or equal to the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool LessThanOrEqual([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return LessThanOrEqual(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is greater than or equal to than the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool GreaterThanOrEqual([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return GreaterThanOrEqual(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is equal to the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool Equal([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return Equal(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Compares the two remote objects and returns true if the 1st is not equal to the 2nd.  Throws an exception if the operation cannot be performed.
        /// </summary>
        public bool NotEqual([NotNull]ObjectHandle self, [NotNull]ObjectHandle other) {
            return NotEqual(GetLocalObject(self), GetLocalObject(other));
        }

        /// <summary>
        /// Returns a string representation of the object in a langauge specific object display format.
        /// </summary>
        public string Format([NotNull]ObjectHandle obj) {
            return Format(GetLocalObject(obj));
        }

        /// <summary>
        /// Returns a list of strings which contain the known members of the remote object.
        /// </summary>
        public IList<string> GetMemberNames([NotNull]ObjectHandle obj) {
            return GetMemberNames(GetLocalObject(obj));
        }

        /// <summary>
        /// Returns a string providing documentation for the specified remote object.
        /// </summary>
        public string GetDocumentation([NotNull]ObjectHandle obj) {
            return GetDocumentation(GetLocalObject(obj));
        }

        /// <summary>
        /// Returns a list of signatures applicable for calling the specified object in a form displayable to the user.
        /// </summary>
        public IList<string> GetCallSignatures([NotNull]ObjectHandle obj) {
            return GetCallSignatures(GetLocalObject(obj));
        }

        /// <summary>
        /// Helper to unwrap an object - in the future maybe we should validate the current app domain.
        /// </summary>
        private static object GetLocalObject([NotNull]ObjectHandle obj) {
            ContractUtils.RequiresNotNull(obj, "obj");

            return obj.Unwrap();
        }

        /// <summary>
        /// Helper to unwrap multiple objects
        /// </summary>
        private static object[] GetLocalObjects(ObjectHandle[] ohs) {
            Debug.Assert(ohs != null);

            object[] res = new object[ohs.Length];
            for (int i = 0; i < res.Length; i++) {
                res[i] = GetLocalObject(ohs[i]);
            }

            return res;
        }
#endif

        #endregion

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
