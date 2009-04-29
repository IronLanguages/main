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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Binding;

using Ast = System.Linq.Expressions.Expression;

namespace IronPython.Runtime.Types {
    [PythonType("field#")]
    public sealed class ReflectedField : PythonTypeSlot, ICodeFormattable {
        private readonly NameType _nameType;
        internal readonly FieldInfo/*!*/ _info;

        public ReflectedField(FieldInfo/*!*/ info, NameType nameType) {
            Debug.Assert(info != null);

            this._nameType = nameType;
            this._info = info;
        }

        public ReflectedField(FieldInfo/*!*/ info)
            : this(info, NameType.PythonField) {
        }

        #region Public Python APIs

        public FieldInfo Info {
            [PythonHidden]
            get { 
                return _info; 
            }
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public object GetValue(CodeContext context, object instance) {
            object value;
            if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out value)) {
                return value;
            }
            throw new InvalidOperationException("cannot get field");
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public void SetValue(CodeContext context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw new InvalidOperationException("cannot set field");
            }            
        }

        public void __set__(object instance, object value) {
            if (instance == null && _info.IsStatic) {
                DoSet(null, value);
            } else if (!_info.IsStatic) {
                DoSet(instance, value);
            } else {
                throw PythonOps.AttributeErrorForReadonlyAttribute(_info.DeclaringType.Name, SymbolTable.StringToId(_info.Name));
            }
        }

        [SpecialName]
        public void __delete__(object instance) {
            throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(_info.DeclaringType.Name, SymbolTable.StringToId(_info.Name));
        }

        public string __doc__ {
            get {
                return DocBuilder.DocOneInfo(_info);
            }
        }

        public PythonType FieldType {
            [PythonHidden]
            get {
                return DynamicHelpers.GetPythonTypeFromType(_info.FieldType);
            }
        }

        #endregion

        #region Internal APIs

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (_info.IsStatic) {
                    value = _info.GetValue(null);
                } else {
                    value = this;
                }
            } else {
                value = _info.GetValue(context.LanguageContext.Binder.Convert(instance, _info.DeclaringType));
            }

            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool CanOptimizeGets {
            get {
                return !_info.IsLiteral;
            }
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (ShouldSetOrDelete(owner)) {
                DoSet(context, instance, value);
                return true;
            }

            return false;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            // field is settable if it is not readonly
            return (_info.Attributes & FieldAttributes.InitOnly) == 0 && !_info.IsLiteral;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            if (ShouldSetOrDelete(owner)) {
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(_info.DeclaringType.Name, SymbolTable.StringToId(_info.Name));
            }
            return false;
        }

        internal override bool IsAlwaysVisible {
            get {
                return _nameType == NameType.PythonField;
            }
        }

        internal override void MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, ConditionalBuilder/*!*/ builder) {
            if (!_info.IsPublic || _info.DeclaringType.ContainsGenericParameters) {
                // fallback to reflection
                base.MakeGetExpression(binder, codeContext, instance, owner, builder);
            } else if (instance == null) {
                if (_info.IsStatic) {
                    builder.FinishCondition(AstUtils.Convert(Ast.Field(null, _info), typeof(object)));
                } else {
                    builder.FinishCondition(Ast.Constant(this));
                }
            } else {
                builder.FinishCondition(
                    AstUtils.Convert(
                        Ast.Field(
                            binder.ConvertExpression(
                                instance,
                                _info.DeclaringType,
                                ConversionResultKind.ExplicitCast,
                                codeContext
                            ),
                            _info
                        ),
                        typeof(object)
                    )
                );
            }
        }

        #endregion

        #region Private helpers

        private void DoSet(CodeContext context, object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw new ArgumentException(String.Format("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", _info.Name, _info.DeclaringType.Name));
            if (_info.IsInitOnly || _info.IsLiteral)
                throw new MissingFieldException(String.Format("Cannot set field {1} on type {0}", _info.DeclaringType.Name, SymbolTable.StringToId(_info.Name)));

            _info.SetValue(instance, context.LanguageContext.Binder.Convert(val, _info.FieldType));
        }

        private void DoSet(object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw PythonOps.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", _info.Name, _info.DeclaringType.Name);
            if (_info.IsInitOnly || _info.IsLiteral)
                throw PythonOps.AttributeErrorForReadonlyAttribute(_info.DeclaringType.Name, SymbolTable.StringToId(_info.Name));

            _info.SetValue(instance, Converter.Convert(val, _info.FieldType));
        }

        private bool ShouldSetOrDelete(PythonType type) {
            PythonType dt = type as PythonType;

            // statics must be assigned through their type, not a derived type.  Non-statics can
            // be assigned through their instances.
            return (dt != null && _info.DeclaringType == dt.UnderlyingSystemType) || !_info.IsStatic || _info.IsLiteral || _info.IsInitOnly;
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<field# {0} on {1}>", _info.Name, _info.DeclaringType.Name);
        }

        #endregion
    }
}
