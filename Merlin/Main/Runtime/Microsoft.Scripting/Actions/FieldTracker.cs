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
using System.Reflection;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public class FieldTracker : MemberTracker {
        private readonly FieldInfo _field;

        public FieldTracker(FieldInfo field) {
            ContractUtils.RequiresNotNull(field, "field");
            _field = field;
        }

        public override Type DeclaringType {
            get { return _field.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Field; }
        }

        public override string Name {
            get { return _field.Name; }
        }

        public bool IsPublic {
            get {
                return _field.IsPublic;
            }
        }

        public bool IsInitOnly {
            get {
                return _field.IsInitOnly;
            }
        }

        public bool IsLiteral {
            get {
                return _field.IsLiteral;
            }
        }

        public Type FieldType {
            get {
                return _field.FieldType;
            }
        }

        public bool IsStatic {
            get {
                return _field.IsStatic;
            }
        }

        public FieldInfo Field {
            get {
                return _field;
            }
        }

        [Confined]
        public override string ToString() {
            return _field.ToString();
        }

        #region Public expression builders

        public override Expression GetValue(Expression context, ActionBinder binder, Type type) {
            if (Field.IsLiteral) {
                return Ast.Constant(Field.GetValue(null));
            }

            if (!IsStatic) {
                // return the field tracker...
                return binder.ReturnMemberTracker(type, this);
            }

            if (Field.DeclaringType.ContainsGenericParameters) {
                return null;
            }

            if (IsPublic && DeclaringType.IsPublic) {
                return Ast.Field(null, Field);
            }

            return Ast.Call(
                AstUtils.Convert(Ast.Constant(Field), typeof(FieldInfo)),
                typeof(FieldInfo).GetMethod("GetValue"),
                Ast.Constant(null)
            );
        }

        public override ErrorInfo GetError(ActionBinder binder) {
            // FieldTracker only has one error - accessing a static field from 
            // a generic type.
            Debug.Assert(Field.DeclaringType.ContainsGenericParameters);

            return binder.MakeContainsGenericParametersError(this);
        }

        #endregion

        #region Internal expression builders

        protected internal override Expression GetBoundValue(Expression context, ActionBinder binder, Type type, Expression instance) {
            if (IsPublic && DeclaringType.IsVisible) {
                return Ast.Field(
                    Ast.Convert(instance, Field.DeclaringType),
                    Field
                );
            }

            return DefaultBinder.MakeError(((DefaultBinder)binder).MakeNonPublicMemberGetError(context, this, type, instance));
        }

        public override MemberTracker BindToInstance(Expression instance) {
            if (IsStatic) return this;

            return new BoundMemberTracker(this, instance);
        }

        #endregion
    }
}
