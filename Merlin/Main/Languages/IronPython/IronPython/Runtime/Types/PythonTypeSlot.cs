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
using System.Linq.Expressions;
using System.Runtime.InteropServices;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// A TypeSlot is an item that gets stored in a type's dictionary.  Slots provide an 
    /// opportunity to customize access at runtime when a value is get or set from a dictionary.
    /// </summary>
    [PythonType]
    public class PythonTypeSlot {
        /// <summary>
        /// Gets the value stored in the slot for the given instance. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal virtual bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = null;
            return false;
        }

        /// <summary>
        /// Gets the value stored in the slot for the given instance, bound to the instance if possible
        /// </summary>
        internal virtual bool TryGetBoundValue(CodeContext context, object instance, PythonType owner, out object value) {
            return TryGetValue(context, instance, owner, out value);
        }

        /// <summary>
        /// Sets the value of the slot for the given instance.
        /// </summary>
        /// <returns>true if the value was set, false if it can't be set</returns>
        internal virtual bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return false;
        }

        /// <summary>
        /// Deletes the value stored in the slot from the instance.
        /// </summary>
        /// <returns>true if the value was deleted, false if it can't be deleted</returns>
        internal virtual bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {            
            return false;
        }

        internal virtual bool IsAlwaysVisible {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets an expression which is used for accessing this slot.  If the slot lookup fails the error expression
        /// is used again.
        /// 
        /// The default implementation just calls the TryGetValue method.  Subtypes of PythonTypeSlot can override
        /// this and provide a more optimal implementation.
        /// </summary>
        internal virtual Expression/*!*/ MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, Expression/*!*/ error) {
            ParameterExpression tmp = Ast.Variable(typeof(object), "slotTmp");
            Expression call = Ast.Call(
                 typeof(PythonOps).GetMethod("SlotTryGetValue"),
                 codeContext,
                 AstUtils.Convert(AstUtils.WeakConstant(this), typeof(PythonTypeSlot)),
                 instance ?? AstUtils.Constant(null),
                 owner,
                 tmp
            );

            if (!GetAlwaysSucceeds) {
                call = Ast.Condition(
                    call,
                    tmp,
                    AstUtils.Convert(error, typeof(object))
                );
            } else {
                call = Ast.Block(call, tmp);
            }

            return Ast.Block(
                new ParameterExpression[] { tmp },
                call
            );
        }

        /// <summary>
        /// True if TryGetValue will always succeed, false if it may fail.
        /// 
        /// This is used to optimize away error generation code.
        /// </summary>
        internal virtual bool GetAlwaysSucceeds {
            get {
                return false;
            }
        }
        
        internal virtual bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return false;
        }

        public object __get__(CodeContext/*!*/ context, object instance, [Optional]object typeContext) {
            PythonType dt = typeContext as PythonType;

            object res;
            if (TryGetValue(context, instance, dt, out res))
                return res;

            throw PythonOps.AttributeErrorForMissingAttribute(dt == null ? "?" : dt.Name, Symbols.GetDescriptor);
        }

#if FALSE
        public void __set__(CodeContext/*!*/ context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetPythonType(instance).Name, Symbols.SetDescriptor);
            }
        }
#endif

        public void __delete__(CodeContext/*!*/ context, object instance) {
            if (!TryDeleteValue(context, instance, DynamicHelpers.GetPythonType(instance))) {
                throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetPythonType(instance).Name, Symbols.DeleteDescriptor);
            }
        }
    }
}
